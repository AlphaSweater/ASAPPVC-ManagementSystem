using ASAPPVC.UI.Services;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Implementation of <see cref="IComponentMapper"/>. Inherits shared helpers from <see cref="MapperBase"/>.
    /// </summary>
    public class ComponentMapper(ICodeGenerationService? codeGenerationService = null, IImageService? imageService = null) : MapperBase(codeGenerationService, imageService), IComponentMapper
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
                UsedInProductsCount = count,
            };
        }

        // ------------------------------------------------------------
        // ViewModels → Domain
        // ------------------------------------------------------------

        public async Task<Component> FromCreateVmAsync(CreateComponentVm vm, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(vm);

            var entity = new Component
            {
                ComponentCode = NormalizeCodeOrGenerate(vm.ComponentCode, "COMP"),
                Name = NormalizeString(vm.Name),
                Unit = vm.Unit,
                CurrentAmount = vm.CurrentAmount < 0 ? 0 : vm.CurrentAmount,
                UnitCost = NormalizeMoney(vm.UnitCost),
                StorageLocation = NormalizeString(vm.StorageLocation),
            };

            if (vm.Image is not null)
            {
                if (ImageService is null)
                    throw new InvalidOperationException("Image service is not available.");

                var processed = await ImageService.ProcessUploadAsync(vm.Image, ct);
                if (!processed.Ok)
                    throw new InvalidOperationException(processed.Error);

                entity.Image = processed.Value!.ToAppImage();
            }

            return entity;
        }

        public async Task ApplyEditVmAsync(Component target, EditComponentVm vm, CancellationToken ct = default)
        {
            // Validate and apply synchronous fields
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

            // Handle optional image processing asynchronously
            if (vm.Image is not null)
            {
                if (ImageService is null)
                    throw new InvalidOperationException("Image service is not available.");

                var processed = await ImageService.ProcessUploadAsync(vm.Image, ct);
                if (!processed.Ok)
                    throw new InvalidOperationException(processed.Error);

                target.Image = processed.Value!.ToAppImage(); // replace existing
            }
            // If vm.Image is null -> keep current image as-is.
        }
    }
}