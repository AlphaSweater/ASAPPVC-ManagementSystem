namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Implementation of <see cref="IComponentMapper"/>. Inherits shared helpers from <see cref="MapperBase"/>.
    /// </summary>
    public class ComponentMapper(ICodeGenerator? codeGenerator = null, IImageService? imageService = null) : MapperBase(codeGenerator, imageService), IComponentMapper
    {
        // ------------------------------------------------------------
        // Domain → ViewModels
        // ------------------------------------------------------------

        public ComponentListVm ToListVm(Component component)
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

        public ComponentDetailVm ToDetailVm(
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

        public Component FromCreateVm(CreateComponentVm vm)
        {
            ArgumentNullException.ThrowIfNull(vm);

            return new Component
            {
                ComponentCode = NormalizeCodeOrGenerate(vm.ComponentCode, "COMP"),
                Name = NormalizeString(vm.Name),
                Unit = vm.Unit,
                CurrentAmount = vm.CurrentAmount < 0 ? 0 : vm.CurrentAmount,
                UnitCost = NormalizeMoney(vm.UnitCost),
                StorageLocation = NormalizeString(vm.StorageLocation),
                ImageData = vm.ImageData ?? Array.Empty<byte>(),
                ImageType = NormalizeString(vm.ImageType)
            };
        }

        public void ApplyEditVm(Component target, EditComponentVm vm)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(vm);
            if (target.Id != vm.Id)
                throw new InvalidOperationException("Mismatched component Id.");

            target.ComponentCode = NormalizeString(vm.ComponentCode);
            target.Name = NormalizeString(vm.Name);
            target.Unit = vm.Unit;
            target.CurrentAmount = vm.CurrentAmount < 0 ? 0 : vm.CurrentAmount;
            target.UnitCost = NormalizeMoney(vm.UnitCost);
            target.StorageLocation = NormalizeString(vm.StorageLocation);

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
                target.ImageType = NormalizeString(vm.ImageType);
            }
        }
    }
}