using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.Enums;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Utils;
using System.Text;

namespace ASAPPVC.UI.Services
{
    /// <summary>
    /// Generates unique, checksummed codes for Products, Components, Orders, and PickingSlips.
    /// Sequential numbering with optional period-based resets and customizable formats.
    ///
    /// Formats (before checksum):
    /// - Product     : PRD(-CAT3)?-NNNN(-Vxx)?
    /// - Component   : CMP(-CAT3)?-NNNNN
    /// - Order       : ORD-YYYYMM-NNNN            (period always before serial)
    /// - PickingSlip : PSL-ORDNNNN-Vxx            (uses short order form: ORD####)
    ///
    /// Final: "<base>-<checksum>" (checksum is single base-36 char).
    /// </summary>
    public class CodeGenerationService(ICodeCountersRepository countersRepository) : ICodeGenerationService
    {
        private readonly ICodeCountersRepository _counters = countersRepository ?? throw new ArgumentNullException(nameof(countersRepository));

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Generate a new code based on request parameters
        public async Task<Result<string>> GenerateCodeAsync(CodeGenerationRequest request, CancellationToken ct = default)
        {
            // Validate request (fast-fail)
            var (isValid, validationError) = ValidateRequest(request);
            if (!isValid)
                return Result<string>.Fail(validationError!);

            try
            {
                // Freeze time for the whole operation
                var nowUtc = (request.When ?? DateTime.UtcNow).ToUniversalTime();

                // Prefix per-type (PRD/CMP/ORD/PSL)
                string prefix = GetPrefixForType(request.Type);

                // Period scoping (only Orders reset monthly)
                string? periodKey = request.Type == CodeType.Order ? nowUtc.ToString("yyyyMM") : null;

                // Counter scope (Type + period bucket)
                string scopeKey = $"{request.Type}-{periodKey ?? "GLOBAL"}";

                // Next number from counter store
                var nextNumberResult = await GetNextCounterValueAsync(scopeKey, ct);
                if (!nextNumberResult.Ok)
                    return Result<string>.Fail(nextNumberResult.Error!);

                int nextNumber = nextNumberResult.Value;

                // Build base (no checksum)
                string codeBase = BuildCodeBase(request, prefix, nextNumber, nowUtc);

                // Compute checksum over normalized, alphanumeric-only form
                char checksum = ComputeChecksum(codeBase);

                // Final form: base + "-" + checksum
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

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Validate a code's checksum
        public bool ValidateChecksum(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return false;

            // checksum is last char after the final '-'
            int lastDash = code.LastIndexOf('-');
            if (lastDash <= 0 || lastDash == code.Length - 1)
                return false;

            string codeBase = code[..lastDash];
            string checksumPart = code[(lastDash + 1)..];

            if (checksumPart.Length != 1)
                return false;

            char expected = ComputeChecksum(codeBase);
            return checksumPart[0] == expected;
        }

        // ===================================================================
        // PRIVATE HELPERS
        // ===================================================================

        /// <summary>
        /// Validates the code generation request.
        /// </summary>
        private static (bool isValid, string? error) ValidateRequest(CodeGenerationRequest? request)
        {
            if (request is null)
                return (false, "Code generation request cannot be null.");

            if (!Enum.IsDefined(typeof(CodeType), request.Type))
                return (false, "Invalid code type specified.");

            // PickingSlip needs a related Order code and a version (for revisions).
            if (request.Type == CodeType.PickingSlip)
            {
                if (string.IsNullOrWhiteSpace(request.RelatedCode))
                    return (false, "RelatedCode is required for PickingSlip generation.");
                if (!request.Version.HasValue)
                    return (false, "Version is required for PickingSlip generation.");
            }

            if (request.Version.HasValue && (request.Version.Value < 1 || request.Version.Value > 99))
                return (false, "Version must be between 1 and 99.");

            return (true, null);
        }

        /// <summary>
        /// Retrieves and increments the counter for the given scope.
        /// </summary>
        private async Task<Result<int>> GetNextCounterValueAsync(string scopeKey, CancellationToken ct)
        {
            try
            {
                // NOTE: Consider making this atomic in the repository (single round-trip UPDATE LastNumber=LastNumber+1 ... RETURNING).
                var counter = await _counters.GetByTypeAndPeriodAsync("SCOPE", scopeKey, ct);

                if (counter is null)
                {
                    // First number
                    counter = new CodeCounters
                    {
                        CodeType = "SCOPE",
                        PeriodKey = scopeKey,
                        LastNumber = 1,
                        UpdatedAt = DateTime.UtcNow
                    };

                    await _counters.AddAndSaveAsync(counter, ct);
                    return Result<int>.Success(counter.LastNumber);
                }

                // Increment existing
                counter.LastNumber++;
                counter.UpdatedAt = DateTime.UtcNow;
                await _counters.SaveAsync(ct);

                return Result<int>.Success(counter.LastNumber);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return Result<int>.Fail("Counter retrieval was canceled.");
            }
            catch (Exception ex)
            {
                return Result<int>.Fail($"Failed to retrieve counter: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds the code base string (without checksum) based on request parameters.
        /// </summary>
        private static string BuildCodeBase(CodeGenerationRequest request, string prefix, int number, DateTime timestampUtc)
        {
            // Normalize inputs once
            string category = KeepAlnumUpper(request.Category, 3);
            string? version = request.Version.HasValue
                ? $"V{Math.Clamp(request.Version.Value, 1, 99):00}"
                : null;

            string codeBase = request.Type switch
            {
                CodeType.Product => BuildProductCode(prefix, category, number, version),
                CodeType.Component => BuildComponentCode(prefix, category, number),
                CodeType.Order => BuildOrderCode(prefix, timestampUtc, number),
                CodeType.PickingSlip => BuildPickingSlipCode(prefix, request.RelatedCode!, version!, number),
                _ => $"{prefix}-{number:0000}"
            };

            // Ensure final casing is consistent
            return codeBase.ToUpperInvariant();
        }

        /// <summary>
        /// Product base: PRD(-CAT3)?-NNNN(-Vxx)?
        /// Example: PRD-WIN-0001-V01 or PRD-0042
        /// </summary>
        private static string BuildProductCode(string prefix, string category, int number, string? version)
        {
            if (!string.IsNullOrEmpty(category) && version is not null)
                return $"{prefix}-{category}-{number:0000}-{version}";

            if (!string.IsNullOrEmpty(category))
                return $"{prefix}-{category}-{number:0000}";

            if (version is not null)
                return $"{prefix}-{number:0000}-{version}";

            return $"{prefix}-{number:0000}";
        }

        /// <summary>
        /// Component base: CMP(-CAT3)?-NNNNN
        /// Example: CMP-DRR-00001 or CMP-00042
        /// </summary>
        private static string BuildComponentCode(string prefix, string category, int number)
        {
            return string.IsNullOrEmpty(category)
                ? $"{prefix}-{number:00000}"
                : $"{prefix}-{category}-{number:00000}";
        }

        /// <summary>
        /// Order base: ORD-YYYYMM-NNNN
        /// Example: ORD-202401-0042
        /// </summary>
        private static string BuildOrderCode(string prefix, DateTime timestampUtc, int number)
        {
            return $"{prefix}-{timestampUtc:yyyyMM}-{number:0000}";
        }

        /// <summary>
        /// Picking Slip base: PSL-ORD####-Vxx
        /// - Extracts "ORD####" from Related Order code (strict ORD-YYYYMM-####[-Vxx][-C]).
        /// - If extraction fails, falls back to current slip sequence as "ORD####".
        /// Example target: PSL-ORD0042-V01
        /// (Final code will be PSL-ORD0042-V01-<checksum>).
        /// </summary>
        private static string BuildPickingSlipCode(string prefix, string relatedOrderCode, string version, int fallbackNumber)
        {
            var shortOrd = TryGetOrderShort(relatedOrderCode, out var s)
                ? s
                : $"ORD{fallbackNumber:0000}";

            return $"{prefix}-{shortOrd}-{version}";
        }

        /// <summary>
        /// Attempts to convert a strict Order code:
        ///   ORD-YYYYMM-####[-Vxx][-C]
        /// into a short form "ORD####".
        /// Returns true if successful and outputs 'shortCode'.
        /// </summary>
        private static bool TryGetOrderShort(string? orderCode, out string shortCode)
        {
            shortCode = string.Empty;
            if (string.IsNullOrWhiteSpace(orderCode))
                return false;

            // Upper + trim
            var s = orderCode.Trim().ToUpperInvariant();

            // Strip trailing checksum segment if last '-' part is a single char
            int lastDash = s.LastIndexOf('-');
            if (lastDash > 0 && lastDash < s.Length - 1 && (s.Length - (lastDash + 1)) == 1)
                s = s[..lastDash];

            // Split parts
            var parts = s.Split('-');
            // Must be ORD-YYYYMM-#### or ORD-YYYYMM-####-Vxx
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

            // Optional Vxx (if present)
            if (parts.Length == 4)
            {
                var v = parts[3];
                if (v.Length != 3 || v[0] != 'V' || !char.IsDigit(v[1]) || !char.IsDigit(v[2]))
                    return false;
            }

            shortCode = $"ORD{parts[2]}";
            return true;
        }

        /// <summary>
        /// Returns the prefix for each code type.
        /// </summary>
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

        /// <summary>
        /// Computes a base-36 checksum character for the given string.
        /// Ignores any non-alphanumeric separators (hyphens, spaces, etc.).
        /// </summary>
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

        /// <summary>
        /// Keeps only alphanumeric characters, uppercases, and truncates to max length (regex-free).
        /// </summary>
        private static string KeepAlnumUpper(string? input, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(input) || maxLength <= 0)
                return string.Empty;

            var sb = new StringBuilder(Math.Min(input.Length, maxLength));
            for (int i = 0; i < input.Length && sb.Length < maxLength; i++)
            {
                char c = input[i];
                if (char.IsLetterOrDigit(c))
                    sb.Append(char.ToUpperInvariant(c));
            }
            return sb.ToString();
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\