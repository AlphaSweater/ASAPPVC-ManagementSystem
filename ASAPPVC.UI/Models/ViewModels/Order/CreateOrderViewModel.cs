using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models.ViewModels.Order
{
    public class CreateOrderViewModel
    {
        [Required]
        public int CustomerID { get; set; }

        [Required]
        public List<int> ProductIDs { get; set; } = new();

        [Required]
        public string OrderStatus { get; set; }

        public DateTime? OrderDate { get; set; } = DateTime.UtcNow;
    }
}
