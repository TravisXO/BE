using Microsoft.AspNetCore.Mvc;
using BE.Models;
using System.Text.Json;

namespace BE.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Shop(int page = 1)
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/products/bags-backpacks/bags-&-backpacks-list.json");
            if (!System.IO.File.Exists(jsonPath))
                return NotFound("Product data file not found.");

            var json = System.IO.File.ReadAllText(jsonPath);
            var allProducts = JsonSerializer.Deserialize<List<ProductList>>(json);

            int itemsPerPage = 20;
            int totalItems = allProducts.Count;
            int totalPages = (int)Math.Ceiling(totalItems / (double)itemsPerPage);

            var pagedProducts = allProducts
                .Skip((page - 1) * itemsPerPage)
                .Take(itemsPerPage)
                .ToList();

            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages;

            return View(pagedProducts);
        }
    }
}
