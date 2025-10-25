namespace ASAPPVC.UI.Models.ViewModels.Inventory.Product
{
    public class ProductViewModel
    {
        public Guid Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string Description { get; set; } = string.Empty;

        // For display, use a safe serialized string rather than raw bytes
        public string? ImageBase64 { get; set; }

        public string? ImageContentType { get; set; }

        // Components/parts that make up this product
        public List<ProductComponentViewModel> Components { get; set; } = new();
    }
}