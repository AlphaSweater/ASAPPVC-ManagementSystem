using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Converts between <see cref="ProductComponent"/> bridge entities and their ViewModels.
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
        ProductComponentVm ToBridgeVm(ProductComponent productComponent);

        /// <summary>
        /// Converts a collection of <see cref="ProductComponent"/> entities to read-only view models.
        /// Returns an empty list if <paramref name="items"/> is null.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var vmLines = _mapper.ToVms(product.ProductComponents);
        /// </code>
        /// </summary>
        List<ProductComponentVm> ToBridgeVms(IEnumerable<ProductComponent> items);

        /// <summary>
        /// Converts a single form VM into a domain ProductComponent.
        /// </summary>
        ProductComponent FromCreateBridgeVm(Guid productId, ProductComponentFormVm vm, IDictionary<Guid, Unit> componentUnitLookup);

        /// <summary>
        /// Bulk helper: merges duplicates (by ComponentId), sums quantities and returns domain ProductComponent rows.
        /// Used for both create and edit upsert operations.
        /// </summary>
        List<ProductComponent> FromCreateBridgeVms(Guid productId, IEnumerable<ProductComponentFormVm> items, IDictionary<Guid, Unit> componentUnitLookup);

        /// <summary>
        /// Applies a single form VM to an existing ProductComponent (mutates the target).
        /// </summary>
        void ApplyUpdateBridgeVm(ProductComponent target, ProductComponentFormVm vm, IDictionary<Guid, Unit> componentUnitLookup);

        /// <summary>
        /// Bulk apply: updates existing ProductComponent rows in-place (preserving instances when possible),
        /// creates missing rows and drops lines marked for removal. Returns the resulting collection to assign to the product.
        /// </summary>
        List<ProductComponent> ApplyUpdateBridgeVms(IEnumerable<ProductComponent> existingComponents, Guid productId, IEnumerable<ProductComponentFormVm> items, IDictionary<Guid, Unit> componentUnitLookup);
    }
}