using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models.ViewModels.Order
{
    public class ProductQuantityViewModel
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;
    }
}