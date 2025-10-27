using ASAPPVC.UI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models
{
    //-----------------------------------------------\\
    // Component ViewModels (Read + Write)
    //-----------------------------------------------\\

    //-----------------------------------------------\\
    // Summary (used in component lists, search results)
    //-----------------------------------------------\\
    /// <summary>
    /// Lightweight summary view model used when rendering lists, tables or small preview cards.
    /// Contains only fields required for quick summaries and list displays.
    /// </summary>
    public sealed class ComponentListVm
    {
        public Guid Id { get; init; }
        public string ComponentCode { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public Unit Unit { get; init; }
        public int CurrentAmount { get; init; }
        public decimal UnitCost { get; init; }
        public string StorageLocation { get; init; } = string.Empty;

        // Optional small preview flag

        public bool HasImage { get; init; }
        public string? ThumbUrl { get; init; }
        public string? ImageEtag { get; init; }     // optional: SHA256 for cache busting

        // Optional computed fields for UI display
        public string DisplayCost => UnitCost.ToString("C");

        public string ShortFormattedAmount => Unit.ToDisplay(CurrentAmount, shortForm: true);
    }

    //-----------------------------------------------\\
    // Detail (used for view screen)
    //-----------------------------------------------\\
    /// <summary>
    /// Detailed view model for a single component shown on a details page or modal.
    /// Includes optional image data and a usage count for informational purposes.
    /// </summary>
    public sealed class ComponentDetailVm
    {
        public Guid Id { get; init; }
        public string ComponentCode { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public Unit Unit { get; init; }
        public int CurrentAmount { get; init; }
        public decimal UnitCost { get; init; }
        public string StorageLocation { get; init; } = string.Empty;

        // Optional image url (null if no image)

        public bool HasImage { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageEtag { get; init; }     // optional: SHA256 for cache busting

        // Number of distinct products that reference this component
        public int UsedInProductsCount { get; init; }

        public string DisplayCost => UnitCost.ToString("C");
        public string ShortFormattedAmount => Unit.ToDisplay(CurrentAmount, shortForm: true);
    }

    //-----------------------------------------------\\
    // Create form (used in POST / add component)
    //-----------------------------------------------\\
    /// <summary>
    /// Form view model used when creating a new component (server-side binding).
    /// Includes validation attributes used by Razor Pages forms and model binding.
    /// </summary>
    public sealed class CreateComponentVm
    {
        [Display(Name = "Component Code")]
        [StringLength(64)]
        public string? ComponentCode { get; set; } // optional; generate if null/blank

        [Display(Name = "Component Name")]
        [Required(ErrorMessage = "Component name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Component name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Unit of Measurement")]
        [Required(ErrorMessage = "A Unit of Measurement is required.")]
        public Unit Unit { get; init; } = Unit.Piece;

        [Display(Name = "Current Amount")]
        [Required(ErrorMessage = "Current amount is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Current amount cannot be negative.")]
        public int CurrentAmount { get; set; }

        [Display(Name = "Unit Cost")]
        [Required(ErrorMessage = "Unit cost is required.")]
        [Range(0.01, 999999, ErrorMessage = "Unit cost must be a positive amount.")]
        public decimal UnitCost { get; set; }

        [Display(Name = "Storage Location")]
        [Required(ErrorMessage = "Storage location is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Storage location must be between 2 and 50 characters.")]
        public string StorageLocation { get; set; } = string.Empty;

        [Display(Name = "Image (optional)")]
        public IFormFile? Image { get; set; }
    }

    //-----------------------------------------------\\
    // Edit form (used in PUT / update component)
    //-----------------------------------------------\\
    /// <summary>
    /// Form view model used when editing an existing component. Includes the Id and
    /// validation attributes similar to the create model.
    /// </summary>
    public sealed class EditComponentVm
    {
        [Required(ErrorMessage = "Component ID is required.")]
        public Guid Id { get; set; }

        [Display(Name = "Component Code")]
        [Required(ErrorMessage = "Component code is required.")]
        [StringLength(64)]
        public string ComponentCode { get; set; } = string.Empty;

        [Display(Name = "Component Name")]
        [Required(ErrorMessage = "Component name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Component name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Unit of Measurement")]
        [Required(ErrorMessage = "A Unit of Measurement is required.")]
        public Unit Unit { get; init; } = Unit.Piece;

        [Display(Name = "Current Amount")]
        [Required(ErrorMessage = "Current amount is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Current amount cannot be negative.")]
        public int CurrentAmount { get; set; }

        [Display(Name = "Unit Cost")]
        [Required(ErrorMessage = "Unit cost is required.")]
        [Range(0.01, 999999, ErrorMessage = "Unit cost must be a positive amount.")]
        public decimal UnitCost { get; set; }

        [Display(Name = "Storage Location")]
        [Required(ErrorMessage = "Storage location is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Storage location must be between 2 and 50 characters.")]
        public string StorageLocation { get; set; } = string.Empty;

        [Display(Name = "Image (optional)")]
        public IFormFile? Image { get; set; }

        public string? ExistingImageUrl { get; set; }
    }
}