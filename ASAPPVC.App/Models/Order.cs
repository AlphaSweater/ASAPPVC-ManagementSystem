using ASAPPVC.App.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.App.Models
{
    /// <summary>
    /// Domain model representing a customer order. This class contains only data and
    /// mapping annotations. Business logic and calculations should live in services.
    /// </summary>
    [Index(nameof(OrderCode), IsUnique = true)]
    public class Order
    {
        // ===============================
        // Core Identification
        // ===============================

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(64)]
        public string OrderCode { get; set; } = string.Empty; // Human-friendly unique order code (e.g. ORD-202510-0001).

        // ===============================
        // Relationships (FKs)
        // ===============================

        [Required, ForeignKey(nameof(Customer))]
        public Guid CustomerId { get; set; }

        public Customer? Customer { get; set; }

        [Required, ForeignKey(nameof(CreatedBy))]
        public Guid CreatedByUserId { get; set; }

        public ApplicationUser? CreatedBy { get; set; }

        [ForeignKey(nameof(UpdatedBy))]
        public Guid? UpdatedByUserId { get; set; }

        public ApplicationUser? UpdatedBy { get; set; }

        // ===============================
        // Dates
        // ===============================

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;

        // ===============================
        // Status
        // ===============================

        [Required]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        // ===============================
        // Money (stored snapshot values)
        // Services should populate and update these values.
        // ===============================

        [Required, StringLength(3, MinimumLength = 3)]
        public string Currency { get; set; } = "ZAR";

        [Precision(18, 2)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; } = 0m;

        [Precision(18, 2)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0m;

        [Precision(18, 2)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0m;

        [Precision(18, 2)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; } = 0m;

        // ===============================
        // Audit
        // ===============================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ===============================
        // Navigation
        // ===============================

        public List<OrderProduct> OrderProducts { get; set; } = new();
    }
}