// ProductService.cs
using BE.Models;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Globalization;
using System.Text.RegularExpressions;
using BE.Converters; // Make sure this is included for the new converters

namespace BE.Services
{
    public class ProductService
    {
        private readonly IWebHostEnvironment _env;
        private static Dictionary<int, ProductViewModel> _productCatalogCache = new();
        private static readonly object _lock = new object();
        private static bool _cacheLoaded = false;

        public ProductService(IWebHostEnvironment env)
        {
            _env = env;
            LoadProductCatalog(); // Load products once on service instantiation
        }

        private void LoadProductCatalog()
        {
            if (!_cacheLoaded)
            {
                lock (_lock)
                {
                    if (!_cacheLoaded) // Double-check lock
                    {
                        var productFiles = new[] { "all-products.json" };
                        List<ProductViewModel> allProductsRaw = new();

                        // Define JsonSerializerOptions with your custom converters
                        var options = new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true, // Matches "productName" in JSON to ProductName in C#
                            AllowTrailingCommas = true, // Allow trailing commas in JSON files
                            ReadCommentHandling = JsonCommentHandling.Skip, // Ignore comments in JSON files
                            Converters =
                            {
                                new StringOrListConverter(), // Register the new List converter
                                new StringOrJsonElementConverter() // Register the new JsonElement converter
                            }
                        };

                        foreach (var file in productFiles)
                        {
                            var path = Path.Combine(_env.WebRootPath, "products", file);
                            if (File.Exists(path))
                            {
                                try
                                {
                                    string json = File.ReadAllText(path);
                                    var products = JsonSerializer.Deserialize<List<ProductViewModel>>(json, options); // Use options with converters

                                    if (products != null)
                                        allProductsRaw.AddRange(products);
                                }
                                catch (JsonException ex)
                                {
                                    Console.WriteLine($"Error loading {file}: {ex.Message}");
                                    // Consider logging the full exception details or throw a more specific error
                                }
                            }
                        }

                        // Assign ProductIds uniquely and sequentially
                        int idCounter = 1;
                        foreach (var product in allProductsRaw)
                        {
                            // Only assign if ProductId hasn't been set (e.g., from JSON if it had one)
                            if (product.ProductId == 0) // Assuming 0 means not assigned or default
                            {
                                product.ProductId = idCounter++;
                            }
                            else
                            {
                                // Ensure unique IDs if they exist in JSON, or increment past them
                                if (product.ProductId >= idCounter)
                                {
                                    idCounter = product.ProductId + 1;
                                }
                            }
                            _productCatalogCache[product.ProductId] = product;
                        }
                        _cacheLoaded = true;
                    }
                }
            }
        }

        public Dictionary<int, ProductViewModel> GetProductCatalog()
        {
            return new Dictionary<int, ProductViewModel>(_productCatalogCache);
        }

        public IEnumerable<ProductViewModel> GetAllProducts()
        {
            return _productCatalogCache.Values.ToList();
        }

        // NEW: Method to get a product by its ID
        public ProductViewModel? GetProductById(int productId)
        {
            _productCatalogCache.TryGetValue(productId, out ProductViewModel? product);
            return product;
        }


        public decimal ParsePrice(string priceString)
        {
            if (string.IsNullOrWhiteSpace(priceString) || priceString.Trim().Equals("N/A", StringComparison.OrdinalIgnoreCase))
            {
                return 0m;
            }

            var matches = Regex.Matches(priceString, @"(\d+(\.\d+)?)");
            var prices = new List<decimal>();

            foreach (Match match in matches)
            {
                if (decimal.TryParse(match.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal p))
                {
                    prices.Add(p);
                }
            }

            if (!prices.Any())
            {
                return 0m;
            }
            else if (prices.Count == 1)
            {
                return prices.First();
            }
            else
            {
                return prices.Max();
            }
        }
    }
}