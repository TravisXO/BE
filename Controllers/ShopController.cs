using Microsoft.AspNetCore.Mvc;
using BE.Models;
using System.Text.Json;

namespace BE.Controllers
{
    public class ShopController : Controller
    {
        public IActionResult Shop()
        {
            var jsonPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/products/bags-backpacks/bags-&-backpacks-list.json");
            if (!System.IO.File.Exists(jsonPath))
                return NotFound("Product data file not found.");

            var json = System.IO.File.ReadAllText(jsonPath);
            var products = JsonSerializer.Deserialize<List<ProductList>>(json);
            return View(products);
        }
    }
}
