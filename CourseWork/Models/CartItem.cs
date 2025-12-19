namespace CourseWork.Models
{
    public class CartItem
    {
        public int VariantId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string WeightInfo { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int Quantity { get; set; }

        public decimal TotalPrice => Price * Quantity;
    }
}