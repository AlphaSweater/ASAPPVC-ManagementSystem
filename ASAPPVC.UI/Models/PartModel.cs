using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections;

namespace ASAPPVC.UI.Models
{
    public class PartModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PartID { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal UnitCost { get; set; }

        [Required]
        public int CurrentAmount { get; set; }

        [Required]
        public string StorageLocation { get; set; }

        public byte[]? ImageBytes { get; set; }
        public string? ImageContentType { get; set; }
    }
}
