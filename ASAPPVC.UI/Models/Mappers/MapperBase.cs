using ASAPPVC.UI.Models.Enums;
using ASAPPVC.UI.Services;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Base class for all mappers, providing shared normalization and helper methods.
    /// Optionally accepts injected services for code generation and image processing.
    /// </summary>
    public abstract class MapperBase
    {
        protected readonly ICodeGenerationService? CodeGenerationService;
        protected readonly IImageService? ImageService;

        protected MapperBase(ICodeGenerationService? codeGenerationService = null, IImageService? imageService = null)
        {
            CodeGenerationService = codeGenerationService;
            ImageService = imageService;
        }

        /// <summary>
        /// Normalizes a string by trimming whitespace. Returns empty string if null.
        /// </summary>
        protected static string NormalizeString(string? s)
        {
            return (s ?? string.Empty).Trim();
        }

        /// <summary>
        /// Normalizes a monetary value by clamping to zero if negative and rounding to 2 decimal places.
        /// </summary>
        protected static decimal NormalizeMoney(decimal amount)
        {
            return amount < 0 ? 0 : decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Converts image data to a Base64 data URL. Returns null if data is empty.
        /// Uses injected IImageService if available, otherwise falls back to direct conversion.
        /// </summary>
        protected string? AsDataUrlOrNull(byte[]? data, string? mime)
        {
            if (data is not { Length: > 0 })
                return null;

            var safeMime = string.IsNullOrWhiteSpace(mime) ? "image/png" : mime.Trim();

            if (ImageService is not null)
                return ImageService.GetDataUrl(data, safeMime);

            var b64 = Convert.ToBase64String(data);
            return $"data:{safeMime};base64,{b64}";
        }

        /// <summary>
        /// Normalizes a code or generates one using the provided prefix and optional code generator.
        /// Falls back to a GUID-based code if no generator is available.
        /// </summary>
        protected string NormalizeCodeOrGenerate(string? code, string prefix)
        {
            var trimmed = (code ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
                return trimmed;

            // Try using the new async code generation service if available. We call it synchronously
            // here because mapper APIs are synchronous; if generation fails we fall back to GUID.
            if (CodeGenerationService is not null)
            {
                try
                {
                    // Map simple prefix hints to CodeType. Keep it forgiving and case-insensitive.
                    var p = (prefix ?? string.Empty).Trim().ToUpperInvariant();
                    var type = p switch
                    {
                        var s when s.StartsWith("PROD") || s.StartsWith("PRO") => CodeType.Product,
                        var s when s.StartsWith("COMP") || s.StartsWith("COMP") => CodeType.Component,
                        var s when s.StartsWith("ORD") => CodeType.Order,
                        var s when s.StartsWith("PSL") || s.StartsWith("PICK") => CodeType.PickingSlip,
                        _ => CodeType.Product
                    };

                    var req = new CodeGenerationRequest { Type = type };
                    var res = CodeGenerationService.GenerateCodeAsync(req).GetAwaiter().GetResult();
                    if (res.Ok && !string.IsNullOrWhiteSpace(res.Value))
                        return res.Value.Trim();
                }
                catch
                {
                    // swallow and fallback to GUID-style code below
                }
            }

            // Fallback: short stable unique-ish code
            return $"{prefix}-{Guid.NewGuid():N}"[..13];
        }
    }
}