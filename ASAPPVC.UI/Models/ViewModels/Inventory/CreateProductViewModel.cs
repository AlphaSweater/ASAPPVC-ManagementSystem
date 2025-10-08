using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models.ViewModels.Inventory
{
    public class CreateProductViewModel
    {
        [Required]
        public string? ProductName { get; set; }

        [Range(0, 1_000_000)]
        public decimal BasePrice { get; set; }

        [Required]
        public string? Description { get; set; }

        public IFormFile? ImageFile { get; set; }

        public List<ProductPartLine> Parts { get; set; } = new();
    }
    public class ProductPartLine
    {
        public int? PartId { get; set; }        
        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }
}
