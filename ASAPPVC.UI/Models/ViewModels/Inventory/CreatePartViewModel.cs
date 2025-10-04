using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models.ViewModels.Inventory
{
    public class CreatePartViewModel
    {
        [Required, MaxLength(200)]
        public string Name { get; set; }

        [Required, MaxLength(200)]
        public string StorageLocation { get; set; }

        [Required]
        public decimal UnitCost { get; set; }

        [Required]
        public int CurrentAmount { get; set; }

        // Optional image
        public IFormFile? ImageFile { get; set; }

    }
}
