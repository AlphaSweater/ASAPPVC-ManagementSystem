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
        // ===============================
        // Core Identification
        // ===============================

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(64)]
        public string ProductCode { get; set; } = string.Empty;

        // ===============================
        // Content
        // ===============================

        [Required, MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public AppImage? Image { get; set; }

        // ===============================
        // Classification
        // ===============================

        public Category Category { get; set; } = Category.None;
        public Material MaterialType { get; set; } = Material.None;
        public Colour ColourOption { get; set; } = Colour.None;

        // ===============================
        // Pricing & Cost
        // ===============================

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ProductionCost =>
            ProductComponents?.Sum(pc => pc.UnitCost * pc.RequiredQuantity);

        // ===============================
        // Inventory
        // ===============================

        public decimal ReorderLevel { get; set; } = 0m;     // Threshold for restocking alerts

        // ===============================
        // Lifecycle / Audit
        // ===============================

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ===============================
        // Relationships
        // ===============================
        public List<ProductComponent> ProductComponents { get; set; } = new();
    }
}