using ASAPPVC.UI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models
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
        public string Name { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string Description { get; init; } = string.Empty;

        public bool HasImage { get; set; }
        public string? ThumbUrl { get; init; }

        // Distinct number of components linked to this product
        public int ComponentCount { get; init; }

        // Enum modifiers

        public Category Category { get; init; } = Category.None;
        public Material Material { get; init; } = Material.None;
        public Colour Colour { get; init; } = Colour.None;

        // Optional computed fields for UI display
        public string DisplayPrice => Price.ToString("C"); // UI currency format

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
        public string Name { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string Description { get; init; } = string.Empty;

        public bool HasImage { get; set; }
        public string? ImageUrl { get; init; }
        public string? ImageEtag { get; init; }

        // Enum modifiers
        public Category Category { get; init; } = Category.None;

        public Material Material { get; init; } = Material.None;
        public Colour Colour { get; init; } = Colour.None;

        // Linked components
        public List<ProductComponentVm> Components { get; init; } = new();
    }

    //-----------------------------------------------\\
    // Create form (used in POST / add product)
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
        [StringLength(64)]
        public string? ProductCode { get; set; }

        [Display(Name = "Product Name")]
        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Product name must be between 2 and 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "Unit Price")]
        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 999999, ErrorMessage = "Price must be a positive amount.")]
        public decimal Price { get; set; }

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

        [Display(Name = "Material")]
        public Material Material { get; set; } = Material.None;

        [Display(Name = "Colour")]
        public Colour Colour { get; set; } = Colour.None;

        // Component lines
        [Display(Name = "Components")]
        [MinLength(1, ErrorMessage = "A product requires at least one component.")]
        public List<ProductComponentFormVm> Components { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsEdit && string.IsNullOrWhiteSpace(ProductCode))
            {
                yield return new ValidationResult(
                    "Product code is required when editing.",
                    new[] { nameof(ProductCode) });
            }

            if (Components is { Count: > 0 })
            {
                for (int i = 0; i < Components.Count; i++)
                {
                    var c = Components[i];

                    if (c.Quantity <= 0)
                    {
                        yield return new ValidationResult(
                            "Quantity must be at least 1.",
                            new[] { $"{nameof(Components)}[{i}].{nameof(ProductComponentFormVm.Quantity)}" });
                    }
                }
            }
        }
    }
}