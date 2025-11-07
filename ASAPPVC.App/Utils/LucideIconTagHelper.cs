using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using System.Text.RegularExpressions;

namespace ASAPPVC.App.Utils
{
    /// <summary>
    /// <lucide name="search" size="18" class="me-2" stroke-width="1.75" aria-label="Search" />
    /// - Looks up /wwwroot/assets/icons/lucide/{name}.svg
    /// - Ensures icons inherit text color (stroke: currentColor) and fill: none
    /// - Defaults to 1em sizing; override via size (px) or style/class in CSS
    /// </summary>
    [HtmlTargetElement("lucideIcon", Attributes = "name")]
    public sealed class LucideIconTagHelper : TagHelper
    {
        private readonly IFileProvider _webRoot;
        private readonly IMemoryCache _cache;

        public LucideIconTagHelper(IWebHostEnvironment env, IMemoryCache cache)
        {
            _webRoot = env.WebRootFileProvider;
            _cache = cache;
        }

        /// <summary>Icon file name without .svg (e.g., "search").</summary>
        [HtmlAttributeName("name")]
        public string Name { get; set; } = string.Empty;

        /// <summary>Optional pixel size (e.g., 16, 18, 24). If omitted, uses 1em via CSS.</summary>
        [HtmlAttributeName("size")]
        public int? Size { get; set; }

        /// <summary>Optional stroke width (e.g., 1.5, 1.75, 2).</summary>
        [HtmlAttributeName("stroke-width")]
        public double? StrokeWidth { get; set; }

        /// <summary>Optional extra classes to add to the <svg> (in addition to "icon").</summary>
        [HtmlAttributeName("class")]
        public string? ExtraClass { get; set; }

        /// <summary>Optional title element for accessibility/tooltips.</summary>
        [HtmlAttributeName("title")]
        public string? Title { get; set; }

        /// <summary>Optional aria-label; if set, aria-hidden=false.</summary>
        [HtmlAttributeName("aria-label")]
        public string? AriaLabel { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null; // we will output raw <svg> markup

            if (string.IsNullOrWhiteSpace(Name))
            {
                output.Content.SetHtmlContent("<!-- lucide: missing name -->");
                return;
            }

            var cacheKey = $"lucide:{Name}";
            var svg = await _cache.GetOrCreateAsync(cacheKey, async entry =>
            {
                entry.SetSlidingExpiration(TimeSpan.FromMinutes(30));
                var file = _webRoot.GetFileInfo($"/assets/icons/lucide/{Name}.svg");
                if (!file.Exists)
                    return null;

                using var stream = file.CreateReadStream();
                using var reader = new StreamReader(stream);
                return await reader.ReadToEndAsync();
            });

            if (string.IsNullOrWhiteSpace(svg))
            {
                output.Content.SetHtmlContent($"<!-- lucideIcon: {Name}.svg not found -->");
                return;
            }

            // Normalize essential attributes so icons follow text color and theme.
            svg = EnsureAttr(svg, "stroke", "currentColor");
            svg = EnsureAttr(svg, "fill", "none");

            // Size: default to 1em (CSS); if size provided, set px explicitly.
            if (Size.HasValue)
            {
                svg = SetAttr(svg, "width", Size.Value.ToString());
                svg = SetAttr(svg, "height", Size.Value.ToString());
            }
            else
            {
                svg = SetAttr(svg, "width", "1em");
                svg = SetAttr(svg, "height", "1em");
            }

            if (StrokeWidth.HasValue)
            {
                svg = SetAttr(svg, "stroke-width", StrokeWidth.Value.ToString(System.Globalization.CultureInfo.InvariantCulture));
            }

            // Accessibility: if aria-label provided, it's a semantic img.
            if (!string.IsNullOrWhiteSpace(AriaLabel))
            {
                svg = SetAttr(svg, "role", "img");
                svg = SetAttr(svg, "aria-hidden", "false");
                svg = SetAttr(svg, "aria-label", AriaLabel!);
            }
            else
            {
                // decorative by default
                svg = SetAttr(svg, "aria-hidden", "true");
            }

            // Classes
            var classes = string.IsNullOrWhiteSpace(ExtraClass) ? "icon" : $"icon {ExtraClass}";
            svg = AddClass(svg, classes);

            // Optional <title> for hover/tooltips if provided
            if (!string.IsNullOrWhiteSpace(Title))
            {
                // If a <title> exists, replace it; else insert after opening tag
                svg = Regex.IsMatch(svg, @"<title>.*?</title>", RegexOptions.Singleline | RegexOptions.IgnoreCase)
                    ? Regex.Replace(svg, @"<title>.*?</title>", $"<title>{System.Net.WebUtility.HtmlEncode(Title)}</title>", RegexOptions.Singleline | RegexOptions.IgnoreCase)
                    : Regex.Replace(svg, @"<svg([^>]*)>", $"<svg$1><title>{System.Net.WebUtility.HtmlEncode(Title)}</title>");
            }

            output.Content.SetHtmlContent(svg);
        }

        // --- Helpers ----------------------------------------------------------

        private static string EnsureAttr(string svg, string attr, string value)
        {
            var has = Regex.IsMatch(svg, $@"\b{attr}\s*=\s*""[^""]*""", RegexOptions.IgnoreCase);
            return has ? svg : SetAttr(svg, attr, value);
        }

        private static string SetAttr(string svg, string attr, string value)
        {
            var pattern = $@"\b{attr}\s*=\s*""[^""]*""";
            if (Regex.IsMatch(svg, pattern, RegexOptions.IgnoreCase))
                return Regex.Replace(svg, pattern, $"{attr}=\"{value}\"", RegexOptions.IgnoreCase);
            return svg.Replace("<svg", $"<svg {attr}=\"{value}\"");
        }

        private static string AddClass(string svg, string classesToAdd)
        {
            var match = Regex.Match(svg, @"\bclass\s*=\s*""([^""]*)""", RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var existing = match.Groups[1].Value;
                var merged = string.IsNullOrWhiteSpace(existing) ? classesToAdd : $"{existing} {classesToAdd}";
                return Regex.Replace(svg, @"\bclass\s*=\s*""([^""]*)""", $"class=\"{merged}\"", RegexOptions.IgnoreCase);
            }
            return svg.Replace("<svg", $"<svg class=\"{classesToAdd}\"");
        }
    }
}
