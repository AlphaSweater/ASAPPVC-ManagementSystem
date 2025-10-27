using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Models
{
    /// <summary>
    /// Strongly-typed request for code generation.
    /// </summary>
    public sealed class CodeGenerationRequest
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
}