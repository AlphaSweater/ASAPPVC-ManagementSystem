using ASAPPVC.App.Models.Enums;
using ASAPPVC.App.Models.General;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.App.Models
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
        public AppImage? Image { get; set; }

        // Navigation property for parts (bridge rows)
        public List<ProductComponent> ProductComponents { get; set; } = new();

        // Enum-typed modifiers stored as simple columns.
        // Each holds a single enum value (None when not set).

        public Category Category { get; set; } = Category.None;
        public Material Material { get; set; } = Material.None;
        public Colour Colour { get; set; } = Colour.None;
    }
}