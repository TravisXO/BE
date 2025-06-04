namespace BE.Models
{
    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        // Calculated properties for the entire cart
        public decimal Subtotal => Items.Sum(item => item.TotalPrice);

        // Placeholder for shipping, will be 0 for now as per requirement
        public decimal ShippingCost => 0m;

        public decimal GrandTotal => Subtotal + ShippingCost;

        public bool HasItems => Items.Any();

        // Optional: TempData message to display feedback to the user
        public string? Message { get; set; }
    }
}