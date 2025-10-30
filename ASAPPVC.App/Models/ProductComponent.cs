using ASAPPVC.App.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.App.Models
{
    /// <summary>
    /// Bridge entity linking a Product to many Components with a required Quantity (per set).<br/>
    /// Uses a composite key of ProductId and ComponentId.
    /// </summary>
    public class ProductComponent
    {
        // ===============================
        // Core Identification
        // ===============================

        // Foreign key to the Product
        [Required, ForeignKey(nameof(Product))]
        public Guid ProductId { get; set; }

        // Foreign key to the Component
        [Required, ForeignKey(nameof(Component))]
        public Guid ComponentId { get; set; }

        // ===============================
        // Quantity Requirements
        // ===============================

        // How many component units are required to make ONE product unit.
        [Required, Column(TypeName = "decimal(18,4)")]
        [Range(0.0001, double.MaxValue, ErrorMessage = "Quantity per unit must be > 0")]
        public decimal RequiredQuantity { get; set; } = 1m;

        public Unit UnitOfMeasure { get; set; }

        // ==============================
        // Snapshot of Unit Cost
        // ==============================
        [Required, Column(TypeName = "decimal(18,2)")]
        public decimal UnitCost { get; set; }

        // ===============================
        // Audit
        // ===============================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ===============================
        // Navigation
        // ===============================

        public Product? Product { get; set; }
        public Component? Component { get; set; }
    }
}