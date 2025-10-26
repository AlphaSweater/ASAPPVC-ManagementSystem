using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.UI.Models
{
    public class ProductComponent
    {
        // Internal GUID primary key
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign key to the Component
        [Required, ForeignKey(nameof(Component))]
        public Guid ComponentId { get; set; }

        // Foreign key to the Product
        [Required, ForeignKey(nameof(Product))]
        public Guid ProductId { get; set; }

        // Quantity of this Component in the Product
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        // Navigation properties
        public ComponentModel? Component { get; set; }

        public Product? Product { get; set; }
    }
}