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

        [Required, MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        // ===============================
        // Classification
        // ===============================

        public Category Category { get; set; } = Category.None;
        public Material MaterialType { get; set; } = Material.None;
        public Colour ColourOption { get; set; } = Colour.None;

        // ===============================
        // Pricing & Costing
        // ===============================

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ProductionCost { get; set; }

        [NotMapped]
        public decimal? GrossMarginAmount =>
            (ProductionCost.HasValue) ? SellingPrice - ProductionCost.Value : null;

        [NotMapped]
        public decimal? GrossMarginPercent =>
            (ProductionCost.HasValue && ProductionCost.Value > 0)
                ? (SellingPrice - ProductionCost.Value) / ProductionCost.Value * 100m
                : null;

        // ===============================
        // Inventory & Unit
        // ===============================

        [Required]
        public Unit UnitOfMeasure { get; set; } = Unit.Piece;

        [Column(TypeName = "decimal(18,4)")]
        public decimal PotentialQuantityOnHand { get; set; } = 0m; // This should be calculated based on what math of what components are required and in what quantities ? and then like how many of these productys we could make based on the component stock

        public decimal ReorderLevel { get; set; } = 0m;     // Trigger point
        public decimal ReorderQuantity { get; set; } = 0m;  // Suggested batch

        // ===============================
        // Content
        // ===============================

        [Required, MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        public AppImage? Image { get; set; }

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