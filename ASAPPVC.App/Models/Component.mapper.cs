using ASAPPVC.App.Models.General;
using ASAPPVC.App.Models.Mappers;
using ASAPPVC.App.Services;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.App.Models
{
    #region Interface

    public interface IComponentMapper
    {
        ComponentListVm ToListVm(Component component);

        ComponentDetailVm ToDetailVm(Component component, int? usedInProductsCount = null, bool includeImageDataUrl = true);

        Task<Component> FromCreateVmAsync(ComponentFormVm vm, CancellationToken ct = default);

        Task<Component> ApplyUpdateAsync(Component existing, ComponentFormVm vm, CancellationToken ct = default);

        // Added: map domain entity to form VM for edit/create prefilling
        ComponentFormVm ToFormVm(Component component);
    }

    #endregion Interface

    /// <summary>
    /// Maps between Component domain entities and their ViewModels.
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
                ComponentName = component.ComponentName,

                UnitOfMeasure = component.UnitOfMeasure,
                QuantityOnHand = component.QuantityOnHand,
                UnitCost = component.UnitCost,

                LocationCode = component.LocationCode ?? string.Empty,
                LocationNote = component.LocationNote,

                ReorderLevel = component.ReorderLevel,
                IsActive = component.IsActive,

                HasImage = component.Image?.Data is { Length: > 0 },
                ThumbUrl = component.Image?.Thumb is { Length: > 0 }
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
                ComponentName = component.ComponentName,

                MaterialType = component.MaterialType,
                ColourOption = component.ColourOption,

                UnitOfMeasure = component.UnitOfMeasure,
                QuantityOnHand = component.QuantityOnHand,
                UnitCost = component.UnitCost,

                LocationCode = component.LocationCode ?? string.Empty,
                LocationNote = component.LocationNote,

                ReorderLevel = component.ReorderLevel,

                HasImage = component.Image?.Data is { Length: > 0 },
                ImageUrl = component.Image?.Data is { Length: > 0 }
                ? $"/components/{component.Id}/image"
                : null,
                ImageEtag = component.Image?.Sha256,

                UsedInProductsCount = count,

                IsActive = component.IsActive,
                CreatedAt = component.CreatedAt,
                UpdatedAt = component.UpdatedAt
            };
        }

        public ComponentFormVm ToFormVm(Component component)
        {
            ArgumentNullException.ThrowIfNull(component);

            return new ComponentFormVm
            {
                Id = component.Id,
                ComponentCode = component.ComponentCode,
                ComponentName = component.ComponentName,

                MaterialType = component.MaterialType,
                ColourOption = component.ColourOption,

                UnitOfMeasure = component.UnitOfMeasure,
                QuantityOnHand = component.QuantityOnHand,
                UnitCost = component.UnitCost,

                LocationCode = component.LocationCode ?? string.Empty,
                LocationNote = component.LocationNote,

                ReorderLevel = component.ReorderLevel,

                // Provide existing image as data URL (if present) to show preview in edit forms
                ExistingImageUrl = AsDataUrlOrNull(component.Image?.Data, component.Image?.ContentType),

                IsActive = component.IsActive,
            };
        }

        // ------------------------------------------------------------
        // ViewModel (Upsert) → Domain
        // ------------------------------------------------------------

        public async Task<Component> FromCreateVmAsync(ComponentFormVm vm, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(vm);

            var entity = new Component
            {
                ComponentCode = NormalizeCodeOrGenerate(code: vm.ComponentCode, prefix: "COMP"),
                ComponentName = NormalizeString(vm.ComponentName),

                MaterialType = vm.MaterialType,
                ColourOption = vm.ColourOption,

                UnitOfMeasure = vm.UnitOfMeasure,
                QuantityOnHand = vm.QuantityOnHand,
                UnitCost = NormalizeMoney(vm.UnitCost),

                LocationCode = NormalizeString(vm.LocationCode),
                LocationNote = NormalizeString(vm.LocationNote),

                ReorderLevel = vm.ReorderLevel,
                IsActive = vm.IsActive
            };

            if (vm.Image is not null)
            {
                EnsureImageService();
                var processed = await _imageService!.ProcessUploadAsync(vm.Image, ct);
                if (!processed.Ok)
                    throw new ValidationException(processed.Error);
                entity.Image = processed.Value!.ToAppImage();
            }

            return entity;
        }

        public async Task<Component> ApplyUpdateAsync(Component existing, ComponentFormVm vm, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(existing);
            ArgumentNullException.ThrowIfNull(vm);
            if (vm.Id is null || existing.Id != vm.Id.Value)
                throw new InvalidOperationException("Mismatched component Id.");

            existing.ComponentCode = NormalizeString(vm.ComponentCode ?? existing.ComponentCode);
            existing.ComponentName = NormalizeString(vm.ComponentName);

            existing.MaterialType = vm.MaterialType;
            existing.ColourOption = vm.ColourOption;

            existing.UnitOfMeasure = vm.UnitOfMeasure;
            existing.QuantityOnHand = vm.QuantityOnHand < 0m ? 0m : vm.QuantityOnHand;
            existing.UnitCost = NormalizeMoney(vm.UnitCost);

            existing.LocationCode = NormalizeString(vm.LocationCode);
            existing.LocationNote = NormalizeString(vm.LocationNote);

            existing.ReorderLevel = vm.ReorderLevel;
            existing.IsActive = vm.IsActive;

            if (vm.Image is not null)
            {
                EnsureImageService();
                var processed = await _imageService!.ProcessUploadAsync(vm.Image, ct);
                if (!processed.Ok)
                    throw new ValidationException(processed.Error);
                existing.Image = processed.Value!.ToAppImage(); // replace existing
            }

            return existing;
        }

        // ------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------

        private void EnsureImageService()
        {
            if (_imageService is null)
                throw new InvalidOperationException("Image service is not available.");
        }
    }
}