using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models
{
    /// <summary>
    /// Tracks last used sequence numbers for each code type and (optionally) period.
    /// Composite primary key (CodeType + PeriodKey) ensures atomic upserts.
    /// </summary>
    [PrimaryKey(nameof(CodeType), nameof(PeriodKey))]
    public class CodeCounters
    {
        /// <summary>
        /// Code type name (e.g. "Product", "Order", "Component", "PickingSlip")
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string CodeType { get; set; } = string.Empty;

        /// <summary>
        /// Optional period key for resets (e.g. "202410" for October 2024 orders)
        /// </summary>
        [MaxLength(10)]
        public string? PeriodKey { get; set; }

        /// <summary>
        /// Last issued sequential number for this type/period.
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        public int LastNumber { get; set; } = 0;

        /// <summary>
        /// Timestamp of last counter update (UTC).
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}