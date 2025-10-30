using ASAPPVC.App.Models.Enums;
using FluentValidation;

namespace ASAPPVC.App.Models.Validation
{
    /// <summary>
    /// Common validator base class with shared helper methods and defaults.
    /// Inherit from this to reuse normalization, unit helpers and default cascade behavior.
    /// </summary>
    public abstract class BaseValidator<T> : AbstractValidator<T> where T : class
    {
        protected const decimal MaxDecimal = 1_000_000_000_000m; //1 trillion

        protected BaseValidator()
        {
            // Be concise with errors by default
            ClassLevelCascadeMode = CascadeMode.Stop;
            RuleLevelCascadeMode = CascadeMode.Stop;
        }

        /// <summary>
        /// Normalizes input by trimming and optionally applying case conversion.
        /// Returns null if input is null or empty after trimming.
        /// </summary>
        protected static string? Normalize(string? input, CaseMode mode = CaseMode.None)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            var s = input.Trim();
            return mode switch
            {
                CaseMode.ToUpper => s.ToUpperInvariant(),
                CaseMode.ToLower => s.ToLowerInvariant(),
                _ => s,
            };
        }

        protected enum CaseMode
        {
            None,
            ToUpper,
            ToLower
        }

        /// <summary>
        /// Units that require integer-only values (countable items).
        /// </summary>
        protected static bool IsIntegerOnlyUnit(Unit unit)
        {
            return unit is Unit.Piece or Unit.Pair or Unit.Sheet or Unit.Bottle or Unit.Can
            or Unit.Tube or Unit.Pack or Unit.Set or Unit.Bag or Unit.Roll
            or Unit.Box or Unit.Pallet;
        }
    }
}