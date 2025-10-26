using ASAPPVC.UI.Models;
using ASAPPVC.UI.Repositories;
using System.Text.RegularExpressions;

namespace ASAPPVC.UI.Utils
{
    #region Interface

    public interface ICodeGenerator
    {
        Task<string> GenerateAsync(CodeRequest request, CancellationToken ct = default);
    }

    #endregion Interface

    #region Request Model

    /// <summary>
    /// Strongly-typed request for code generation.
    /// </summary>
    public sealed class CodeRequest
    {
        public CodeType Type { get; init; }

        // Optional descriptive category: e.g. "WIN" (window), "DRR" (door)
        public string? Category { get; init; }

        // Optional version or revision number (for products / picking slips)
        public int? Version { get; init; }

        // Optional related code (used by PickingSlips to tie to Order)
        public string? RelatedCode { get; init; }

        // Optional explicit timestamp for backfills
        public DateTime? When { get; init; }
    }

    #endregion Request Model

    #region Implementation

    public class CodeGenerator : ICodeGenerator
    {
        private readonly ICodeCountersRepository _counters;

        public CodeGenerator(ICodeCountersRepository countersRepo)
        {
            _counters = countersRepo;
        }

        public async Task<string> GenerateAsync(CodeRequest request, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var now = request.When?.ToUniversalTime() ?? DateTime.UtcNow;
            string prefix = GetPrefix(request.Type);
            string codeBase;

            // determine if period-based
            string? periodKey = request.Type == CodeType.Order ? now.ToString("yyyyMM") : null;

            // determine counter scope
            string scopeKey = $"{request.Type}-{periodKey ?? "GLOBAL"}";

            // get next number
            int next = await GetNextCounterAsync(scopeKey, ct);

            // derive category and version
            var cat3 = SanitizeAlphaNum(request.Category, 3);
            var rev2 = $"V{Math.Clamp(request.Version ?? 1, 1, 99):00}";

            // build code
            switch (request.Type)
            {
                case CodeType.Product:
                    // PRD(-CAT3)?-SERIAL(-REV2)?-CHK
                    codeBase = $"{prefix}"
                             + (string.IsNullOrEmpty(cat3) ? "" : $"-{cat3}")
                             + $"-{next:0000}"
                             + (request.Version.HasValue ? $"-{rev2}" : "");
                    break;

                case CodeType.Component:
                    // CMP(-CAT3)?-SERIAL-CHK
                    codeBase = $"{prefix}"
                             + (string.IsNullOrEmpty(cat3) ? "" : $"-{cat3}")
                             + $"-{next:00000}";
                    break;

                case CodeType.Order:
                    // ORD-YYYYMM-SERIAL-CHK
                    codeBase = $"{prefix}-{now:yyyyMM}-{next:0000}";
                    break;

                case CodeType.PickingSlip:
                    // PSL-ORDxxxx-Vxx-CHK
                    if (string.IsNullOrWhiteSpace(request.RelatedCode))
                        throw new ArgumentException("RelatedCode required for PickingSlip generation.");
                    var ordShort = DeriveOrderShort(request.RelatedCode) ?? $"ORD{next:0000}";
                    codeBase = $"{prefix}-{ordShort}-{rev2}";
                    break;

                default:
                    codeBase = $"{prefix}-{next:0000}";
                    break;
            }

            codeBase = codeBase.ToUpperInvariant();

            // compute checksum (simple base36 mod)
            var checksum = ComputeChecksum36(codeBase);

            return $"{codeBase}-{checksum}";
        }

        // ---------------------------------------------------------------------

        private async Task<int> GetNextCounterAsync(string scopeKey, CancellationToken ct)
        {
            var counter = await _counters.GetByTypeAndPeriodAsync("SCOPE", scopeKey, ct);

            if (counter == null)
            {
                counter = new CodeCounters
                {
                    CodeType = "SCOPE",
                    PeriodKey = scopeKey,
                    LastNumber = 1,
                    UpdatedAt = DateTime.UtcNow
                };
                await _counters.AddAndSaveAsync(counter, ct);
                return counter.LastNumber;
            }

            counter.LastNumber++;
            counter.UpdatedAt = DateTime.UtcNow;
            await _counters.SaveAsync(ct);
            return counter.LastNumber;
        }

        private static string GetPrefix(CodeType type)
        {
            return type switch
            {
                CodeType.Product => "PRD",
                CodeType.Component => "CMP",
                CodeType.Order => "ORD",
                CodeType.PickingSlip => "PSL",
                _ => "GEN"
            };
        }

        private static string SanitizeAlphaNum(string? input, int maxLen)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var clean = Regex.Replace(input.ToUpperInvariant(), @"[^A-Z0-9]", "");
            return clean.Length <= maxLen ? clean : clean[..maxLen];
        }

        private static string? DeriveOrderShort(string? orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
                return null;

            var m = Regex.Match(orderCode.ToUpperInvariant(), @"ORD[^0-9]*([0-9]{3,6})");
            if (m.Success)
                return $"ORD{m.Groups[1].Value[^4..]}";
            return null;
        }

        private static char ComputeChecksum36(string s)
        {
            var up = Regex.Replace(s.ToUpperInvariant(), @"\s+", "");
            int sum = 0;
            foreach (var ch in up)
            {
                int v = ch switch
                {
                    >= '0' and <= '9' => ch - '0',
                    >= 'A' and <= 'Z' => 10 + (ch - 'A'),
                    _ => 0
                };
                sum = (sum * 31 + v) % 36;
            }
            return (char)(sum < 10 ? '0' + sum : 'A' + (sum - 10));
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