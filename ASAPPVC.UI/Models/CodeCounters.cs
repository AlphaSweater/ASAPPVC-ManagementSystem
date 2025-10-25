using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ASAPPVC.UI.Models
{
    [Table("CodeCounters")]
    [Index(nameof(CodeType), nameof(PeriodKey))]
    public class CodeCounters
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string CodeType { get; set; } = null!;

        [MaxLength(10)]
        public string? PeriodKey { get; set; }

        public int LastNumber { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }
    }
}