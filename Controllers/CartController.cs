using BE.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http; // Still needed for general HttpContext access
using System.Text.Json;
using Microsoft.AspNetCore.Hosting; // Retained, though not used in core cart logic
using System.Globalization;
using BE.Extensions; // Needed for SessionExtensions (though cart won't use it anymore for core cart logic)
using System.Text.RegularExpressions;
using BE.Services;
using Microsoft.AspNetCore.Identity; // Added for UserManager
using Microsoft.AspNetCore.Authorization; // Added for [Authorize]
using BE.Data; // Added for BEContext
using System.Threading.Tasks; // Added for async operations
using Microsoft.EntityFrameworkCore; // Added for ToListAsync, FirstOrDefaultAsync
using BE.Areas.Identity.Data; // Added for BEUser

namespace BE.Controllers
{
    [Authorize] // All cart actions now require authentication
    public class CartController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ProductService _productService;
        private readonly BEContext _context; // Injected BEContext for database access
        private readonly UserManager<BEUser> _userManager; // Injected UserManager to get current user's ID

        // Constructor to inject all necessary services
        public CartController(IWebHostEnvironment env, ProductService productService, BEContext context, UserManager<BEUser> userManager)
        {
            _env = env;
            _productService = productService;
            _context = context;
            _userManager = userManager;
        }

        // GET: Cart/Index - Displays the main cart page
        public async Task<IActionResult> Index() // Made async to support database operations
        {
            var userId = _userManager.GetUserId(User); // Get the logged-in user's ID
            // Fetch cart items for this user from the database
            var cartItems = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();

            var productCatalog = _productService.GetProductCatalog(); // Get the full product catalog for details

            // Populate product details (name, image, price) for items in cart from the catalog
            foreach (var item in cartItems)
            {
                if (productCatalog.TryGetValue(item.ProductId, out ProductViewModel? productDetails))
                {
                    item.ProductName = productDetails.ProductName;
                    item.ImageUrl = productDetails.MainImage;
                    item.Price = _productService.ParsePrice(productDetails.Price); // Ensure price is always parsed consistently from latest product data
                }
                else
                {
                    // Handle case where product might no longer exist in catalog
                    // (e.g., product was removed from JSON after being added to cart)
                    item.ProductName = item.ProductName + " (Unavailable)";
                    item.Price = 0m; // Default to 0 price if product not found
                    item.ImageUrl = "/images/placeholder.png"; // Fallback image
                }
            }

            var cartViewModel = new CartViewModel
            {
                Items = cartItems,
                // Message from TempData will still work if RedirectToAction was used elsewhere for non-AJAX
                Message = TempData["CartMessage"] as string
            };
            return View(cartViewModel);
        }

        // POST: Cart/AddToCart - Handles adding a product to the cart (via AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken] // Protects against Cross-Site Request Forgery attacks
        public async Task<IActionResult> AddToCart(int productId) // Made async
        {
            // Defensive check, although [Authorize] should cover this
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "You must be logged in to add items to your cart." });
            }

            var productCatalog = _productService.GetProductCatalog();
            if (!productCatalog.TryGetValue(productId, out ProductViewModel? productToAdd))
            {
                return Json(new { success = false, message = "Error: Product not found." });
            }

            var userId = _userManager.GetUserId(User);

            string feedbackMessage;
            try
            {
                // Check if item already exists in the cart for this specific user
                var existingItem = await _context.CartItems.FirstOrDefaultAsync(item => item.ProductId == productId && item.UserId == userId);

                if (existingItem != null)
                {
                    existingItem.Quantity++; // Increment quantity if item already exists
                    feedbackMessage = $"Quantity for '{productToAdd.ProductName}' increased to {existingItem.Quantity}.";
                }
                else
                {
                    // Parse the product's price using the ProductService's robust method
                    decimal price = _productService.ParsePrice(productToAdd.Price);

                    // Add new CartItem to the database context
                    _context.CartItems.Add(new CartItem
                    {
                        ProductId = productId,
                        ProductName = productToAdd.ProductName,
                        Price = price,
                        Quantity = 1,
                        ImageUrl = productToAdd.MainImage,
                        UserId = userId // Associate the cart item with the current user
                    });
                    feedbackMessage = $"'{productToAdd.ProductName}' added to cart.";
                    if (price == 0m && productToAdd.Price.Trim().ToUpper() != "N/A")
                    {
                        feedbackMessage += " (Price could not be parsed, defaulted to R0.00)";
                    }
                }

                await _context.SaveChangesAsync(); // Persist changes to the database
            }
            catch (DbUpdateException ex)
            {
                // This exception occurs if the unique constraint is violated (i.e., a duplicate was attempted)
                // This often happens in race conditions where FirstOrDefaultAsync didn't find the item,
                // but another concurrent request successfully added it before SaveChangesAsync.
                if (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601))
                {
                    // SQL Server specific error codes for unique constraint violation
                    // Attempt to re-fetch and update if a concurrency issue occurred
                    var existingItemAfterConflict = await _context.CartItems.FirstOrDefaultAsync(item => item.ProductId == productId && item.UserId == userId);
                    if (existingItemAfterConflict != null)
                    {
                        existingItemAfterConflict.Quantity++;
                        await _context.SaveChangesAsync(); // Try saving the update
                        feedbackMessage = $"Quantity for '{productToAdd.ProductName}' increased to {existingItemAfterConflict.Quantity} (resolved concurrency).";
                        return Json(new { success = true, message = feedbackMessage });
                    }
                    else
                    {
                        // Should not happen if it's a unique constraint violation, but as a fallback
                        return Json(new { success = false, message = "Failed to add item due to a concurrency conflict. Please try again." });
                    }
                }
                else
                {
                    // Log other database update exceptions
                    Console.Error.WriteLine($"Database update error: {ex.Message}");
                    return Json(new { success = false, message = $"An unexpected database error occurred: {ex.Message}" });
                }
            }
            catch (Exception ex)
            {
                // Catch any other unexpected errors
                Console.Error.WriteLine($"An unexpected error in AddToCart: {ex.Message}");
                return Json(new { success = false, message = $"An unexpected error occurred: {ex.Message}" });
            }

            return Json(new { success = true, message = feedbackMessage });
        }

        // POST: Cart/RemoveFromCart - Handles removing a product from the cart (via AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromCart(int productId) // Made async
        {
            var userId = _userManager.GetUserId(User);
            // Find the item for the current user and product ID
            var itemToRemove = await _context.CartItems.FirstOrDefaultAsync(item => item.ProductId == productId && item.UserId == userId);

            if (itemToRemove == null)
            {
                return Json(new { success = false, message = "Item not found in cart." });
            }

            _context.CartItems.Remove(itemToRemove); // Mark item for removal
            await _context.SaveChangesAsync(); // Persist changes

            return Json(new { success = true, message = $"'{itemToRemove.ProductName}' removed from cart." });
        }

        // POST: Cart/UpdateQuantity - Handles updating a product's quantity in the cart (via AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateQuantity(int productId, int newQuantity) // Made async
        {
            var userId = _userManager.GetUserId(User);
            // Find the item for the current user and product ID
            var itemToUpdate = await _context.CartItems.FirstOrDefaultAsync(item => item.ProductId == productId && item.UserId == userId);

            if (itemToUpdate == null)
            {
                return Json(new { success = false, message = "Item not found in cart." });
            }

            // If new quantity is 0 or less, remove the item
            if (newQuantity <= 0)
            {
                _context.CartItems.Remove(itemToUpdate);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"'{itemToUpdate.ProductName}' removed from cart as quantity was set to 0." });
            }
            else
            {
                itemToUpdate.Quantity = newQuantity; // Update quantity
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = $"Quantity for '{itemToUpdate.ProductName}' updated to {newQuantity}." });
            }
        }

        // GET: Cart/GetCartData - Provides current cart data for the logged-in user (for AJAX display)
        [HttpGet]
        public async Task<IActionResult> GetCartData() // Made async
        {
            // Return empty cart data if user is not authenticated (though [Authorize] generally handles this)
            if (!User.Identity.IsAuthenticated)
            {
                return Json(new { success = false, message = "User is not logged in.", items = new List<object>(), subtotal = 0, totalItems = 0, hasItems = false, shippingCost = 0m });
            }

            var userId = _userManager.GetUserId(User);
            // Fetch cart items for this user from the database
            var cartItems = await _context.CartItems.Where(c => c.UserId == userId).ToListAsync();

            var productCatalog = _productService.GetProductCatalog();

            // Prepare cart data for JSON response
            var itemsForJson = new List<object>();
            foreach (var item in cartItems)
            {
                // Re-fetch/re-parse product details to ensure latest info and correct TotalPrice calculation
                if (productCatalog.TryGetValue(item.ProductId, out ProductViewModel? productDetails))
                {
                    itemsForJson.Add(new
                    {
                        productId = item.ProductId,
                        productName = productDetails.ProductName, // Use fresh name from catalog
                        price = _productService.ParsePrice(productDetails.Price), // Use fresh parsed price from catalog
                        quantity = item.Quantity,
                        imageUrl = productDetails.MainImage, // Use fresh image from catalog
                        totalPrice = _productService.ParsePrice(productDetails.Price) * item.Quantity // Calculate total
                    });
                }
                else
                {
                    // Handle product not found in catalog for display
                    itemsForJson.Add(new
                    {
                        productId = item.ProductId,
                        productName = item.ProductName + " (Unavailable)",
                        price = item.Price, // Use stored price if actual product not found
                        quantity = item.Quantity,
                        imageUrl = "/images/placeholder.png",
                        totalPrice = item.Price * item.Quantity
                    });
                }
            }

            // Calculate shipping cost from CartViewModel logic for consistency
            var cartViewModel = new CartViewModel { Items = cartItems }; // Temporarily create to get ShippingCost
            decimal shippingCost = cartViewModel.ShippingCost;

            var cartData = new
            {
                items = itemsForJson, // Use the dynamically created list
                subtotal = itemsForJson.Sum(item => (decimal)((dynamic)item).totalPrice), // Sum from dynamic objects
                totalItems = itemsForJson.Sum(item => (int)((dynamic)item).quantity), // Sum quantity
                hasItems = itemsForJson.Any(),
                shippingCost = shippingCost // Include shippingCost
            };

            return Json(cartData);
        }
    }
}