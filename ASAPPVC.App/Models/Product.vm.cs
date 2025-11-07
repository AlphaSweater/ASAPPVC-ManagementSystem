using ASAPPVC.App.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.App.Models
{
    //-----------------------------------------------\\
    //  Product ViewModels (Read + Write)
    //-----------------------------------------------\\

    //-----------------------------------------------\\
    // Summary (used in product lists, search results)
    //-----------------------------------------------\\
    /// <summary>
    /// Lightweight summary view model used in product lists and search results.<br/>
    /// Contains only the fields needed for table/card displays and quick summaries.<br/>
    /// Use this VM when rendering lists, search results, or small preview cards where full<br/>
    /// product details are not required.<br/>
    /// </summary>
    public sealed class ProductListVm
    {
        public Guid Id { get; init; }
        public string ProductCode { get; init; } = string.Empty;

        public string ProductName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ThumbUrl { get; init; }
        public bool HasImage { get; set; }

        public decimal SellingPrice { get; init; }

        public decimal PotentialQuantityOnHand { get; init; }
        public decimal ReorderLevel { get; init; }

        // Distinct number of components linked to this product
        public int ComponentCount { get; init; }

        // --------------------------------------------------
        // Computed UI helpers
        public string DisplayPrice => SellingPrice.ToString("C"); // UI currency format

        public ReorderStatus ReorderStatus => ReorderStatusPolicy.Evaluate(PotentialQuantityOnHand, ReorderLevel);
        public decimal ReorderRatio => ReorderStatusPolicy.GetRatio(PotentialQuantityOnHand, ReorderLevel);

        public string StockHealth =>
            ReorderStatus.GetAttributePropertyOrDefault<ReorderStatusInfoAttribute, string>("Label", ReorderStatus.ToString());

        public string StockHealthCss =>
            ReorderStatus.GetAttributePropertyOrDefault<ReorderStatusInfoAttribute, string>("CssClass", "status-ok");

        public string StockHealthColor =>
            ReorderStatus.GetAttributePropertyOrDefault<ReorderStatusInfoAttribute, string>("Color", "#27ae60");

        public string ComponentsBadge => $"{ComponentCount} comp{(ComponentCount == 1 ? "" : "s")}";
    }

    //-----------------------------------------------\\
    // Detail (used for view screen)
    //-----------------------------------------------\\
    /// <summary>
    /// Detailed view model for a single product used on product detail pages or detail modals.<br/>
    /// Includes optional image data (base64) and a collection of read-only component entries.<br/>
    /// Use this VM when you need to display full product information including linked components.<br/>
    /// Not intended for form binding on create/edit endpoints.<br/>
    /// </summary>
    public sealed class ProductDetailVm
    {
        public Guid Id { get; init; }
        public string ProductCode { get; init; } = string.Empty;

        public string ProductName { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string? ImageUrl { get; init; }
        public bool HasImage { get; set; }

        public Category Category { get; init; } = Category.None;
        public Material MaterialType { get; init; } = Material.None;
        public Colour ColourOption { get; init; } = Colour.None;

        public decimal SellingPrice { get; init; }
        public decimal? ProductionCost { get; init; }

        public decimal PotentialQuantityOnHand { get; init; }
        public decimal ReorderLevel { get; init; }

        // Linked components
        public List<ProductComponentVm> ProductComponents { get; init; } = new();

        // --------------------------------------------------
        // Computed UI helpers
        public int ComponentCount => ProductComponents.Count;

        public string DisplayPrice => SellingPrice.ToString("C"); // UI currency format

        public string? DisplayCost =>
            (ProductionCost.HasValue) ? ProductionCost.Value.ToString("C") : null;

        public decimal? GrossMarginAmount =>
            (ProductionCost.HasValue) ? SellingPrice - ProductionCost.Value : null;

        public decimal? GrossMarginPercent =>
            (ProductionCost.HasValue && ProductionCost.Value > 0)
                ? (SellingPrice - ProductionCost.Value) / ProductionCost.Value * 100m
                : null;

        public ReorderStatus ReorderStatus => ReorderStatusPolicy.Evaluate(PotentialQuantityOnHand, ReorderLevel);
        public decimal ReorderRatio => ReorderStatusPolicy.GetRatio(PotentialQuantityOnHand, ReorderLevel);

        public string StockHealth =>
            ReorderStatus.GetAttributePropertyOrDefault<ReorderStatusInfoAttribute, string>("Label", ReorderStatus.ToString());

        public string StockHealthCss =>
            ReorderStatus.GetAttributePropertyOrDefault<ReorderStatusInfoAttribute, string>("CssClass", "status-ok");

        public string StockHealthColor =>
            ReorderStatus.GetAttributePropertyOrDefault<ReorderStatusInfoAttribute, string>("Color", "#27ae60");

        public string ComponentsBadge => $"{ComponentCount} comp{(ComponentCount == 1 ? "" : "s")}";
    }

    //-----------------------------------------------\\
    // Create/Edit form (unified upsert VM)
    //-----------------------------------------------\\
    /// <summary>
    /// One form VM for both Add and Edit (Upsert).
    /// If Id is null → Add; if Id has value → Edit.
    /// </summary>
    public sealed class ProductFormVm : IValidatableObject
    {
        // Mode
        public Guid? Id { get; set; }

        public bool IsEdit => Id.HasValue;

        [Display(Name = "Product Code")]
     [StringLength(64, ErrorMessage = "Product code must be 64 characters or fewer.")]
        public string? ProductCode { get; set; }

      [Display(Name = "Product Name")]
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters.")]
public string ProductName { get; set; } = string.Empty;

        [Display(Name = "Description")]
        [Required(ErrorMessage = "Description is required.")]
     [StringLength(500, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 500 characters.")]
  public string Description { get; set; } = string.Empty;

        // Image upload
    [Display(Name = "Image (optional)")]
        public IFormFile? Image { get; set; }

   public string? ExistingImageUrl { get; set; }

        // Modifiers
        [Display(Name = "Category")]
        public Category Category { get; set; } = Category.None;

        [Display(Name = "Material Type")]
      public Material MaterialType { get; set; } = Material.None;

        [Display(Name = "Colour Option")]
        public Colour ColourOption { get; set; } = Colour.None;

[Display(Name = "Selling Price")]
        [Required(ErrorMessage = "Selling Price is required.")]
 [Range(0.01, 1_000_000_000, ErrorMessage = "Selling Price must be a positive amount.")]
        public decimal SellingPrice { get; set; }

        [Display(Name = "Reorder Level")]
        [Range(0, 1_000_000_000_000, ErrorMessage = "Reorder level cannot be negative.")]
        public decimal ReorderLevel { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

   // Component lines (match model naming)
        [Display(Name = "Components")]
   [MinLength(1, ErrorMessage = "A product requires at least one component.")]
      public List<ProductComponentVm> SelectedProductComponents { get; set; } = new();

 public List<ProductComponentVm> AvailableProductComponents { get; set; } = new();

        // --------------------------------------------------
        // Validation & helpers
        // --------------------------------------------------
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
     {
            // Normalize inputs
            ProductName = ProductName?.Trim() ?? string.Empty;
            Description = Description?.Trim() ?? string.Empty;
            ProductCode = ProductCode?.Trim();

       // Edit-only validation
            if (IsEdit && string.IsNullOrWhiteSpace(ProductCode))
         {
      yield return new ValidationResult(
         "Product code is required when editing.",
            new[] { nameof(ProductCode) });
     }

   // Selling price decimal places
 if (SellingPrice != decimal.Round(SellingPrice, 2))
      {
     yield return new ValidationResult(
       "Selling Price must have at most 2 decimal places.",
       new[] { nameof(SellingPrice) });
            }

      // Validate components
   if (SelectedProductComponents is null || SelectedProductComponents.Count == 0)
   {
     yield return new ValidationResult(
    "A product requires at least one component.",
          new[] { nameof(SelectedProductComponents) });
        }
     else
     {
                for (int i = 0; i < SelectedProductComponents.Count; i++)
        {
     var c = SelectedProductComponents[i];
       
      if (c.ComponentId == Guid.Empty)
        {
       yield return new ValidationResult(
 $"Component at position {i + 1} is required.",
  new[] { $"{nameof(SelectedProductComponents)}[{i}].{nameof(c.ComponentId)}" });
   }

     if (c.RequiredQuantity <= 0)
      {
            yield return new ValidationResult(
         $"Quantity for component at position {i + 1} must be greater than zero.",
             new[] { $"{nameof(SelectedProductComponents)}[{i}].{nameof(c.RequiredQuantity)}" });
     }
     }
     }
        }
    }
}