using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models.ViewModels.Inventory.Product
{
    public class ProductComponentViewModel
    {
        [Required]
        public Guid ComponentId { get; set; }

        [Range(1, int.MaxValue)]
        public int Quantity { get; set; } = 1;
    }
}