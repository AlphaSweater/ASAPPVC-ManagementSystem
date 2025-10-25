using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models.ViewModels.Inventory.Product
{
    /// <summary>
    /// View model used to bind product creation form input.
    /// </summary>
    public class CreateProductViewModel
    {
        [Required]
        [MaxLength(100)]
        public string? ProductName { get; set; }

        [Range(0, 1_000_000)]
        [Display(Name = "Base Price")]
        public decimal BasePrice { get; set; }

        [Required]
        [MaxLength(500)]
        public string? Description { get; set; }

        // Optional image uploaded by the user
        public IFormFile? ImageFile { get; set; }

        // Components/parts that make up this product
        public List<ProductComponentViewModel> Components { get; set; } = new();
    }
}