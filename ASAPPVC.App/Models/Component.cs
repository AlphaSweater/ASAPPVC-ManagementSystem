using ASAPPVC.App.Models.Enums;
using ASAPPVC.App.Models.General;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.App.Models
{
    [Index(nameof(ComponentCode), IsUnique = true)]
    public class Component
    {
        // ===============================
        // Core Identification
        // ===============================

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(64)]
        public string ComponentCode { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string ComponentName { get; set; } = string.Empty;

        // ===============================
        // Classification
        // ===============================

        public Material MaterialType { get; set; } = Material.None;
        public Colour ColourOption { get; set; } = Colour.None;

        // ===============================
        // Stock & Cost Data
        // ===============================

        [Required, Column(TypeName = "decimal(18,4)")]
        public decimal QuantityOnHand { get; set; }

        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }       // “Cost” → Cost of one component unit

        [Required]
        public Unit UnitOfMeasure { get; set; } = Unit.Piece;

        // ===============================
        // Storage & Location
        // ===============================

        // Examples: "A-2", "B-12", "R1-05", "PACK-1"
        [Required, MaxLength(32)]
        [RegularExpression(@"^[A-Za-z0-9]+(?:-[A-Za-z0-9]+)*$", ErrorMessage = "Use letters/numbers with optional dashes, e.g. A-2 or B-12.")]
        public string? LocationCode { get; set; }

        [MaxLength(100)]
        public string? LocationNote { get; set; } // optional: “top shelf”, “fragile area”, etc.

        // ===============================
        // Inventory Management
        // ===============================

        public decimal ReorderLevel { get; set; } = 0;     // Threshold for restocking alerts

        // ===============================
        // Media / References
        // ===============================

        public AppImage? Image { get; set; }

        // ===============================
        // Lifecycle / Audit
        // ===============================

        public bool IsActive { get; set; } = true;         // Soft disable when discontinued
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ===============================
        // Relationships (Not mapped)
        // ===============================

        public ICollection<ProductComponent>? ProductComponents { get; set; }
    }
}