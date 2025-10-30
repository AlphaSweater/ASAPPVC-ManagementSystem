using ASAPPVC.App.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.App.Models
{
    //-----------------------------------------------\\
    // Component ViewModels (Read + Write)
    //-----------------------------------------------\\

    //-----------------------------------------------\\
    // Summary (lists, tables, search results)
    //-----------------------------------------------\\
    public sealed class ComponentListVm
    {
        public Guid Id { get; init; }
        public string ComponentCode { get; init; } = string.Empty;
        public string ComponentName { get; init; } = string.Empty;

        public Unit UnitOfMeasure { get; init; }
        public decimal QuantityOnHand { get; init; }
        public decimal UnitCost { get; init; }

        // Storage

        public string LocationCode { get; set; } = string.Empty;
        public string? LocationNote { get; set; }

        // Reorder signals

        public decimal ReorderLevel { get; init; }
        public bool IsActive { get; init; } = true;

        // Media

        public bool HasImage { get; init; }
        public string? ThumbUrl { get; init; }

        // Computed UI helpers

        public string DisplayPrice => UnitCost.ToString("C");
        public string ShortFormattedQuantity => UnitOfMeasure.ToDisplay(QuantityOnHand, shortForm: true);
        public bool IsBelowReorder => ReorderLevel > 0 && QuantityOnHand <= ReorderLevel;
        public string StockHealth => IsBelowReorder ? "Restock" : "OK";
    }

    //-----------------------------------------------\\
    // Detail (single component view)
    //-----------------------------------------------\\
    public sealed class ComponentDetailVm
    {
        public Guid Id { get; init; }
        public string ComponentCode { get; init; } = string.Empty;
        public string ComponentName { get; init; } = string.Empty;

        // Classification

        public Category Category { get; init; }
        public Material MaterialType { get; init; }
        public Colour ColourOption { get; init; }

        // Inventory & cost

        public Unit UnitOfMeasure { get; init; }
        public decimal QuantityOnHand { get; init; }
        public decimal UnitCost { get; init; }

        // Storage

        public string LocationCode { get; set; } = string.Empty;
        public string? LocationNote { get; set; }

        // Reorder

        public decimal ReorderLevel { get; init; }
        public decimal ReorderQuantity { get; init; }
        public bool IsActive { get; init; } = true;

        // Media

        public bool HasImage { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageEtag { get; init; }

        // Relationships / usage

        public int UsedInProductsCount { get; init; }

        // Computed UI helpers

        public string DisplayPrice => UnitCost.ToString("C");
        public string ShortFormattedQuantity => UnitOfMeasure.ToDisplay(QuantityOnHand, shortForm: true);
        public bool IsBelowReorder => ReorderLevel > 0 && QuantityOnHand <= ReorderLevel;

        // Audit (read-only display)

        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    //-----------------------------------------------\\
    // Create/Update form (Add + Edit)
    //-----------------------------------------------\\
    public sealed class ComponentFormVm : IValidatableObject
    {
        private const decimal MaxQuantity = 1_000_000_000_000m;

        public Guid? Id { get; set; }
        public bool IsEdit => Id.HasValue;

        [Display(Name = "Component Code")]
        [StringLength(64)]
        public string? ComponentCode { get; set; } // required only on Edit (see Validate)

        [Display(Name = "Component Name")]
        [Required(ErrorMessage = "Component name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Component name must be between 2 and 100 characters.")]
        public string ComponentName { get; set; } = string.Empty;

        // Classification (optional inputs)
        [Display(Name = "Category")]
        public Category Category { get; set; } = Category.None;

        [Display(Name = "Material")]
        public Material MaterialType { get; set; } = Material.None;

        [Display(Name = "Colour")]
        public Colour ColourOption { get; set; } = Colour.None;

        // Inventory & cost
        [Display(Name = "Unit of Measure")]
        [Required(ErrorMessage = "Unit of Measure is required.")]
        public Unit UnitOfMeasure { get; init; } = Unit.Piece;

        [Display(Name = "Quantity on Hand")]
        [Required(ErrorMessage = "Quantity on hand is required.")]
        public decimal QuantityOnHand { get; set; }

        [Display(Name = "Unit Cost")]
        [Required(ErrorMessage = "Unit cost is required.")]
        [Range(0.01, 999_999_999, ErrorMessage = "Unit cost must be a positive amount.")]
        public decimal UnitCost { get; set; }

        // Storage
        [Display(Name = "Location Code")]
        [Required(ErrorMessage = "Location code is required.")]
        [StringLength(32)]
        [RegularExpression(@"^[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*$", ErrorMessage = "Use letters/numbers with optional dashes, e.g. A-2 or B-12.")]
        public string LocationCode { get; set; } = string.Empty;

        [Display(Name = "Location Note")]
        [StringLength(100)]
        public string? LocationNote { get; set; }

        // Reorder settings
        [Display(Name = "Reorder Level")]
        [Range(0, double.MaxValue, ErrorMessage = "Reorder level cannot be negative.")]
        public decimal ReorderLevel { get; set; }

        [Display(Name = "Reorder Quantity")]
        [Range(0, double.MaxValue, ErrorMessage = "Reorder quantity cannot be negative.")]
        public decimal ReorderQuantity { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Media
        [Display(Name = "Image (optional)")]
        public IFormFile? Image { get; set; }

        // For edit preview
        public string? ExistingImageUrl { get; set; }

        // Validation
        public IEnumerable<ValidationResult> Validate(ValidationContext context)
        {
            // 1) On Edit, ComponentCode is required
            if (IsEdit && string.IsNullOrWhiteSpace(ComponentCode))
            {
                yield return new ValidationResult(
                    "Component code is required when editing.",
                    new[] { nameof(ComponentCode) });
            }

            // 2) QuantityOnHand must be >= 0 and within sane bounds
            if (QuantityOnHand < 0m)
            {
                yield return new ValidationResult(
                    "Quantity on hand cannot be negative.",
                    new[] { nameof(QuantityOnHand) });
            }
            if (QuantityOnHand >= MaxQuantity)
            {
                yield return new ValidationResult(
                    $"Quantity on hand must be less than {MaxQuantity:N0}.",
                    new[] { nameof(QuantityOnHand) });
            }

            // 3) Integer-only units cannot have fractional amounts
            if (IsIntegerOnlyUnit(UnitOfMeasure) && QuantityOnHand != Math.Floor(QuantityOnHand))
            {
                yield return new ValidationResult(
                    "This unit does not allow fractional quantities.",
                    new[] { nameof(QuantityOnHand) });
            }

            // 4) If a reorder level is set, reorder quantity should be > 0 (helps UX)
            if (ReorderLevel > 0 && ReorderQuantity <= 0)
            {
                yield return new ValidationResult(
                    "Reorder quantity should be greater than 0 when a reorder level is set.",
                    new[] { nameof(ReorderQuantity) });
            }
        }

        /// <summary>
        /// Units that require integer-only values (countable items).
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