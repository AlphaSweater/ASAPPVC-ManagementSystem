using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Converts between <see cref="ProductComponent"/> bridge entities and their ViewModels
    /// (<see cref="CreateProductComponentVm"/>, <see cref="EditProductComponentVm"/>, <see cref="ProductComponentVm"/>).
    /// Uses shared helpers for merging duplicate lines, normalizing quantities and resolving units.
    /// </summary>
    public static class ProductComponentMapper
    {
        // ------------------------------------------------------------
        // Domain → VM
        // ------------------------------------------------------------

        /// <summary>
        /// Converts a single <see cref="ProductComponent"/> domain entity
        /// into a read-only view model for use in product detail or summary views.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var vm = ProductComponentMapper.ToVm(productComponent);
        /// </code>
        /// </summary>
        public static ProductComponentVm ToVm(ProductComponent productComponent)
        {
            ArgumentNullException.ThrowIfNull(productComponent);

            return new ProductComponentVm
            {
                Id = productComponent.Id,
                ProductId = productComponent.ProductId,
                ComponentId = productComponent.ComponentId,
                ComponentCode = productComponent.Component?.ComponentCode ?? string.Empty,
                ComponentName = productComponent.Component?.Name ?? string.Empty,
                Unit = productComponent.Unit,
                QuantityRequired = productComponent.QuantityRequired,
                UnitCost = productComponent.Component?.UnitCost ?? 0m
            };
        }

        /// <summary>
        /// Converts a collection of <see cref="ProductComponent"/> entities to read-only view models.
        /// Returns an empty list if <paramref name="items"/> is null.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var vmLines = ProductComponentMapper.ToVms(product.ProductComponents);
        /// </code>
        /// </summary>
        public static List<ProductComponentVm> ToVms(IEnumerable<ProductComponent> items)
        {
            if (items is null)
                return new();

            return items.Select(ToVm).ToList();
        }

        // ------------------------------------------------------------
        // Create VM → Domain
        // ------------------------------------------------------------

        /// <summary>
        /// Creates a new <see cref="ProductComponent"/> from a create-line view model.<br/>
        /// Requires the parent product ID and a lookup dictionary mapping ComponentId → Unit.
        /// Falls back to <see cref="Unit.Piece"/> if the unit cannot be resolved.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var unitLookup = components.ToDictionary(c => c.Id, c => c.Unit);
        /// var line = ProductComponentMapper.FromCreateVm(productId, createLineVm, unitLookup);
        /// </code>
        /// </summary>
        /// <param name="productId">The parent Product identifier.</param>
        /// <param name="componentUnitLookup">A lookup dictionary to resolve units for each component.</param>
        public static ProductComponent FromCreateVm(
            Guid productId,
            CreateProductComponentVm vm,
            IDictionary<Guid, Unit> componentUnitLookup)
        {
            ArgumentNullException.ThrowIfNull(vm);
            ArgumentNullException.ThrowIfNull(componentUnitLookup);

            return new ProductComponent
            {
                ProductId = productId,
                ComponentId = vm.ComponentId,
                Unit = ResolveUnit(componentUnitLookup, vm.ComponentId),
                QuantityRequired = NormalizeQuantity(vm.QuantityRequired)
            };
        }

        /// <summary>
        /// Bulk helper: merges duplicates (by ComponentId) and creates ProductComponent rows.<br/>
        /// Automatically sums quantities and resolves units using the provided lookup.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// var lines = ProductComponentMapper.FromCreateVms(productId, vm.Components, unitLookup);
        /// </code>
        /// </summary>
        public static List<ProductComponent> FromCreateVms(
            Guid productId,
            IEnumerable<CreateProductComponentVm> items,
            IDictionary<Guid, Unit> componentUnitLookup)
        {
            ArgumentNullException.ThrowIfNull(componentUnitLookup);

            return (items ?? Enumerable.Empty<CreateProductComponentVm>())
                .GroupBy(i => i.ComponentId)
                .Select(g => new CreateProductComponentVm
                {
                    ComponentId = g.Key,
                    QuantityRequired = g.Sum(x => x.QuantityRequired)
                })
                .Select(vm => FromCreateVm(productId, vm, componentUnitLookup))
                .ToList();
        }

        // ------------------------------------------------------------
        // Edit VM → Domain (apply to existing)
        // ------------------------------------------------------------

        /// <summary>
        /// Applies an edit-line view model to an existing <see cref="ProductComponent"/> entity.<br/>
        /// Recalculates quantity and optionally updates the unit if the component changed
        /// or if no unit was previously assigned.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// ProductComponentMapper.ApplyEditVm(existingLine, editLineVm, unitLookup);
        /// </code>
        /// </summary>
        public static void ApplyEditVm(
            ProductComponent target,
            EditProductComponentVm vm,
            IDictionary<Guid, Unit> componentUnitLookup)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(vm);
            ArgumentNullException.ThrowIfNull(componentUnitLookup);

            var componentChanged = target.ComponentId != vm.ComponentId;

            target.ComponentId = vm.ComponentId;
            target.QuantityRequired = NormalizeQuantity(vm.QuantityRequired);

            if (componentChanged || target.Unit == default)
                target.Unit = ResolveUnit(componentUnitLookup, vm.ComponentId);
        }

        /// <summary>
        /// Bulk helper: applies a collection of edit-line view models to an existing product’s components.<br/>
        /// Updates existing lines, creates missing ones, and merges duplicates by ComponentId.
        /// Automatically sums duplicate quantities and resolves missing units.
        /// <br/><br/><b>Example:</b>
        /// <code>
        /// product.ProductComponents = ProductComponentMapper.ApplyEditVms(
        ///     product.ProductComponents,
        ///     editVm.Components,
        ///     unitLookup);
        /// </code>
        /// </summary>
        public static List<ProductComponent> ApplyEditVms(
            IEnumerable<ProductComponent> existingComponents,
            IEnumerable<EditProductComponentVm> editVms,
            IDictionary<Guid, Unit> componentUnitLookup)
        {
            ArgumentNullException.ThrowIfNull(componentUnitLookup);

            var existingByComponent = (existingComponents ?? Enumerable.Empty<ProductComponent>())
                .ToDictionary(x => x.ComponentId, x => x);

            return (editVms ?? Enumerable.Empty<EditProductComponentVm>())
                .GroupBy(vm => vm.ComponentId)
                .Select(g => new EditProductComponentVm
                {
                    ComponentId = g.Key,
                    QuantityRequired = g.Sum(x => x.QuantityRequired)
                })
                .Select(vm =>
                {
                    var line = existingByComponent.TryGetValue(vm.ComponentId, out var existing)
                        ? existing
                        : new ProductComponent();
                    ApplyEditVm(line, vm, componentUnitLookup);
                    return line;
                })
                .ToList();
        }

        // ------------------------------------------------------------
        // Utilities
        // ------------------------------------------------------------

        /// <summary>
        /// Ensures the quantity is non-negative and rounded to four decimals.
        /// </summary>
        private static decimal NormalizeQuantity(decimal q)
        {
            return q < 0 ? 0 : Math.Round(q, 4, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Resolves the component’s unit from the lookup or defaults to <see cref="Unit.Piece"/>.
        /// </summary>
        private static Unit ResolveUnit(IDictionary<Guid, Unit> lookup, Guid componentId)
        {
            return lookup.TryGetValue(componentId, out var u) ? u : Unit.Piece;
        }
    }
}