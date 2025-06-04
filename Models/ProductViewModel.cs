using System.Collections.Generic;
using System.Text.Json.Serialization; // Make sure this is included
using System.Text.Json; // Needed for JsonElement
using BE.Converters; // Needed for custom converters

namespace BE.Models
{
    public class ProductViewModel
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }
        public string Price { get; set; }
        public string DetailsUrl { get; set; }
        public string MainImage { get; set; }
        public string Category { get; set; }
        public string SubCategory { get; set; }
        public string SubSubCategory { get; set; }

        [JsonConverter(typeof(StringOrListConverter))] // Apply the new converter
        public List<string>? GalleryImages { get; set; } // Made nullable to align with converter returning null/empty list

        // NEW: Added properties based on the JSON snippet
        public string? FullDescription { get; set; } // Can be null if not present
        public string? BrandingGuide { get; set; } // Can be null if not present

        [JsonConverter(typeof(StringOrListConverter))] // Apply the converter for list handling
        public List<string>? ShortDescription { get; set; } // Can be null or empty list

        // Note: PricingTiers and Variants are handled within AdditionalInfo via JsonElement or custom parsing
        // If you want them as strongly typed properties, you would need to define new classes
        // for them and potentially add more converters, or parse them directly in the controller/view.

        [JsonConverter(typeof(StringOrJsonElementConverter))] // Apply the new converter
        public System.Text.Json.JsonElement AdditionalInfo { get; set; }
    }
}