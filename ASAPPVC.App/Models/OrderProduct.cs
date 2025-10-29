using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.App.Models
{
    /// <summary>
    /// Bridge entity linking an Order to many Products with a Quantity (per order).<br/>
    /// Uses a composite key of OrderId and ProductId.
    /// </summary>
    public class OrderProduct
    {
        // Foreign key to the order
        [Required, ForeignKey(nameof(Order))]
        public Guid OrderId { get; set; }

        // Foreign key to the product
        [Required, ForeignKey(nameof(Product))]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        public Order? Order { get; set; }

        public Product? Product { get; set; }
    }
}