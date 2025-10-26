using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models.Enums
{
    /// <summary>
    /// Represents common units of measure used for inventory and warehouse components.
    /// <br/>
    /// <br/><b>Examples:</b>
    /// <code>
    /// var unit = Unit.Piece;
    /// unit.ToDisplay(3, true);  // "pcs"
    /// </code>
    /// </summary>
    public enum Unit
    {
        // Countable units (items counted as discrete pieces)

        [Display(Name = "pc", Description = "piece")] Piece,
        [Display(Name = "pair", Description = "pair")] Pair,
        [Display(Name = "sheet", Description = "sheet")] Sheet,
        [Display(Name = "bottle", Description = "bottle")] Bottle,
        [Display(Name = "can", Description = "can")] Can,
        [Display(Name = "tube", Description = "tube")] Tube,
        [Display(Name = "pack", Description = "pack")] Pack,
        [Display(Name = "set", Description = "set")] Set,
        [Display(Name = "bag", Description = "bag")] Bag,
        [Display(Name = "roll", Description = "roll")] Roll,
        [Display(Name = "box", Description = "box")] Box,
        [Display(Name = "pallet", Description = "pallet")] Pallet,

        // Length units

        [Display(Name = "m", Description = "meter")] Meter,
        [Display(Name = "cm", Description = "centimeter")] Centimeter,
        [Display(Name = "mm", Description = "millimeter")] Millimeter,

        // Mass/weight units

        [Display(Name = "t", Description = "tonne")] Tonne,
        [Display(Name = "kg", Description = "kilogram")] Kilogram,
        [Display(Name = "g", Description = "gram")] Gram,

        // Volume units

        [Display(Name = "L", Description = "liter")] Liter,
        [Display(Name = "mL", Description = "milliliter")] Milliliter
    }

    public static class UnitExtensions
    {
        private static readonly ConcurrentDictionary<Unit, DisplayAttribute?> _cache = new();

        private static DisplayAttribute? GetDisplay(Unit uom)
        {
            return _cache.GetOrAdd(uom, key =>
            {
                var member = typeof(Unit).GetMember(key.ToString()).FirstOrDefault();
                return member?.GetCustomAttributes(typeof(DisplayAttribute), false)
                             .FirstOrDefault() as DisplayAttribute;
            });
        }

        /// <summary>
        /// Returns a formatted display name for the unit of measure, automatically pluralized when quantity ≠ 1.
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Unit.Piece.ToDisplay(1);        // "piece"
        /// Unit.Piece.ToDisplay(3);        // "pieces"
        /// Unit.Piece.ToDisplay(1, true);  // "pc"
        /// Unit.Piece.ToDisplay(3, true);  // "pcs"
        /// Unit.Kilogram.ToDisplay(2);     // "kilograms"
        /// </code>
        /// </summary>
        public static string ToDisplay(this Unit uom, decimal quantity, bool shortForm = false)
        {
            var attr = GetDisplay(uom);
            if (attr == null)
                return uom.ToString();

            var text = shortForm
                ? attr.Name ?? uom.ToString()
                : attr.Description ?? uom.ToString();

            // pluralize if needed and not already plural
            if (quantity != 1 && !text.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                text += "s";

            return text;
        }
    }
}