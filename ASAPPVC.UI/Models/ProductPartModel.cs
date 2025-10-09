using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models
{
    public class ProductPartModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int ProductPartID { get; set; }

        [Required, ForeignKey("PartModel")]
        public int PartID { get; set; }

        [Required, ForeignKey("ProductModel")]
        public int ProductID { get; set; }

        public int Quantity { get; set; }

        public PartModel? Part { get; set; }
        public ProductModel? Product { get; set; }
    }
}
