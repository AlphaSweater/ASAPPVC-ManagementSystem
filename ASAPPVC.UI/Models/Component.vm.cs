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
        public decimal CurrentAmount { get; init; }
        public decimal UnitCost { get; init; }
        public string StorageLocation { get; init; } = string.Empty;

        public bool HasImage { get; set; }
        public string? ThumbUrl { get; init; }

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
        public decimal CurrentAmount { get; init; }
        public decimal UnitCost { get; init; }
        public string StorageLocation { get; init; } = string.Empty;

        public bool HasImage { get; set; }
        public string? ImageUrl { get; init; }
        public string? ImageEtag { get; init; }     // optional: SHA256 for cache busting

        // Number of distinct products that reference this component
        public int UsedInProductsCount { get; init; }

        public string DisplayCost => UnitCost.ToString("C");
        public string ShortFormattedAmount => Unit.ToDisplay(CurrentAmount, shortForm: true);
    }

    //-----------------------------------------------\\
    // Create form (used in POST / add + edit component)
    //-----------------------------------------------\\
    /// <summary>
    /// One form VM for both Add and Edit.
    /// If Id is null → Add; if Id has value → Edit.
    /// </summary>
    public sealed class ComponentFormVm : IValidatableObject
    {
        private const decimal MaxAmount = 1_000_000_000_000m;
        private const decimal MinAmount = 0m;

        public Guid? Id { get; set; }

        // Mode
        public bool IsEdit => Id.HasValue;

        [Display(Name = "Component Code")]
        [StringLength(64)]
        public string? ComponentCode { get; set; }

        [Display(Name = "Component Name")]
        [Required(ErrorMessage = "Component name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Component name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Unit of Measurement")]
        [Required(ErrorMessage = "A Unit of Measurement is required.")]
        public Unit Unit { get; init; } = Unit.Piece;

        [Display(Name = "Current Amount")]
        [Required(ErrorMessage = "Current amount is required.")]
        public decimal CurrentAmount { get; set; }

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

        // For edit preview
        public string? ExistingImageUrl { get; set; }

        // Extra validation rules:
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 1) On Edit, ComponentCode is required
            if (IsEdit && string.IsNullOrWhiteSpace(ComponentCode))
            {
                yield return new ValidationResult(
                    "Component code is required when editing.",
                    new[] { nameof(ComponentCode) });
            }

            // 2) CurrentAmount must be greater than 0
            if (CurrentAmount <= 0m)
            {
                yield return new ValidationResult(
                    "Current amount must be greater than 0.",
                    new[] { nameof(CurrentAmount) });
            }

            // 3) CurrentAmount must be less than 1 trillion
            if (CurrentAmount >= MaxAmount)
            {
                yield return new ValidationResult(
                    $"Current amount must be less than {MaxAmount:N0}.",
                    new[] { nameof(CurrentAmount) });
            }

            // 4) Integer-only units cannot have fractional amounts
            if (IsIntegerOnlyUnit(Unit) && CurrentAmount != Math.Floor(CurrentAmount))
            {
                yield return new ValidationResult(
                    "This unit does not allow fractional amounts.",
                    new[] { nameof(CurrentAmount) });
            }
        }

        /// <summary>
        /// Determines if a unit requires integer-only values (countable items).
        /// Metric/measurement units (meter, gram, liter, etc.) allow fractions.
        /// </summary>
        private static bool IsIntegerOnlyUnit(Unit unit)
        {
            return unit switch
            {
                Unit.Piece or Unit.Pair or Unit.Sheet or Unit.Bottle or Unit.Can or
                     Unit.Tube or Unit.Pack or Unit.Set or Unit.Bag or Unit.Roll or
                     Unit.Box or Unit.Pallet => true,
                _ => false
            };
        }
    }
}