using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.UI.Models
{
    [Index(nameof(ProductCode), IsUnique = true)]
    public class Product
    {
        // Internal GUID primary key for safe relations
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Public human-friendly code (generated elsewhere)
        [Required, MaxLength(64)]
        public string ProductCode { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        // Optional image data
        public byte[] ImageData { get; set; } = Array.Empty<byte>();

        [MaxLength(50)]
        public string ImageType { get; set; } = string.Empty;

        // Navigation property for parts (bridge rows)
        public List<ProductComponent> ProductComponents { get; set; } = new();

        // Typed modifiers object stored as JSON. Each property holds a3-char code (or null):
        // - Category (e.g. "WIN")
        // - Colour (e.g. "RED")
        // - Material (e.g. "ALU")
        public ProductModifiers Modifiers { get; set; } = new();
    }

    // Small DTO representing product modifiers by type.
    // Stored on Product as JSON (TEXT column). Keep simple — just three3-char code fields.
    public class ProductModifiers
    {
        [MaxLength(3)]
        public string? Category { get; set; }

        [MaxLength(3)]
        public string? Colour { get; set; }

        [MaxLength(3)]
        public string? Material { get; set; }

        public bool IsEmpty()
            => string.IsNullOrWhiteSpace(Category)
            && string.IsNullOrWhiteSpace(Colour)
            && string.IsNullOrWhiteSpace(Material);
    }
}