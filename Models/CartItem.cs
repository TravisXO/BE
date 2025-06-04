using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BE.Areas.Identity.Data;
using Microsoft.EntityFrameworkCore; // Added for Index attribute

namespace BE.Models
{
    // Add this attribute to define a unique composite index
    [Index(nameof(UserId), nameof(ProductId), IsUnique = true)]
    public class CartItem
    {
        [Key]
        public int CartItemId { get; set; }

        [Display(Name = "Product ID")]
        public int ProductId { get; set; }

        [Display(Name = "Product Name")]
        public string ProductName { get; set; } = string.Empty;

        [DisplayFormat(DataFormatString = "{0:C}")]
        [DataType(DataType.Currency)]
        public decimal Price { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Display(Name = "Image")]
        public string ImageUrl { get; set; } = string.Empty;

        [NotMapped]
        public decimal TotalPrice => Price * Quantity;

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public BEUser? User { get; set; }
    }
}