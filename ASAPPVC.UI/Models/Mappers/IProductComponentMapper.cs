using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Converts between <see cref="ProductComponent"/> bridge entities and their ViewModels
    /// (<see cref="CreateProductComponentVm"/>, <see cref="EditProductComponentVm"/>, <see cref="ProductComponentVm"/>).
    /// Uses shared helpers for merging duplicate lines, normalizing quantities and resolving units.
    /// </summary>
    public interface IProductComponentMapper
    {
        /// <summary>
        /// Converts a single <see cref="ProductComponent"/> domain entity
        /// into a read-only view model for use in product detail or summary views.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var vm = _mapper.ToVm(productComponent);
        /// </code>
        /// </summary>
        ProductComponentVm ToVm(ProductComponent productComponent);

        /// <summary>
        /// Converts a collection of <see cref="ProductComponent"/> entities to read-only view models.
        /// Returns an empty list if <paramref name="items"/> is null.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var vmLines = _mapper.ToVms(product.ProductComponents);
        /// </code>
        /// </summary>
        List<ProductComponentVm> ToVms(IEnumerable<ProductComponent> items);

        /// <summary>
        /// Creates a new <see cref="ProductComponent"/> from a create-line view model.<br/>
        /// Requires the parent product ID and a lookup dictionary mapping ComponentId → Unit.
        /// Falls back to <see cref="Unit.Piece"/> if the unit cannot be resolved.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var unitLookup = components.ToDictionary(c => c.Id, c => c.Unit);
        /// var line = _mapper.FromCreateVm(productId, createLineVm, unitLookup);
        /// </code>
        /// </summary>
        /// <param name="productId">The parent Product identifier.</param>
        /// <param name="componentUnitLookup">A lookup dictionary to resolve units for each component.</param>
        ProductComponent FromCreateVm(Guid productId, CreateProductComponentVm vm, IDictionary<Guid, Unit> componentUnitLookup);

        /// <summary>
        /// Bulk helper: merges duplicates (by ComponentId) and creates ProductComponent rows.<br/>
        /// Automatically sums quantities and resolves units using the provided lookup.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var lines = _mapper.FromCreateVms(productId, vm.Components, unitLookup);
        /// </code>
        /// </summary>
        List<ProductComponent> FromCreateVms(Guid productId, IEnumerable<CreateProductComponentVm> items, IDictionary<Guid, Unit> componentUnitLookup);

        /// <summary>
        /// Applies an edit-line view model to an existing <see cref="ProductComponent"/> entity.<br/>
        /// Recalculates quantity and optionally updates the unit if the component changed
        /// or if no unit was previously assigned.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// _mapper.ApplyEditVm(existingLine, editLineVm, unitLookup);
        /// </code>
        /// </summary>
        void ApplyEditVm(ProductComponent target, EditProductComponentVm vm, IDictionary<Guid, Unit> componentUnitLookup);

        /// <summary>
        /// Bulk helper: applies a collection of edit-line view models to an existing product's components.<br/>
        /// Updates existing lines, creates missing ones, and merges duplicates by ComponentId.
        /// Automatically sums duplicate quantities and resolves missing units.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// product.ProductComponents = _mapper.ApplyEditVms(
        ///     product.ProductComponents,
        ///     editVm.Components,
        ///     unitLookup);
        /// </code>
        /// </summary>
        List<ProductComponent> ApplyEditVms(IEnumerable<ProductComponent> existingComponents, IEnumerable<EditProductComponentVm> editVms, IDictionary<Guid, Unit> componentUnitLookup);
    }
}
