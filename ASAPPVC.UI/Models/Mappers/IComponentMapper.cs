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
        /// Materialize a new Component from Create VM. Trims/normalizes input.
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Create → Domain
        /// var comp = _mapper.FromCreateVm(createVm);
        /// await _components.AddAsync(comp, ct);
        /// </code>
        /// </summary>
        Component FromCreateVm(CreateComponentVm vm);

        /// <summary>
        /// Apply edits from Edit VM to an existing Component (in-place).
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Edit → Apply
        /// _mapper.ApplyEditVm(existing, editVm);
        /// await _repo.SaveAsync(ct);
        /// </code>
        /// </summary>
        void ApplyEditVm(Component target, EditComponentVm vm);
    }
}
