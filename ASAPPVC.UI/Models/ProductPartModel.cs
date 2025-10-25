using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.UI.Models
{
    public class ProductPartModel
    {
        // Internal GUID primary key
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Foreign key to the Part
        [Required, ForeignKey(nameof(Part))]
        public Guid PartId { get; set; }

        // Foreign key to the Product
        [Required, ForeignKey(nameof(Product))]
        public Guid ProductId { get; set; }

        // Quantity of this part in the product
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; } = 1;

        // Navigation properties
        public PartModel? Part { get; set; }

        public ProductModel? Product { get; set; }
    }
}