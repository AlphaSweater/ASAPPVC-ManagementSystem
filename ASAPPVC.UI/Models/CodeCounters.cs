using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models
{
    [Index(nameof(CodeType), nameof(PeriodKey))]
    public class CodeCounters
    {
        // Internal GUID primary key for safe relations
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Type of code (Product, Component, Order, PickingSlip)
        [Required]
        public string CodeType { get; set; } = string.Empty;

        // Optional period key (e.g., YYYYMM for monthly resets)
        [MaxLength(10)]
        public string? PeriodKey { get; set; }

        // Last used number for this code type / period
        [Required]
        [Range(0, int.MaxValue)]
        public int LastNumber { get; set; } = 0;

        // Last updated timestamp
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}