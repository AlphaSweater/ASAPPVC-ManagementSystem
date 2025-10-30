using ASAPPVC.App.Models.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.App.Models
{
    [Index(nameof(OrderCode), IsUnique = true)]
    public class Order
    {
        // ===============================
        // Core Identification
        // ===============================

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(64)]
        public string OrderCode { get; set; } = string.Empty; // e.g., ORD-202510-0001

        // Who placed this order (customer account)
        [Required, ForeignKey(nameof(Customer))]
        public Guid CustomerId { get; set; }

        public Customer? Customer { get; set; }

        // Who created the order in the system (your staff/employee)
        [Required, ForeignKey(nameof(CreatedBy))]
        public Guid CreatedByUserId { get; set; }

        public ApplicationUser? CreatedBy { get; set; }

        // Optional: who last updated (for audit trails)
        [ForeignKey(nameof(UpdatedBy))]
        public Guid? UpdatedByUserId { get; set; }

        public ApplicationUser? UpdatedBy { get; set; }

        // ===============================
        // Dates & Lifecycle
        // ===============================

        [Required]
        public DateTime OrderDate { get; set; } = DateTime.UtcNow; // when order was placed

        // ===============================
        // Statuses
        // ===============================

        [Required]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending; // your existing enum

        // ===============================
        // Money (Stored and Set in Services)
        // ===============================

        [Required, MaxLength(3)]
        public string Currency { get; set; } = "ZAR";

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; } = 0m;   // sum of lines before tax/discount

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; } = 0m;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Total { get; set; } = 0m;      // Subtotal - Discount + Tax + Shipping

        // ===============================
        // Audit
        // ===============================

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // ===============================
        // Relationships
        // ===============================

        public List<OrderProduct> OrderProducts { get; set; } = new();
    }
}