using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.UI.Models
{
    public class OrderProductModel
    {
        // Internal GUID primary key for safe relations
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign key to the order
        [Required, ForeignKey(nameof(Order))]
        public Guid OrderId { get; set; }

        // Foreign key to the product
        [Required, ForeignKey(nameof(Product))]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        public OrderModel? Order { get; set; }

        public Product? Product { get; set; }
    }
}