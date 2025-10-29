using ASAPPVC.App.Models.Enums;
using ASAPPVC.App.Models.General;
using ASAPPVC.App.Repositories;
using ASAPPVC.App.Utils;
using System.Text;

namespace ASAPPVC.App.Services
{
    #region Interface

    /// <summary>
    /// Service contract for generating unique, checksummed codes for various entity types.
    /// Provides business logic for sequential code generation with configurable formats and period-based resets.
    /// </summary>
    public interface ICodeGenerationService
    {
        /// <summary>
        /// Generates a new unique code based on the provided request parameters.
        /// The generated code format varies by type (Product, Component, Order, PickingSlip)
        /// and includes an automatic checksum suffix for validation.
        /// </summary>
        /// <param name="request">
        /// A <see cref="CodeGenerationRequest"/> specifying the type, optional category, version,
        /// related code, and timestamp for code generation. Must not be null.
        /// </param>
        /// <param name="ct">Cancellation token for async operation.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the generated code string on success,
        /// or an error message on failure (e.g., validation error, database error).
        /// </returns>
        /// <example>
        /// Product code: PRD-WIN-0001-V01-A
        /// Component code: CMP-DRR-00001-B
        /// Order code: ORD-202401-0042-C
        /// PickingSlip code: PSL-ORD0042-V01-D
        /// </example>
        Task<Result<string>> GenerateCodeAsync(CodeGenerationRequest request, CancellationToken ct = default);

        /// <summary>
        /// Validates a code's checksum to verify its integrity.
        /// </summary>
        /// <param name="code">The complete code including checksum to validate.</param>
        /// <returns>
        /// True if the checksum is valid, false otherwise.
        /// </returns>
        bool ValidateChecksum(string code);
    }

    #endregion Interface

    /// <summary>
    /// Generates unique, checksummed codes for Products, Components, Orders, and PickingSlips.
    /// Uses atomic, DB-backed counters (per CodeType + optional period).
    ///
    /// Base formats (before checksum):
    /// - Product     : PRD(-CAT3)?-NNNN(-Vxx)?
    /// - Component   : CMP(-CAT3)?-NNNNN
    /// - Order       : ORD-YYYYMM-NNNN
    /// - PickingSlip : PSL-ORD####-Vxx   (short order form)
    ///
    /// Final: "<base>-<checksum>"  (checksum is base-36 single char).
    /// </summary>
    public sealed class CodeGenerationService(ICodeCountersRepository counters) : ICodeGenerationService
    {
        private readonly ICodeCountersRepository _counters = counters ?? throw new ArgumentNullException(nameof(counters));
        private const string GlobalPeriod = "GLOBAL";

        public async Task<Result<string>> GenerateCodeAsync(CodeGenerationRequest request, CancellationToken ct = default)
        {
            var (ok, err) = ValidateRequest(request);
            if (!ok)
                return Result<string>.Fail(err!);

            try
            {
                var nowUtc = (request.When ?? DateTime.UtcNow).ToUniversalTime();

                string prefix = GetPrefixForType(request.Type);
                string? periodKey = request.Type == CodeType.Order ? nowUtc.ToString("yyyyMM") : null;

                // Atomic: single round trip to increment and fetch
                int number = await _counters.IncrementAndGetAsync(request.Type, periodKey, ct);

                string codeBase = BuildCodeBase(request, prefix, number, nowUtc);
                char checksum = ComputeChecksum(codeBase);
                return Result<string>.Success($"{codeBase}-{checksum}");
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return Result<string>.Fail("Code generation was canceled.");
            }
            catch (Exception ex)
            {
                return Result<string>.Fail($"Failed to generate code: {ex.Message}");
            }
        }

        public bool ValidateChecksum(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            int lastDash = code.LastIndexOf('-');
            if (lastDash <= 0 || lastDash == code.Length - 1)
                return false;

            string codeBase = code[..lastDash];
            string check = code[(lastDash + 1)..];
            if (check.Length != 1)
                return false;

            return check[0] == ComputeChecksum(codeBase);
        }

        // ------------------- helpers -------------------

        private static (bool ok, string? error) ValidateRequest(CodeGenerationRequest? req)
        {
            if (req is null)
                return (false, "Code generation request cannot be null.");
            if (!Enum.IsDefined(typeof(CodeType), req.Type))
                return (false, "Invalid code type specified.");

            if (req.Type == CodeType.PickingSlip)
            {
                if (string.IsNullOrWhiteSpace(req.RelatedCode))
                    return (false, "RelatedCode is required for PickingSlip generation.");
                if (!req.Version.HasValue)
                    return (false, "Version is required for PickingSlip generation.");
            }

            if (req.Version is int v && (v < 1 || v > 99))
                return (false, "Version must be between 1 and 99.");

            return (true, null);
        }

        private static string BuildCodeBase(CodeGenerationRequest req, string prefix, int number, DateTime tsUtc)
        {
            string category = KeepAlnumUpper(req.Category, 3);
            string? version = req.Version.HasValue ? $"V{Math.Clamp(req.Version.Value, 1, 99):00}" : null;

            return (req.Type) switch
            {
                CodeType.Product => BuildProductCode(prefix, category, number, version),
                CodeType.Component => BuildComponentCode(prefix, category, number),
                CodeType.Order => $"{prefix}-{tsUtc:yyyyMM}-{number:0000}",
                CodeType.PickingSlip => BuildPickingSlipCode(prefix, req.RelatedCode!, version!, number),
                _ => $"{prefix}-{number:0000}"
            };
        }

        private static string BuildProductCode(string pfx, string category, int num, string? ver)
        {
            if (!string.IsNullOrEmpty(category) && ver is not null)
                return $"{pfx}-{category}-{num:0000}-{ver}";
            if (!string.IsNullOrEmpty(category))
                return $"{pfx}-{category}-{num:0000}";
            if (ver is not null)
                return $"{pfx}-{num:0000}-{ver}";
            return $"{pfx}-{num:0000}";
        }

        private static string BuildComponentCode(string pfx, string category, int num)
        {
            return string.IsNullOrEmpty(category) ? $"{pfx}-{num:00000}" : $"{pfx}-{category}-{num:00000}";
        }

        private static string BuildPickingSlipCode(string pfx, string relatedOrderCode, string version, int fallbackNum)
        {
            var shortOrd = TryGetOrderShort(relatedOrderCode, out var s) ? s : $"ORD{fallbackNum:0000}";
            return $"{pfx}-{shortOrd}-{version}";
        }

        private static bool TryGetOrderShort(string? orderCode, out string shortCode)
        {
            shortCode = string.Empty;
            if (string.IsNullOrWhiteSpace(orderCode))
                return false;

            var s = orderCode.Trim().ToUpperInvariant();

            // Drop trailing single-char checksum part if present
            int lastDash = s.LastIndexOf('-');
            if (lastDash > 0 && lastDash < s.Length - 1 && (s.Length - (lastDash + 1)) == 1)
                s = s[..lastDash];

            var parts = s.Split('-');
            if (parts.Length is not (3 or 4))
                return false;
            if (parts[0] != "ORD")
                return false;

            // YYYYMM
            if (parts[1].Length != 6 || !AllDigits(parts[1]))
                return false;
            // ####
            if (parts[2].Length != 4 || !AllDigits(parts[2]))
                return false;
            // optional Vxx
            if (parts.Length == 4)
            {
                var v = parts[3];
                if (v.Length != 3 || v[0] != 'V' || !char.IsDigit(v[1]) || !char.IsDigit(v[2]))
                    return false;
            }

            shortCode = $"ORD{parts[2]}";
            return true;
        }

        private static string GetPrefixForType(CodeType type)
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

        private static bool AllDigits(string s)
        {
            for (int i = 0; i < s.Length; i++)
                if (!char.IsDigit(s[i]))
                    return false;
            return true;
        }

        private static char ComputeChecksum(string codeBase)
        {
            var s = codeBase.ToUpperInvariant();
            int sum = 0;
            for (int i = 0; i < s.Length; i++)
            {
                char ch = s[i];
                int value =
                    (ch >= '0' && ch <= '9') ? (ch - '0') :
                    (ch >= 'A' && ch <= 'Z') ? (10 + (ch - 'A')) :
                    -1;
                if (value >= 0)
                    sum = (sum * 31 + value) % 36;
            }
            return (char)(sum < 10 ? '0' + sum : 'A' + (sum - 10));
        }

        private static string KeepAlnumUpper(string? input, int maxLen)
        {
            if (string.IsNullOrWhiteSpace(input) || maxLen <= 0)
                return string.Empty;
            var sb = new StringBuilder(Math.Min(input.Length, maxLen));
            for (int i = 0; i < input.Length && sb.Length < maxLen; i++)
            {
                char c = input[i];
                if (char.IsLetterOrDigit(c))
                    sb.Append(char.ToUpperInvariant(c));
            }
            return sb.ToString();
        }
    }
}