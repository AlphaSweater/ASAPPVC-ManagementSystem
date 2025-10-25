using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories;
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

    public class CodeGenerator(ICodeCountersRepository countersRepo) : ICodeGenerator
    {
        private readonly ICodeCountersRepository _countersRepo = countersRepo;

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
                    var nameCode = GenerateNameCode(name ?? "Item", 4);
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

            var code = baseCode.ToUpperInvariant();

            // append a simple mod-10 checksum to help detect typos
            var checksum = ChecksumUtils.ComputeChecksum(code);
            return $"{code}-{checksum}";
        }

        private async Task<int> GetNextCounterAsync(CodeType type, string? periodKey)
        {
            // Use periodKey for date-based resets (e.g., orders reset monthly)
            var counter = await _countersRepo.GetByTypeAndPeriodAsync(type.ToString(), periodKey);

            if (counter == null)
            {
                counter = new CodeCounters
                {
                    CodeType = type.ToString(),
                    PeriodKey = periodKey,
                    LastNumber = 1,
                    UpdatedAt = DateTime.UtcNow
                };

                counter = await _countersRepo.AddAndSaveAsync(counter);
                return counter.LastNumber;
            }
            else
            {
                counter.LastNumber++;
                counter.UpdatedAt = DateTime.UtcNow;
            }

            await _countersRepo.SaveAsync();
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