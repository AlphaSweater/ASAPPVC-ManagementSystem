using System.Collections.Concurrent;
using System.Reflection;

namespace ASAPPVC.App.Models.Enums
{
    /// <summary>
    /// Generic helpers to read custom attributes on enum members and access their properties via reflection.
    /// This allows each enum file to define its own attribute type / property layout while reusing a common backend.
    /// </summary>
    public static class EnumAttributeAccessor
    {
        // cache attribute instances per enum value + attribute type
        private static readonly ConcurrentDictionary<string, Attribute?> _attrCache = new();

        private static string BuildKey(Enum? value, Type attrType)
        {
            var typeName = value?.GetType().FullName ?? "<null>";
            var valueName = value?.ToString() ?? "<null>";
            var attrTypeName = attrType.FullName ?? attrType.Name;
            return string.Concat(typeName, "|", valueName, "|", attrTypeName);
        }

        /// <summary>
        /// Get the custom attribute instance of type TAttr for the enum value, or null if not present.
        /// The result is cached for performance.
        /// </summary>
        public static TAttr? GetAttribute<TAttr>(this Enum value) where TAttr : Attribute
        {
            if (value == null)
                return null;

            var key = BuildKey(value, typeof(TAttr));
            var obj = _attrCache.GetOrAdd(key, k =>
            {
                var fi = value.GetType().GetField(value.ToString());
                return fi?.GetCustomAttribute(typeof(TAttr));
            });

            return obj as TAttr;
        }

        /// <summary>
        /// Read a property (by name) from the attribute TAttr applied to the enum value.
        /// Returns defaultValue when attribute or property not found or when conversion fails.
        /// Property name matching is case-insensitive.
        /// </summary>
        public static T GetAttributePropertyOrDefault<TAttr, T>(this Enum value, string propertyName, T defaultValue = default!) where TAttr : Attribute
        {
            if (value == null || string.IsNullOrWhiteSpace(propertyName))
                return defaultValue;

            var attr = value.GetAttribute<TAttr>();
            if (attr == null)
                return defaultValue;

            var pi = attr.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (pi == null)
                return defaultValue;

            try
            {
                var raw = pi.GetValue(attr);
                if (raw == null)
                    return defaultValue;

                if (raw is T t)
                    return t;

                // try to convert
                return (T)Convert.ChangeType(raw, typeof(T));
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Convenience: extract attribute-derived value using a typed extractor function.
        /// Returns defaultValue when the attribute is missing.
        /// </summary>
        public static T GetAttributeValueOrDefault<TAttr, T>(this Enum value, Func<TAttr, T> extractor, T defaultValue = default!) where TAttr : Attribute
        {
            if (value == null)
                return defaultValue;
            var attr = value.GetAttribute<TAttr>();
            if (attr == null)
                return defaultValue;
            try
            {
                return extractor(attr);
            }
            catch
            {
                return defaultValue;
            }
        }
    }
}