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
    }
}
