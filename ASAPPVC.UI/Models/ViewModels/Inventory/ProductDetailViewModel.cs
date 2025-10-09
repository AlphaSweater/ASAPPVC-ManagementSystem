namespace ASAPPVC.UI.Models.ViewModels.Inventory
{
    public class ProductDetailViewModel
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string? Sku { get; set; } 
        public decimal Price { get; set; } 
        public string Description { get; set; }
        public string? ImageSrc { get; set; }
        public List<ProductDetailPartLine> Parts { get; set; } = new();
    }

    public class ProductDetailPartLine
    {
        public string PartName { get; set; }
        public int Quantity { get; set; }
        public decimal? UnitPrice { get; set; } 
    }
}
