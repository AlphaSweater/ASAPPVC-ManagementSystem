namespace ASAPPVC.UI.Models.Enums
{
    // Example: local attribute definition for this enum file. Consumers can define their own attribute
    // with any property names; backend helpers will read them dynamically by name.
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class UnitAttr(string code, string name, string shortName) : Attribute
    {
        public string Code { get; } = code;
        public string Name { get; } = name;
        public string ShortName { get; } = shortName;
    }

    /// <summary>
    /// Represents common units of measure used for inventory and warehouse components.
    /// </summary>
    public enum Unit
    {
        [UnitAttr("PC", "piece", "pc")] Piece,
        [UnitAttr("PR", "pair", "pair")] Pair,
        [UnitAttr("SH", "sheet", "sheet")] Sheet,
        [UnitAttr("BT", "bottle", "bottle")] Bottle,
        [UnitAttr("CN", "can", "can")] Can,
        [UnitAttr("TB", "tube", "tube")] Tube,
        [UnitAttr("PK", "pack", "pack")] Pack,
        [UnitAttr("ST", "set", "set")] Set,
        [UnitAttr("BG", "bag", "bag")] Bag,
        [UnitAttr("RL", "roll", "roll")] Roll,
        [UnitAttr("BX", "box", "box")] Box,
        [UnitAttr("PL", "pallet", "pallet")] Pallet,

        [UnitAttr("M", "meter", "m")] Meter,
        [UnitAttr("CM", "centimeter", "cm")] Centimeter,
        [UnitAttr("MM", "millimeter", "mm")] Millimeter,

        [UnitAttr("T", "tonne", "t")] Tonne,
        [UnitAttr("KG", "kilogram", "kg")] Kilogram,
        [UnitAttr("G", "gram", "g")] Gram,

        [UnitAttr("L", "liter", "L")] Liter,
        [UnitAttr("ML", "milliliter", "mL")] Milliliter
    }

    public static class UnitExtensions
    {
        public static string ToDisplay(this Unit uom, decimal quantity, bool shortForm = false)
        {
            var enumObj = (Enum)uom;
            if (shortForm)
            {
                // read 'Short' property from UnitAttr if present
                var shortVal = enumObj.GetAttributePropertyOrDefault<UnitAttr, string>("ShortName", enumObj.GetAttributePropertyOrDefault<UnitAttr, string>("Code", uom.ToString()));
                var txt = shortVal;
                if (quantity != 1 && !txt.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                    txt += "s";
                return txt;
            }
            else
            {
                var name = enumObj.GetAttributePropertyOrDefault<UnitAttr, string>("Name", uom.ToString());
                var txt = name;
                if (quantity != 1 && !txt.EndsWith("s", StringComparison.OrdinalIgnoreCase))
                    txt += "s";
                return txt;
            }
        }
    }
}