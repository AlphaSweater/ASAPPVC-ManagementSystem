using ASAPPVC.UI.Services;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Maps between Component domain entities and ViewModels (list/detail/form).
    /// </summary>
    public class ComponentMapper(
        ICodeGenerationService? codeGenerationService = null,
        IImageService? imageService = null)
        : MapperBase(codeGenerationService, imageService), IComponentMapper
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
                HasImage = component.Image?.Data is { Length: > 0 },
                ThumbUrl = component.Image?.Data is { Length: > 0 }
                    ? $"/components/{component.Id}/image/thumb"
                    : null,
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
                HasImage = component.Image?.Data is { Length: > 0 },
                ImageUrl = component.Image?.Data is { Length: > 0 }
                    ? $"/components/{component.Id}/image"
                    : null,
                ImageEtag = component.Image?.Sha256
            };
        }

        // ------------------------------------------------------------
        // ViewModel (Upsert) → Domain
        // ------------------------------------------------------------

        /// <summary>
        /// Creates a new Component from a ComponentFormVm.
        /// </summary>
        public async Task<Component> FromCreateVmAsync(ComponentFormVm vm, CancellationToken ct = default)
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
                EnsureImageService();
                var processed = await ImageService!.ProcessUploadAsync(vm.Image, ct);
                if (!processed.Ok)
                    throw new ValidationException(processed.Error);
                entity.Image = processed.Value!.ToAppImage();
            }

            return entity;
        }

        /// <summary>
        /// Applies an update to an existing Component using a ComponentFormVm.
        /// </summary>
        public async Task<Component> ApplyUpdateVmAsync(Component existing, ComponentFormVm vm, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(existing);
            ArgumentNullException.ThrowIfNull(vm);
            if (vm.Id is null || existing.Id != vm.Id.Value)
                throw new InvalidOperationException("Mismatched component Id.");

            existing.ComponentCode = NormalizeString(vm.ComponentCode ?? existing.ComponentCode);
            existing.Name = NormalizeString(vm.Name);
            existing.Unit = vm.Unit;
            existing.CurrentAmount = vm.CurrentAmount < 0 ? 0 : vm.CurrentAmount;
            existing.UnitCost = NormalizeMoney(vm.UnitCost);
            existing.StorageLocation = NormalizeString(vm.StorageLocation);

            if (vm.Image is not null)
            {
                EnsureImageService();
                var processed = await ImageService!.ProcessUploadAsync(vm.Image, ct);
                if (!processed.Ok)
                    throw new ValidationException(processed.Error);
                existing.Image = processed.Value!.ToAppImage(); // replace existing
            }
            // If vm.Image is null -> keep current image as-is.

            return existing;
        }

        // ------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------

        private void EnsureImageService()
        {
            if (ImageService is null)
                throw new InvalidOperationException("Image service is not available.");
        }
    }
}