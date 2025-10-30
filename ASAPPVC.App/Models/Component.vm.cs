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

        // Reorder signal
        public decimal ReorderLevel { get; init; }

        // Media

        public bool HasImage { get; init; }
        public string? ThumbUrl { get; init; }

        // --------------------------------------------------
        // Computed UI helpers

        public string DisplayCost => UnitCost.ToString("C");
        public string ShortFormattedQuantity => UnitOfMeasure.ToDisplay(QuantityOnHand, shortForm: true);

        public ReorderStatus ReorderStatus => ReorderStatusPolicy.Evaluate(QuantityOnHand, ReorderLevel);
        public decimal ReorderRatio => ReorderStatusPolicy.GetRatio(QuantityOnHand, ReorderLevel);

        public string StockHealth =>
            ReorderStatus.GetAttributePropertyOrDefault<ReorderStatusInfoAttribute, string>("Label", ReorderStatus.ToString());

        public string StockHealthCss =>
            ReorderStatus.GetAttributePropertyOrDefault<ReorderStatusInfoAttribute, string>("CssClass", "status-ok");

        public string StockHealthColor =>
            ReorderStatus.GetAttributePropertyOrDefault<ReorderStatusInfoAttribute, string>("Color", "#27ae60");

        // Audit + Lifecycle
        public bool IsActive { get; init; } = true;
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

        // Media

        public bool HasImage { get; init; }
        public string? ImageUrl { get; init; }
        public string? ImageEtag { get; init; }

        // Relationships / usage

        public int UsedInProductsCount { get; init; }

        // --------------------------------------------------
        // Computed UI helpers

        public string DisplayPrice => UnitCost.ToString("C");
        public string ShortFormattedQuantity => UnitOfMeasure.ToDisplay(QuantityOnHand, shortForm: true);

        public ReorderStatus ReorderStatus => ReorderStatusPolicy.Evaluate(QuantityOnHand, ReorderLevel);
        public decimal ReorderRatio => ReorderStatusPolicy.GetRatio(QuantityOnHand, ReorderLevel);

        public string StockHealth =>
            ReorderStatus.GetAttributePropertyOrDefault<ReorderStatusInfoAttribute, string>("Label", ReorderStatus.ToString());

        public string StockHealthCss =>
            ReorderStatus.GetAttributePropertyOrDefault<ReorderStatusInfoAttribute, string>("CssClass", "status-ok");

        public string StockHealthColor =>
            ReorderStatus.GetAttributePropertyOrDefault<ReorderStatusInfoAttribute, string>("Color", "#27ae60");

        // Audit + Lifecycle

        public bool IsActive { get; init; } = true;
        public DateTime CreatedAt { get; init; }
        public DateTime? UpdatedAt { get; init; }
    }

    //-----------------------------------------------\\
    // Create/Update form (Add + Edit)
    //-----------------------------------------------\\
    public sealed class ComponentFormVm
    {
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

        [Display(Name = "Material")]
        public Material MaterialType { get; set; } = Material.None;

        [Display(Name = "Colour")]
        public Colour ColourOption { get; set; } = Colour.None;

        // Inventory & cost
        [Display(Name = "Unit of Measurement")]
        [Required(ErrorMessage = "Unit of Measurement is required.")]
        public Unit UnitOfMeasure { get; set; } = Unit.Piece;

        [Display(Name = "Quantity on Hand")]
        [Required(ErrorMessage = "Quantity on hand is required.")]
        public decimal QuantityOnHand { get; set; }

        [Display(Name = "Unit Cost")]
        [Required(ErrorMessage = "Unit cost is required.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Unit cost must be a positive amount.")]
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

        // Media
        [Display(Name = "Image (optional)")]
        public IFormFile? Image { get; set; }

        // For edit preview
        public string? ExistingImageUrl { get; set; }

        // Audit + Lifecycle
        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;
    }
}