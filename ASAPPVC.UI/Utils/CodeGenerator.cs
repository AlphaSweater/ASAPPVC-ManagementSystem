using ASAPPVC.UI.Data;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace ASAPPVC.UI.Utils
{
    #region Interface

    public interface ICodeGenerator
    {
        Task<string> GenerateAsync(CodeType type, string? name = null, string? relatedId = null);
    }

    #endregion Interface

    #region Implementation

    // Implement the CodeGenerator interface (was previously inheriting from itself)
    public class CodeGenerator : ICodeGenerator
    {
        private readonly AppDbContext _context;

        public CodeGenerator(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateAsync(CodeType type, string? name = null, string? relatedId = null)
        {
            string prefix = GetPrefix(type);
            string baseCode = prefix;

            string? periodKey = type == CodeType.Order ? DateTime.UtcNow.ToString("yyyyMM") : null;

            // Get next counter number
            int nextNumber = await GetNextCounterAsync(type, periodKey);

            // Build base code depending on type
            switch (type)
            {
                case CodeType.Product:
                case CodeType.Component:
                    var nameCode = GenerateNameCode(name ?? "Item", 8);
                    baseCode += $"_{nameCode}_{nextNumber:000}";
                    break;

                case CodeType.Order:
                    baseCode += $"_{periodKey}_{nextNumber:000}";
                    break;

                case CodeType.PickingSlip:
                    if (string.IsNullOrWhiteSpace(relatedId))
                        throw new ArgumentException("relatedId required for PickingSlip generation.");
                    baseCode += $"_{relatedId}_V{nextNumber}";
                    break;
            }

            return baseCode.ToUpperInvariant();
        }

        private async Task<int> GetNextCounterAsync(CodeType type, string? periodKey)
        {
            // Use periodKey for date-based resets (e.g., orders reset monthly)
            var counter = await _context.CodeCounters
                .FirstOrDefaultAsync(c => c.CodeType == type.ToString() && c.PeriodKey == periodKey);

            if (counter == null)
            {
                counter = new CodeCounters
                {
                    CodeType = type.ToString(),
                    PeriodKey = periodKey,
                    LastNumber = 1,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.CodeCounters.Add(counter);
            }
            else
            {
                counter.LastNumber++;
                counter.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return counter.LastNumber;
        }

        private string GetPrefix(CodeType type)
        {
            return type switch
            {
                CodeType.Product => "PRO",
                CodeType.Component => "CMP",
                CodeType.Order => "ORD",
                CodeType.PickingSlip => "PIC",
                _ => "GEN"
            };
        }

        private string GenerateNameCode(string name, int maxLen)
        {
            var clean = Regex.Replace(name.ToUpper(), @"[^A-Z0-9]", "");
            if (clean.Length > maxLen)
                clean = clean[..maxLen];
            return clean;
        }
    }

    #endregion Implementation

    #region Model

    [Table("CodeCounters")]
    [Index(nameof(CodeType), nameof(PeriodKey))]
    public class CodeCounters
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string CodeType { get; set; } = default!;

        [MaxLength(10)]
        public string? PeriodKey { get; set; }

        public int LastNumber { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }
    }

    #endregion Model

    #region Enums

    public enum CodeType
    {
        Product,
        Component,
        Order,
        PickingSlip
    }

    #endregion Enums
}