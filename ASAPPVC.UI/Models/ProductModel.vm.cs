using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models
{
    //───────────────────────────────────────────────\\
    //  Product ViewModels (Read + Write)
    //───────────────────────────────────────────────\\
    //
    //  Purpose:
    //  - Keep all VMs for the Product entity together
    //  - Support list, detail, and create/edit screens
    //  - Use clean, flat naming: ProductListVm, ProductDetailVm, etc.
    //
    //  Convention:
    //  - List VMs → lightweight summaries for tables/cards
    //  - Detail VMs → include nested component info
    //  - Form VMs → used in POST forms with validation
    //───────────────────────────────────────────────\\

    //───────────────────────────────────────────────\\
    // Summary (used in product lists, search results)
    //───────────────────────────────────────────────\\
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

    //───────────────────────────────────────────────\\
    // Detail (used for view screen)
    //───────────────────────────────────────────────\\
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
        public List<ProductComponentVm> Components { get; init; } = [];
    }

    //───────────────────────────────────────────────\\
    // Component inside product (read-only view)
    //───────────────────────────────────────────────\\
    public sealed class ProductComponentVm
    {
        public Guid ComponentId { get; init; }
        public string ComponentCode { get; init; } = string.Empty;
        public string ComponentName { get; init; } = string.Empty;
        public decimal QuantityRequired { get; init; }
        public decimal UnitCost { get; init; }

        // Optional computed cost for UI
        public decimal TotalCost => UnitCost * QuantityRequired;
    }

    //───────────────────────────────────────────────\\
    // Create form (used in POST / add product)
    //───────────────────────────────────────────────\\
    public sealed class CreateProductVm
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, Range(0.01, double.MaxValue, ErrorMessage = "Price must be positive.")]
        public decimal Price { get; set; }

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Image upload (optional)
        public byte[]? ImageBytes { get; set; }

        public string? ImageContentType { get; set; }

        // Components chosen from inventory (optional)
        public List<CreateProductComponentVm> Components { get; set; } = [];
    }

    public sealed class CreateProductComponentVm
    {
        [Required]
        public Guid ComponentId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal QuantityRequired { get; set; }
    }

    //───────────────────────────────────────────────\\
    // Edit form (used in PUT / update product)
    //───────────────────────────────────────────────\\
    public sealed class EditProductVm
    {
        [Required]
        public Guid Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Image editing (optional)
        public byte[]? ImageBytes { get; set; }

        public string? ImageContentType { get; set; }

        // Editable components
        public List<EditProductComponentVm> Components { get; set; } = [];
    }

    public sealed class EditProductComponentVm
    {
        public Guid Id { get; set; } // Bridge ID if you track it
        [Required] public Guid ComponentId { get; set; }
        [Range(0.01, double.MaxValue)] public decimal QuantityRequired { get; set; }
    }
}
}