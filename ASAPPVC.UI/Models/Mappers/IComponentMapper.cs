namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Maps between Component domain entities and their ViewModels.
    /// ViewModels (<see cref="CreateComponentVm"/>, <see cref="EditComponentVm"/>, <see cref="ComponentListVm"/>, <see cref="ComponentDetailVm"/>).
    /// </summary>
    public interface IComponentMapper
    {
        /// <summary>
        /// Convert a Component to a lightweight list VM.
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Domain → List
        /// var list = components.Select(c => _mapper.ToListVm(c)).ToList();
        /// </code>
        /// </summary>
        ComponentListVm ToListVm(Component component);

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
        ComponentDetailVm ToDetailVm(Component component, int? usedInProductsCount = null, bool includeImageDataUrl = true);

        /// <summary>
        /// Creates a new Component from a ComponentFormVm. This may process an uploaded image.
        /// </summary>
        Task<Component> FromCreateVmAsync(ComponentFormVm vm, CancellationToken ct = default);

        /// <summary>
        /// Applies an update to an existing Component using a ComponentFormVm. This may process an uploaded image.
        /// Returns the modified existing entity.
        /// </summary>
        Task<Component> ApplyUpdateVmAsync(Component existing, ComponentFormVm vm, CancellationToken ct = default);
    }
}