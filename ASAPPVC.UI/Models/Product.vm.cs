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

        // Optional small preview flag
        public bool HasImage { get; init; }

        // Distinct number of components linked to this product
        public int ComponentCount { get; init; }

        // Sum of quantities required across all components (for "pieces in a set")
        public decimal? TotalComponentQuantity { get; init; }

        // Optional computed fields for UI display
        public string DisplayPrice => Price.ToString("C"); // UI currency format

        // Helpful label if you want to show "3 comps" quickly in a chip/badge
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

        // Optional image display (converted to base64 in controller/service)
        public string? ImageBase64 { get; init; }

        // Linked parts/components
        public List<ProductComponentVm> Components { get; init; } = new();
    }

    //-----------------------------------------------\\
    // Create form (used in POST / add product)
    //-----------------------------------------------\\
    /// <summary>
    /// Form view model used when creating a new product (POST).<br/>
    /// Includes validation attributes for server-side model binding and a collection<br/>
    /// of component entries. Use this VM for create forms and endpoints that accept<br/>
    /// product creation data.<br/>
    /// </summary>
    public sealed class CreateProductVm
    {
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

        [Display(Name = "Image File (optional)")]
        public byte[]? ImageBytes { get; set; }

        public string? ImageContentType { get; set; }

        [Display(Name = "Components")]
        [MinLength(1)]
        public List<CreateProductComponentVm> Components { get; set; } = new();
    }

    //-----------------------------------------------\\
    // Edit form (used in PUT / update product)
    //-----------------------------------------------\\
    /// <summary>
    /// Form view model used when editing an existing product (PUT).<br/>
    /// Includes the product Id and validation attributes for update binding.<br/>
    /// Components include editable entries and may contain database ids for existing<br/>
    /// associations. Use this VM for edit forms and update endpoints.<br/>
    /// </summary>
    public sealed class EditProductVm
    {
        [Required(ErrorMessage = "Product ID is required.")]
        public Guid Id { get; set; }

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

        [Display(Name = "Image File (optional)")]
        public byte[]? ImageBytes { get; set; }

        public string? ImageContentType { get; set; }

        [Display(Name = "Components")]
        [MinLength(1)]
        public List<EditProductComponentVm> Components { get; set; } = new();
    }
}