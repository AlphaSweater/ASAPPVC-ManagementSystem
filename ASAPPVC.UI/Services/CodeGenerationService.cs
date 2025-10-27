using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.Enums;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Utils;
using System.Text.RegularExpressions;

namespace ASAPPVC.UI.Services
{
    /// <summary>
    /// Service for generating unique, checksummed codes for Products, Components, Orders, and PickingSlips.
    /// Implements sequential numbering with optional period-based resets and customizable formats.
    /// </summary>
    public class CodeGenerationService : ICodeGenerationService
    {
        private readonly ICodeCountersRepository _counters;

        // Compiled regex patterns for performance
        private static readonly Regex _alphaNumericRegex = new(@"[^A-Z0-9]", RegexOptions.Compiled);

        private static readonly Regex _orderCodeRegex = new(@"ORD[^0-9]*([0-9]{3,6})", RegexOptions.Compiled);
        private static readonly Regex _whitespaceRegex = new(@"\s+", RegexOptions.Compiled);

        public CodeGenerationService(ICodeCountersRepository countersRepository)
        {
            _counters = countersRepository ?? throw new ArgumentNullException(nameof(countersRepository));
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Generate a new code based on request parameters
        public async Task<Result<string>> GenerateCodeAsync(CodeGenerationRequest request, CancellationToken ct = default)
        {
            // Validate request
            var (isValid, validationError) = ValidateRequest(request);
            if (!isValid)
                return Result<string>.Fail(validationError!);

            try
            {
                var now = request.When?.ToUniversalTime() ?? DateTime.UtcNow;
                string prefix = GetPrefixForType(request.Type);

                // Determine if this code type uses period-based resets
                string? periodKey = request.Type == CodeType.Order ? now.ToString("yyyyMM") : null;

                // Build scope key for counter lookup
                string scopeKey = $"{request.Type}-{periodKey ?? "GLOBAL"}";

                // Get next sequential number
                var nextNumberResult = await GetNextCounterValueAsync(scopeKey, ct);
                if (!nextNumberResult.Ok)
                    return Result<string>.Fail(nextNumberResult.Error!);

                int nextNumber = nextNumberResult.Value;

                // Build the code base (without checksum)
                string codeBase = BuildCodeBase(request, prefix, nextNumber, now);

                // Compute and append checksum
                char checksum = ComputeChecksum(codeBase);
                string fullCode = $"{codeBase}-{checksum}";

                return Result<string>.Success(fullCode);
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

            // Code format: XXX-...-C (checksum is last character after final dash)
            var parts = code.Split('-');
            if (parts.Length < 2)
                return false;

            string checksumPart = parts[^1];
            if (checksumPart.Length != 1)
                return false;

            // Reconstruct code base without checksum
            string codeBase = string.Join("-", parts[..^1]);

            // Recompute checksum
            char expectedChecksum = ComputeChecksum(codeBase);

            return checksumPart[0] == expectedChecksum;
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

            if (request.Type == CodeType.PickingSlip && string.IsNullOrWhiteSpace(request.RelatedCode))
                return (false, "RelatedCode is required for PickingSlip generation.");

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
                var counter = await _counters.GetByTypeAndPeriodAsync("SCOPE", scopeKey, ct);

                if (counter == null)
                {
                    // Create new counter starting at 1
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

                // Increment existing counter
                counter.LastNumber++;
                counter.UpdatedAt = DateTime.UtcNow;
                await _counters.SaveAsync(ct);

                return Result<int>.Success(counter.LastNumber);
            }
            catch (Exception ex)
            {
                return Result<int>.Fail($"Failed to retrieve counter: {ex.Message}");
            }
        }

        /// <summary>
        /// Builds the code base string (without checksum) based on request parameters.
        /// </summary>
        private static string BuildCodeBase(CodeGenerationRequest request, string prefix, int number, DateTime timestamp)
        {
            var category = SanitizeAlphaNumeric(request.Category, 3);
            var version = $"V{Math.Clamp(request.Version ?? 1, 1, 99):00}";

            string codeBase = request.Type switch
            {
                CodeType.Product => BuildProductCode(prefix, category, number, request.Version.HasValue ? version : null),
                CodeType.Component => BuildComponentCode(prefix, category, number),
                CodeType.Order => BuildOrderCode(prefix, timestamp, number),
                CodeType.PickingSlip => BuildPickingSlipCode(prefix, request.RelatedCode!, version, number),
                _ => $"{prefix}-{number:0000}"
            };

            return codeBase.ToUpperInvariant();
        }

        /// <summary>
        /// Builds a product code: PRD(-CAT3)?-SERIAL(-REV2)?
        /// Example: PRD-WIN-0001-V01 or PRD-0042
        /// </summary>
        private static string BuildProductCode(string prefix, string category, int number, string? version)
        {
            var code = prefix;

            if (!string.IsNullOrEmpty(category))
                code += $"-{category}";

            code += $"-{number:0000}";

            if (version != null)
                code += $"-{version}";

            return code;
        }

        /// <summary>
        /// Builds a component code: CMP(-CAT3)?-SERIAL
        /// Example: CMP-DRR-00001 or CMP-00042
        /// </summary>
        private static string BuildComponentCode(string prefix, string category, int number)
        {
            var code = prefix;

            if (!string.IsNullOrEmpty(category))
                code += $"-{category}";

            code += $"-{number:00000}";

            return code;
        }

        /// <summary>
        /// Builds an order code: ORD-YYYYMM-SERIAL
        /// Example: ORD-202401-0042
        /// </summary>
        private static string BuildOrderCode(string prefix, DateTime timestamp, int number)
        {
            return $"{prefix}-{timestamp:yyyyMM}-{number:0000}";
        }

        /// <summary>
        /// Builds a picking slip code: PSL-ORDxxxx-Vxx
        /// Example: PSL-ORD0042-V01
        /// </summary>
        private static string BuildPickingSlipCode(string prefix, string relatedOrderCode, string version, int fallbackNumber)
        {
            var orderShort = ExtractOrderShortCode(relatedOrderCode) ?? $"ORD{fallbackNumber:0000}";
            return $"{prefix}-{orderShort}-{version}";
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

        /// <summary>
        /// Sanitizes input to alphanumeric characters only and truncates to max length.
        /// </summary>
        private static string SanitizeAlphaNumeric(string? input, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var cleaned = _alphaNumericRegex.Replace(input.ToUpperInvariant(), "");
            return cleaned.Length <= maxLength ? cleaned : cleaned[..maxLength];
        }

        /// <summary>
        /// Extracts the last 4 digits from an order code.
        /// Example: "ORD-202401-0042-X" → "ORD0042"
        /// </summary>
        private static string? ExtractOrderShortCode(string? orderCode)
        {
            if (string.IsNullOrWhiteSpace(orderCode))
                return null;

            var match = _orderCodeRegex.Match(orderCode.ToUpperInvariant());
            if (match.Success)
            {
                string digits = match.Groups[1].Value;
                // Take last 4 digits
                return $"ORD{digits[^4..]}";
            }

            return null;
        }

        /// <summary>
        /// Computes a base-36 checksum character for the given string.
        /// </summary>
        private static char ComputeChecksum(string codeBase)
        {
            var cleaned = _whitespaceRegex.Replace(codeBase.ToUpperInvariant(), "");
            int sum = 0;

            foreach (char ch in cleaned)
            {
                int value = ch switch
                {
                    >= '0' and <= '9' => ch - '0',
                    >= 'A' and <= 'Z' => 10 + (ch - 'A'),
                    _ => 0
                };
                sum = (sum * 31 + value) % 36;
            }

            return (char)(sum < 10 ? '0' + sum : 'A' + (sum - 10));
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\