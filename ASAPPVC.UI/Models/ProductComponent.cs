using ASAPPVC.UI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.UI.Models
{
    /// <summary>
    /// Bridge entity linking a Product to many Components with a required Quantity (per set).<br/>
    /// Uses a composite key of ProductId and ComponentId.
    /// </summary>
    public class ProductComponent
    {
        // Foreign key to the Product
        [Required, ForeignKey(nameof(Product))]
        public Guid ProductId { get; set; }

        // Foreign key to the Component
        [Required, ForeignKey(nameof(Component))]
        public Guid ComponentId { get; set; }

        // Unit of Measure for the Component in this Product
        [Required]
        public Unit Unit { get; set; } = Unit.Piece;

        // Quantity of this Component in the Product
        [Required]
        [Range(typeof(decimal), "0.01", "999999", ErrorMessage = "Quantity must be greater than zero")]
        public decimal QuantityRequired { get; set; } = 1m;

        // Navigation properties
        public Component? Component { get; set; }

        public Product? Product { get; set; }
    }
}