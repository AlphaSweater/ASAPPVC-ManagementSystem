namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Maps between Component domain entities and their ViewModels.<br/>
    /// ViewModels (<see cref="CreateComponentVm"/>, <see cref="EditComponentVm"/>, <see cref="ComponentListVm"/>, <see cref="ComponentDetailVm"/>).
    /// </summary>
    public static class ComponentMapper
    {
        // ------------------------------------------------------------
        // Domain → ViewModels
        // ------------------------------------------------------------

        /// <summary>
        /// Convert a Component to a lightweight list VM.
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Domain → List
        /// var list = components.Select(ComponentMapper.ToListVm).ToList();
        /// </code>
        /// </summary>
        public static ComponentListVm ToListVm(Component component)
        {
            ArgumentNullException.ThrowIfNull(component);

            return new ComponentListVm
            {
                Id = component.Id,
                ComponentCode = component.ComponentCode,
                Name = component.Name,
                Unit = component.Unit,
                CurrentAmount = component.CurrentAmount,
                UnitCost = component.UnitCost,
                StorageLocation = component.StorageLocation,
                HasImage = component.ImageData is { Length: > 0 } && !string.IsNullOrWhiteSpace(component.ImageType),
            };
        }

        /// <summary>
        /// Convert a Component to a detail VM.
        /// If <paramref name="usedInProductsCount"/> is null, tries to infer from the reverse nav.
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Domain → Detail (with inferred usage count)
        /// var detail = ComponentMapper.ToDetailVm(component);
        /// </code>
        /// </summary>
        public static ComponentDetailVm ToDetailVm(
            Component component,
            int? usedInProductsCount = null,
            bool includeImageDataUrl = true)
        {
            ArgumentNullException.ThrowIfNull(component);

            var count = usedInProductsCount
                        ?? component.ProductComponents?.Select(pc => pc.ProductId).Distinct().Count()
                        ?? 0;

            return new ComponentDetailVm
            {
                Id = component.Id,
                ComponentCode = component.ComponentCode,
                Name = component.Name,
                Unit = component.Unit,
                CurrentAmount = component.CurrentAmount,
                UnitCost = component.UnitCost,
                StorageLocation = component.StorageLocation,
                ImageBase64DataUrl = includeImageDataUrl ? AsDataUrlOrNull(component.ImageData, component.ImageType) : null,
                UsedInProductsCount = count
            };
        }

        // ------------------------------------------------------------
        // ViewModels → Domain
        // ------------------------------------------------------------

        /// <summary>
        /// Materialize a new Component from Create VM. Trims/normalizes input.
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Create → Domain
        /// var comp = ComponentMapper.FromCreateVm(createVm, () => CodeGen.NextComponent());
        /// await _components.AddAsync(comp, ct);
        /// </code>
        /// </summary>
        public static Component FromCreateVm(CreateComponentVm vm, Func<string>? codeGenerator = null)
        {
            ArgumentNullException.ThrowIfNull(vm);

            return new Component
            {
                ComponentCode = NormalizeCodeOrGenerate(vm.ComponentCode, codeGenerator),
                Name = Normalize(vm.Name),
                Unit = vm.Unit,
                CurrentAmount = vm.CurrentAmount < 0 ? 0 : vm.CurrentAmount,
                UnitCost = NormalizeMoney(vm.UnitCost),
                StorageLocation = Normalize(vm.StorageLocation),
                ImageData = vm.ImageData ?? Array.Empty<byte>(),
                ImageType = Normalize(vm.ImageType)
            };
        }

        /// <summary>
        /// Apply edits from Edit VM to an existing Component (in-place).
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Edit → Apply
        /// ComponentMapper.ApplyEditVm(existing, editVm);
        /// await _repo.SaveAsync(ct);
        /// </code>
        /// </summary>
        public static void ApplyEditVm(Component target, EditComponentVm vm)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(vm);
            if (target.Id != vm.Id)
                throw new InvalidOperationException("Mismatched component Id.");

            target.ComponentCode = Normalize(vm.ComponentCode);
            target.Name = Normalize(vm.Name);
            target.Unit = vm.Unit;
            target.CurrentAmount = vm.CurrentAmount < 0 ? 0 : vm.CurrentAmount;
            target.UnitCost = NormalizeMoney(vm.UnitCost);
            target.StorageLocation = Normalize(vm.StorageLocation);

            // Image semantics (null: keep, empty: clear, data: replace)
            if (vm.ImageData is null)
            {
                // keep existing
            }
            else if (vm.ImageData.Length == 0)
            {
                target.ImageData = Array.Empty<byte>();
                target.ImageType = string.Empty;
            }
            else
            {
                target.ImageData = vm.ImageData;
                target.ImageType = Normalize(vm.ImageType);
            }
        }

        // ------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------

        private static string Normalize(string? s)
        {
            return (s ?? string.Empty).Trim();
        }

        private static string NormalizeCodeOrGenerate(string? code, Func<string>? generator)
        {
            var trimmed = (code ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
                return trimmed;

            if (generator is not null)
            {
                var gen = (generator() ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(gen))
                    return gen;
            }

            // Fallback predictable placeholder (replace with your CodeGenerator if desired)
            return $"COMP-{Guid.NewGuid():N}".Substring(0, 13);
        }

        private static decimal NormalizeMoney(decimal amount)
        {
            return amount < 0 ? 0 : decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        }

        private static string? AsDataUrlOrNull(byte[]? data, string? mime)
        {
            if (data is not { Length: > 0 })
                return null;
            var safeMime = string.IsNullOrWhiteSpace(mime) ? "image/png" : mime.Trim();
            var b64 = Convert.ToBase64String(data);
            return $"data:{safeMime};base64,{b64}";
        }
    }
}