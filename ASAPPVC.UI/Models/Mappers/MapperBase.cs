namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Base class for all mappers, providing shared normalization and helper methods.
    /// Optionally accepts injected services for code generation and image processing.
    /// </summary>
    public abstract class MapperBase
    {
        protected readonly ICodeGenerator? CodeGenerator;
        protected readonly IImageService? ImageService;

        protected MapperBase(ICodeGenerator? codeGenerator = null, IImageService? imageService = null)
        {
            CodeGenerator = codeGenerator;
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

            if (CodeGenerator is not null)
            {
                var gen = (CodeGenerator.Generate(prefix) ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(gen))
                    return gen;
            }

            // Fallback: short stable unique-ish code
            return $"{prefix}-{Guid.NewGuid():N}"[..13];
        }
    }
}