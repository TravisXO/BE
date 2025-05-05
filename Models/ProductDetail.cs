using System.Collections.Generic;

namespace BE.Models
{
    public class ProductDetail
    {
        public string Id { get; set; }
        public List<string> Breadcrumbs { get; set; }
        public List<string> GalleryImageUrls { get; set; }
        public string Name { get; set; }
        public string Price { get; set; }
        public string VatLabel { get; set; }
        public List<string> ShortDescriptionBullets { get; set; }
        public List<PriceTier> InclVatPriceTiers { get; set; }
        public List<PriceTier> ExclVatPriceTiers { get; set; }
        public List<Variant> Variants { get; set; }
        public string Sku { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Tags { get; set; }
        public string FullDescription { get; set; }
        public string AdditionalInfoHtml { get; set; }
        public string BrandingGuidePdfUrl { get; set; }
        public AdditionalInfo AdditionalInfo { get; set; }
        public Attributes Attributes { get; set; }
    }

    public class PriceTier
    {
        public string Price { get; set; }
        public string QuantityRange { get; set; }
    }

    public class Variant
    {
        public string ImageUrl { get; set; }
        public string Name { get; set; }
    }

    public class AdditionalInfo
    {
        public string Colours { get; set; }
    }

    public class Attributes
    {
        public List<string> Colours { get; set; }
        public string LiquidCapacity { get; set; }
        public string CanCapacity { get; set; }
        public string MemoryCapacity { get; set; }
        public string BatteryCapacity { get; set; }
        public string Dimensions { get; set; }
        public string Size { get; set; }
        public string Gender { get; set; }
        public string ClothingSize { get; set; }
        public string StrawType { get; set; }
        public string Brand { get; set; }
        public string AgeGroup { get; set; }
        public string HeadwearSize { get; set; }
        public string Weight { get; set; }
    }
}
