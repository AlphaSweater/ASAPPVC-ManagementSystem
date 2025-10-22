using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models
{
    public class OrderProductModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int OrderProductID { get; set; }

        [Required, ForeignKey("OrderModel")]
        public int OrderID { get; set; }

        [Required, ForeignKey("ProductModel")]
        public int ProductID { get; set; }

        // Quantity of the product in the order. Default to1.
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least1")]
        public int Quantity { get; set; } =1;

        public OrderModel? Order { get; set; }
        public ProductModel? Product { get; set; }
    }
}
