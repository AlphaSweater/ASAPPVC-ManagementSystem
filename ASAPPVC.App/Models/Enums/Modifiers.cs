namespace ASAPPVC.App.Models.Enums
{
    // Local attribute used to annotate modifier enums with a code + display name.
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public sealed class ModifierAttr(string code, string name) : Attribute
    {
        public string Code { get; } = code;
        public string Name { get; } = name;
    }

    // Categories (examples)
    public enum Category
    {
        None = 0,

        [ModifierAttr("WIN", "Window")]
        Window = 1,

        [ModifierAttr("DOR", "Door")]
        Door = 2,

        [ModifierAttr("SKI", "Skylight")]
        Skylight = 3
    }

    // Colours (examples)
    public enum Colour
    {
        None = 0,

        [ModifierAttr("RED", "Red")]
        Red = 1,

        [ModifierAttr("BLU", "Blue")]
        Blue = 2,

        [ModifierAttr("GRN", "Green")]
        Green = 3,

        [ModifierAttr("BLK", "Black")]
        Black = 4,

        [ModifierAttr("WHT", "White")]
        White = 5
    }

    // Materials (examples)
    public enum Material
    {
        None = 0,

        [ModifierAttr("ALU", "Aluminium")]
        Aluminium = 1,

        [ModifierAttr("WOD", "Wood")]
        Wood = 2,

        [ModifierAttr("PVC", "PVC")]
        PVC = 3,

        [ModifierAttr("STL", "Steel")]
        Steel = 4
    }

    // Convenience extension methods that use EnumAttributeAccessor to read attribute data.
    public static class ModifierExtensions
    {
        public static string GetCode(this Enum value)
        {
            return value.GetAttributePropertyOrDefault<ModifierAttr, string>("Code", value.ToString());
        }

        public static string GetName(this Enum value)
        {
            return value.GetAttributePropertyOrDefault<ModifierAttr, string>("Name", value.ToString());
        }

        // Return the enum type's simple name (e.g. "Category").
        public static string GetEnumTypeName(this Enum value)
        {
            if (value == null)
                return string.Empty;
            return value.GetType().Name;
        }

        // Return a labeled code string like "Category: WIN". Optionally include namespace.
        public static string ToLabeledCode(this Enum value)
        {
            if (value == null)
                return string.Empty;

            var typeName = value.GetType().Name;
            var code = value.GetAttributePropertyOrDefault<ModifierAttr, string>("Code", value.ToString());
            return $"{typeName}: {code}";
        }

        /// <summary>
        /// Build a CSV line in a fixed order: Category, Material, Colour.
        /// Each element uses the enum type name (e.g. "Category") followed by the code, e.g. "Category: WIN,Material: ALU,Colour: RED".
        /// Missing types are omitted.
        /// </summary>
        public static string ToModifiersCsvOrdered(this IEnumerable<Enum> values, char separator = ',')
        {
            if (values == null)
                return string.Empty;

            var list = values.Where(v => v != null).ToList();
            var parts = new List<string>(3);

            // helper to add if present using the full enum type name
            void TryAdd(Type t)
            {
                var found = list.FirstOrDefault(v => v.GetType() == t);
                if (found != null)
                {
                    var code = found.GetAttributePropertyOrDefault<ModifierAttr, string>("Code", found.ToString());
                    if (!string.IsNullOrEmpty(code))
                        parts.Add($"{t.Name}: {code}");
                }
            }

            TryAdd(typeof(Category));
            TryAdd(typeof(Material));
            TryAdd(typeof(Colour));

            return string.Join(separator, parts);
        }

        /// <summary>
        /// Params overload for ordered CSV.
        /// </summary>
        public static string ToModifiersCsvOrdered(char separator = ',', params Enum[] values)
        {
            return ToModifiersCsvOrdered((IEnumerable<Enum>?)values ?? Array.Empty<Enum>(), separator);
        }
    }
}