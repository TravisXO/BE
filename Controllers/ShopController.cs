using BE.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;
using BE.Services;
using Microsoft.AspNetCore.Identity;
using BE.Areas.Identity.Data;

namespace BE.Controllers
{
    public class ShopController : Controller
    {
        private readonly IWebHostEnvironment _env;
        private readonly ProductService _productService;
        private readonly SignInManager<BEUser> _signInManager;

        public ShopController(IWebHostEnvironment env, ProductService productService, SignInManager<BEUser> signInManager)
        {
            _env = env;
            _productService = productService;
            _signInManager = signInManager;
        }

        private IEnumerable<string> ExtractColorWordsFromUrl(string url, HashSet<string> knownColors)
        {
            var uri = new Uri(url);
            var filename = Path.GetFileNameWithoutExtension(uri.LocalPath);

            var potentialWords = Regex.Split(filename, @"[-_\s%]+", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            foreach (var word in potentialWords)
            {
                var cleanedWord = Regex.Replace(word, @"[^a-zA-Z]", "");
                if (knownColors.Contains(cleanedWord))
                {
                    yield return cleanedWord;
                }
            }
        }

        private static string ToTitleCase(string key)
        {
            TextInfo textInfo = new CultureInfo("en-US", false).TextInfo;
            string formattedKey = Regex.Replace(key, @"[-_]", " ");
            return textInfo.ToTitleCase(formattedKey);
        }

        public IActionResult Shop(int page = 1, string search = "", string category = "", string subCategory = "", string subSubCategory = "", string color = "", string liquidCapacity = "", string headwearSize = "", string ageGroup = "", string sizeFilter = "", decimal? minPrice = null, decimal? maxPrice = null, string sortBy = "")
        {
            List<ProductViewModel> allProducts = _productService.GetAllProducts().ToList();

            var knownColours = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Red", "Blue", "Green", "Black", "White", "Yellow", "Purple", "Orange",
                "Grey", "Gray", "Pink", "Brown", "Beige", "Cyan", "Magenta", "Teal",
                "Maroon", "Navy", "Fuchsia", "Lime", "Olive", "Silver", "Gold",
                "Transparent", "Clear"
            };

            var purchasableProductsForBounds = _productService.GetAllProducts()
                .Where(p => !string.IsNullOrWhiteSpace(p.Price) && p.Price.Trim().ToUpper() != "N/A")
                .ToList();
            decimal initialMinPrice = purchasableProductsForBounds.Any() ? purchasableProductsForBounds.Min(p => _productService.ParsePrice(p.Price)) : 0;
            decimal initialMaxPrice = purchasableProductsForBounds.Any() ? purchasableProductsForBounds.Max(p => _productService.ParsePrice(p.Price)) : 1000;

            if (minPrice.HasValue || maxPrice.HasValue)
            {
                allProducts = allProducts
                    .Where(p =>
                    {
                        decimal parsedPrice = _productService.ParsePrice(p.Price);
                        bool passesMin = !minPrice.HasValue || parsedPrice >= minPrice.Value;
                        bool passesMax = !maxPrice.HasValue || parsedPrice <= maxPrice.Value;
                        return passesMin && passesMax;
                    })
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                allProducts = allProducts
                    .Where(p => !string.IsNullOrWhiteSpace(p.ProductName) &&
                                p.ProductName.Contains(search, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                allProducts = allProducts
                    .Where(p =>
                        (p.Category != null && p.Category.Equals(category, StringComparison.OrdinalIgnoreCase)) ||
                        (p.SubCategory != null && p.SubCategory.Equals(category, StringComparison.OrdinalIgnoreCase)) ||
                        (p.SubSubCategory != null && p.SubSubCategory.Equals(category, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(color))
            {
                Console.WriteLine($"[Color Filter] Applying filter for color: {color}");
                var productsBeforeFilter = allProducts.Count;
                Console.WriteLine($"[Color Filter] Products before filter: {productsBeforeFilter}");

                allProducts = allProducts
                    .Where(p =>
                        (p.GalleryImages != null && p.GalleryImages.Any(imgUrl =>
                        {
                            foreach (var extractedColorWord in ExtractColorWordsFromUrl(imgUrl, knownColours))
                            {
                                if (extractedColorWord.Equals(color, StringComparison.OrdinalIgnoreCase))
                                {
                                    return true;
                                }
                            }
                            return false;
                        }))
                        ||
                        (p.AdditionalInfo.ValueKind == JsonValueKind.Object &&
                         p.AdditionalInfo.TryGetProperty("Colours", out JsonElement colorsElement) &&
                         colorsElement.ValueKind == JsonValueKind.String &&
                         !string.IsNullOrWhiteSpace(colorsElement.GetString()) &&
                         colorsElement.GetString().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                  .Select(c => Regex.Replace(c, @"[^a-zA-Z]", "").Trim())
                                  .Any(c => c.Equals(color, StringComparison.OrdinalIgnoreCase)))
                    )
                    .ToList();

                var productsAfterFilter = allProducts.Count;
                Console.WriteLine($"[Color Filter] Products after filter: {productsAfterFilter}");
                if (productsBeforeFilter > 0 && productsAfterFilter == productsBeforeFilter)
                {
                    Console.WriteLine("[Color Filter] Warning: No products were filtered out. Check image naming conventions or input color.");
                }
            }

            if (!string.IsNullOrWhiteSpace(Request.Query["gender"]))
            {
                string genderFilter = Request.Query["gender"];
                allProducts = allProducts
                    .Where(p =>
                        p.AdditionalInfo.ValueKind == JsonValueKind.Object &&
                        p.AdditionalInfo.TryGetProperty("Gender", out JsonElement genderElement) &&
                        genderElement.ValueKind == JsonValueKind.String &&
                        genderElement.GetString().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                     .Select(g => Regex.Replace(g, @"[^a-zA-Z]", "").Trim())
                                     .Any(g => g.Equals(genderFilter, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
            }

            if (!string.IsNullOrWhiteSpace(liquidCapacity))
            {
                Console.WriteLine($"[Liquid Capacity Filter] Applying filter for: {liquidCapacity}");
                allProducts = allProducts
                    .Where(p =>
                        p.AdditionalInfo.ValueKind == JsonValueKind.Object &&
                        p.AdditionalInfo.TryGetProperty("Liquid Capacity", out JsonElement liquidCapacityElement) &&
                        liquidCapacityElement.ValueKind == JsonValueKind.String &&
                        liquidCapacityElement.GetString().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                             .Select(lc => Regex.Replace(lc, @"[^0-9a-zA-Z.]", "").Trim())
                                             .Any(lc => lc.Equals(liquidCapacity, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                Console.WriteLine($"[Liquid Capacity Filter] Products after filter: {allProducts.Count}");
            }

            if (!string.IsNullOrWhiteSpace(headwearSize))
            {
                Console.WriteLine($"[Headwear Size Filter] Applying filter for: {headwearSize}");
                allProducts = allProducts
                    .Where(p =>
                        p.AdditionalInfo.ValueKind == JsonValueKind.Object &&
                        p.AdditionalInfo.TryGetProperty("Headwear Size", out JsonElement headwearSizeElement) &&
                        headwearSizeElement.ValueKind == JsonValueKind.String &&
                        headwearSizeElement.GetString().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(hs => Regex.Replace(hs, @"[^a-zA-Z]", "").Trim())
                                           .Any(hs => hs.Equals(headwearSize, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                Console.WriteLine($"[Headwear Size Filter] Products after filter: {allProducts.Count}");
            }

            if (!string.IsNullOrWhiteSpace(ageGroup))
            {
                Console.WriteLine($"[Age Group Filter] Applying filter for: {ageGroup}");
                allProducts = allProducts
                    .Where(p =>
                        p.AdditionalInfo.ValueKind == JsonValueKind.Object &&
                        p.AdditionalInfo.TryGetProperty("Age Group", out JsonElement ageGroupElement) &&
                        ageGroupElement.ValueKind == JsonValueKind.String &&
                        ageGroupElement.GetString().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(ag => Regex.Replace(ag, @"[^a-zA-Z]", "").Trim())
                                           .Any(ag => ag.Equals(ageGroup, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                Console.WriteLine($"[Age Group Filter] Products after filter: {allProducts.Count}");
            }

            if (!string.IsNullOrWhiteSpace(sizeFilter))
            {
                Console.WriteLine($"[Size Filter] Applying filter for: {sizeFilter}");
                allProducts = allProducts
                    .Where(p =>
                        p.AdditionalInfo.ValueKind == JsonValueKind.Object &&
                        p.AdditionalInfo.TryGetProperty("Size", out JsonElement sizeElement) &&
                        sizeElement.ValueKind == JsonValueKind.String &&
                        sizeElement.GetString().Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                           .Select(s => Regex.Replace(s, @"[^0-9a-zA-Z]", "").Trim())
                                           .Any(s => s.Equals(sizeFilter, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                Console.WriteLine($"[Size Filter] Products after filter: {allProducts.Count}");
            }

            switch (sortBy)
            {
                case "priceAsc":
                    allProducts = allProducts.OrderBy(p => _productService.ParsePrice(p.Price)).ToList();
                    break;
                case "priceDesc":
                    allProducts = allProducts.OrderByDescending(p => _productService.ParsePrice(p.Price)).ToList();
                    break;
                case "nameAsc":
                    allProducts = allProducts.OrderBy(p => p.ProductName).ToList();
                    break;
                case "nameDesc":
                    allProducts = allProducts.OrderByDescending(p => p.ProductName).ToList();
                    break;
                default:
                    allProducts = allProducts.OrderBy(p => p.ProductId).ToList();
                    break;
            }

            var filteredProductsForFilterOptions = allProducts.ToList();

            HashSet<string> colorSet = new();
            foreach (var product in filteredProductsForFilterOptions)
            {
                if (product.GalleryImages != null)
                {
                    foreach (var url in product.GalleryImages)
                    {
                        foreach (var extractedColorWord in ExtractColorWordsFromUrl(url, knownColours))
                        {
                            colorSet.Add(extractedColorWord);
                            break;
                        }
                    }
                }
                if (product.AdditionalInfo.ValueKind == JsonValueKind.Object && product.AdditionalInfo.TryGetProperty("Colours", out JsonElement colorsObjElement) && colorsObjElement.ValueKind == JsonValueKind.String)
                {
                    string colorsString = colorsObjElement.GetString();
                    if (!string.IsNullOrWhiteSpace(colorsString))
                    {
                        var colorsInInfo = colorsString.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                       .Select(c => Regex.Replace(c, @"[^a-zA-Z]", "").Trim())
                                                       .Where(c => knownColours.Contains(c));
                        foreach (var c in colorsInInfo)
                        {
                            colorSet.Add(c);
                        }
                    }
                }
            }
            ViewBag.AvailableColors = colorSet.OrderBy(c => c).ToList();

            var genderSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var product in filteredProductsForFilterOptions)
            {
                if (product.AdditionalInfo.ValueKind == JsonValueKind.Object &&
                    product.AdditionalInfo.TryGetProperty("Gender", out JsonElement genderElement) &&
                    genderElement.ValueKind == JsonValueKind.String)
                {
                    string genderString = genderElement.GetString();
                    if (!string.IsNullOrWhiteSpace(genderString))
                    {
                        var gendersInInfo = genderString.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(g => Regex.Replace(g, @"[^a-zA-Z]", "").Trim())
                                                        .Where(g => !string.IsNullOrWhiteSpace(g));
                        foreach (var g in gendersInInfo)
                        {
                            genderSet.Add(g);
                        }
                    }
                }
            }
            ViewBag.AvailableGenders = genderSet.OrderBy(g => g).ToList();

            var liquidCapacitySet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var product in filteredProductsForFilterOptions)
            {
                if (product.AdditionalInfo.ValueKind == JsonValueKind.Object &&
                    product.AdditionalInfo.TryGetProperty("Liquid Capacity", out JsonElement liquidCapacityElement) &&
                    liquidCapacityElement.ValueKind == JsonValueKind.String)
                {
                    string lcString = liquidCapacityElement.GetString();
                    if (!string.IsNullOrWhiteSpace(lcString))
                    {
                        var capacitiesInInfo = lcString.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(lc => Regex.Replace(lc, @"[^0-9a-zA-Z.]", "").Trim())
                                                        .Where(lc => !string.IsNullOrWhiteSpace(lc));
                        foreach (var lc in capacitiesInInfo)
                        {
                            liquidCapacitySet.Add(lc);
                        }
                    }
                }
            }
            ViewBag.AvailableLiquidCapacities = liquidCapacitySet.OrderBy(lc => lc).ToList();

            var headwearSizeSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var product in filteredProductsForFilterOptions)
            {
                if (product.AdditionalInfo.ValueKind == JsonValueKind.Object &&
                    product.AdditionalInfo.TryGetProperty("Headwear Size", out JsonElement headwearSizeElement) &&
                    headwearSizeElement.ValueKind == JsonValueKind.String)
                {
                    string hsString = headwearSizeElement.GetString();
                    if (!string.IsNullOrWhiteSpace(hsString))
                    {
                        var sizesInInfo = hsString.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(hs => Regex.Replace(hs, @"[^a-zA-Z]", "").Trim())
                                                        .Where(hs => !string.IsNullOrWhiteSpace(hs));
                        foreach (var hs in sizesInInfo)
                        {
                            headwearSizeSet.Add(hs);
                        }
                    }
                }
            }
            ViewBag.AvailableHeadwearSizes = headwearSizeSet.OrderBy(hs => hs).ToList();

            var ageGroupSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var product in filteredProductsForFilterOptions)
            {
                if (product.AdditionalInfo.ValueKind == JsonValueKind.Object &&
                    product.AdditionalInfo.TryGetProperty("Age Group", out JsonElement ageGroupElement) &&
                    ageGroupElement.ValueKind == JsonValueKind.String)
                {
                    string agString = ageGroupElement.GetString();
                    if (!string.IsNullOrWhiteSpace(agString))
                    {
                        var groupsInInfo = agString.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(ag => Regex.Replace(ag, @"[^a-zA-Z]", "").Trim())
                                                        .Where(ag => !string.IsNullOrWhiteSpace(ag));
                        foreach (var ag in groupsInInfo)
                        {
                            ageGroupSet.Add(ag);
                        }
                    }
                }
            }
            ViewBag.AvailableAgeGroups = ageGroupSet.OrderBy(ag => ag).ToList();

            var sizeSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var product in allProducts)
            {
                if (product.AdditionalInfo.ValueKind == JsonValueKind.Object &&
                    product.AdditionalInfo.TryGetProperty("Size", out JsonElement sizeElement) &&
                    sizeElement.ValueKind == JsonValueKind.String)
                {
                    string sizeString = sizeElement.GetString();
                    if (!string.IsNullOrWhiteSpace(sizeString))
                    {
                        var sizesInInfo = sizeString.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                                                        .Select(s => Regex.Replace(s, @"[^0-9a-zA-Z]", "").Trim())
                                                        .Where(s => !string.IsNullOrWhiteSpace(s));
                        foreach (var s in sizesInInfo)
                        {
                            sizeSet.Add(s);
                        }
                    }
                }
            }
            ViewBag.AvailableSizes = sizeSet.OrderBy(s => s).ToList();


            int pageSizeValue = 20;
            int skip = (page - 1) * pageSizeValue;
            var paginated = allProducts.Skip(skip).Take(pageSizeValue).ToList();
            var totalPages = (int)Math.Ceiling(allProducts.Count / (double)pageSizeValue);

            var model = new ProductListViewModel
            {
                Products = paginated,
                CurrentPage = page,
                TotalPages = totalPages
            };

            ViewBag.Search = search;
            ViewBag.Category = category;
            ViewBag.Color = color;
            ViewBag.Gender = Request.Query["gender"];
            ViewBag.LiquidCapacity = liquidCapacity;
            ViewBag.HeadwearSize = headwearSize;
            ViewBag.AgeGroup = ageGroup;
            ViewBag.Size = sizeFilter;
            ViewBag.MinPrice = minPrice ?? initialMinPrice;
            ViewBag.MaxPrice = maxPrice ?? initialMaxPrice;
            ViewBag.InitialMinPrice = initialMinPrice;
            ViewBag.InitialMaxPrice = initialMaxPrice;
            ViewBag.SortBy = sortBy;
            ViewBag.TotalProducts = allProducts.Count;


            ViewBag.AllCategoriesUnified = allProducts
                .SelectMany(p => new[] { p.Category, p.SubCategory, p.SubSubCategory })
                .Where(c => !string.IsNullOrWhiteSpace(c) && !c.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                .Select(c => c.Trim())
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            string matchedCategory = category?.Trim();
            string matchedCategoryLevel = "";

            if (!string.IsNullOrWhiteSpace(matchedCategory))
            {
                var sample = allProducts
                    .FirstOrDefault(p =>
                        string.Equals(p.Category, matchedCategory, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.SubCategory, matchedCategory, StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(p.SubSubCategory, matchedCategory, StringComparison.OrdinalIgnoreCase));

                if (sample != null)
                {
                    if (string.Equals(sample.Category, matchedCategory, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedCategoryLevel = "Category";
                    }
                    else if (string.Equals(sample.SubCategory, matchedCategory, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedCategoryLevel = "SubCategory";
                    }
                    else if (string.Equals(sample.SubSubCategory, matchedCategory, StringComparison.OrdinalIgnoreCase))
                    {
                        matchedCategoryLevel = "SubSubCategory";
                    }

                    ViewBag.SubCategories = allProducts
                        .Where(p => string.Equals(p.Category, sample.Category, StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.SubCategory?.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();

                    ViewBag.SubSubCategories = allProducts
                        .Where(p => string.Equals(p.Category, sample.Category, StringComparison.OrdinalIgnoreCase))
                        .Select(p => p.SubSubCategory?.Trim())
                        .Where(c => !string.IsNullOrWhiteSpace(c))
                        .Distinct()
                        .OrderBy(c => c)
                        .ToList();
                }
            }

            ViewBag.MatchedCategory = matchedCategory;
            ViewBag.MatchedCategoryLevel = matchedCategoryLevel;

            var colorUrls = allProducts
                .Where(p => p.GalleryImages != null)
                .SelectMany(p => p.GalleryImages)
                .Distinct()
                .ToList();

            return View(model);
        }

        // NEW: Action for Product Details page
        public IActionResult ProductDetails(int id)
        {
            var product = _productService.GetProductById(id);
            if (product == null)
            {
                return NotFound(); // Return 404 if product not found
            }
            return View(product); // Pass the product to the view
        }
    }
}