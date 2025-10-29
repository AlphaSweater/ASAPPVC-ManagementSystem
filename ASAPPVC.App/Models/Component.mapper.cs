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

        Task<Component> ApplyUpdateVmAsync(Component existing, ComponentFormVm vm, CancellationToken ct = default);

        // Added: map domain entity to form VM for edit/create prefilling
        ComponentFormVm ToFormVm(Component component);
    }

    #endregion Interface

    /// <summary>
    /// Maps between Component domain entities and their ViewModels.
    /// ViewModels (<see cref="CreateComponentVm"/>, <see cref="EditComponentVm"/>, <see cref="ComponentListVm"/>, <see cref="ComponentDetailVm"/>).
    /// </summary>
    public class ComponentMapper(
        ICodeGenerationService? codeGenerationService = null,
        IImageService? imageService = null)
        : MapperBase(codeGenerationService, imageService), IComponentMapper
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
        /// var list = components.Select(c => _mapper.ToListVm(c)).ToList();
        /// </code>
        /// </summary>
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

        /// <summary>
        /// Convert a Component to a detail VM.<br/>
        /// If <paramref name="usedInProductsCount"/> is null, tries to infer from the reverse nav.
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Domain → Detail (with inferred usage count)
        /// var detail = _mapper.ToDetailVm(component);
        /// </code>
        /// </summary>
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

        /// <summary>
        /// Convert a Component to a ComponentFormVm for prefilling the create/edit form.
        /// </summary>
        public ComponentFormVm ToFormVm(Component component)
        {
            ArgumentNullException.ThrowIfNull(component);

            return new ComponentFormVm
            {
                Id = component.Id,
                ComponentCode = component.ComponentCode,
                Name = component.Name,
                Unit = component.Unit,
                CurrentAmount = component.CurrentAmount,
                UnitCost = component.UnitCost,
                StorageLocation = component.StorageLocation,
                // Provide existing image as data URL (if present) to show preview in edit forms
                ExistingImageUrl = AsDataUrlOrNull(component.Image?.Data, component.Image?.ContentType)
            };
        }

        // ------------------------------------------------------------
        // ViewModel (Upsert) → Domain
        // ------------------------------------------------------------

        /// <summary>
        /// Creates a new Component from a ComponentFormVm.
        /// This may process an uploaded image.
        /// </summary>
        public async Task<Component> FromCreateVmAsync(ComponentFormVm vm, CancellationToken ct = default)
        {
            ArgumentNullException.ThrowIfNull(vm);

            var entity = new Component
            {
                ComponentCode = NormalizeCodeOrGenerate(vm.ComponentCode, "COMP"),
                Name = NormalizeString(vm.Name),
                Unit = vm.Unit,
                CurrentAmount = vm.CurrentAmount < 0m ? 0m : vm.CurrentAmount,
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
        /// This may process an uploaded image. Returns the modified existing entity.
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
            existing.CurrentAmount = vm.CurrentAmount < 0m ? 0m : vm.CurrentAmount;
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