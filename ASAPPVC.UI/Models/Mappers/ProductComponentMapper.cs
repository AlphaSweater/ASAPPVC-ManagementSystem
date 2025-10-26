using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Converts between ProductComponent bridge rows and their corresponding VMs.
    /// Designed to mirror ProductMapper patterns for muscle memory.
    /// </summary>
    public static class ProductComponentMapper
    {
        // ------------------------------------------------------------
        // Domain → VM
        // ------------------------------------------------------------

        /// <summary>
        /// Converts a ProductComponent domain entity to a read-only VM <br/>
        /// for display inside product detail views.
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Domain → VM (single)
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
        /// Converts a collection of ProductComponents to read-only VMs.
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Domain → VMs (collection)
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
        /// Create a ProductComponent from a create-line VM.<br/>
        /// Provide the parent productId and a Unit lookup dictionary.
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Create → Domain
        /// var components = await _components.GetByIdsAsync(vm.Components.Select(c => c.ComponentId), ct);
        /// var unitLookup = components.ToDictionary(c => c.Id, c => c.Unit);
        ///
        /// var line = ProductComponentMapper.FromCreateVm(productId, createLineVm, unitLookup);
        /// </code>
        /// </summary>
        /// <param name="productId">The parent Product Id.</param>
        /// <param name="componentUnitLookup">A lookup dictionary to resolve ComponentId → Unit.</param>
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
        /// Bulk helper: merge duplicates by ComponentId and create ProductComponent rows using a Unit lookup dictionary.
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Create lines → Domain (bulk)
        /// var components = await _components.GetByIdsAsync(vm.Components.Select(c => c.ComponentId), ct);
        /// var unitLookup = components.ToDictionary(c => c.Id, c => c.Unit);
        ///
        /// var lines = ProductComponentMapper.FromCreateVms(productId, createVm.Components, unitLookup);
        /// </code>
        /// </summary>
        public static List<ProductComponent> FromCreateVms(
            Guid productId,
            IEnumerable<CreateProductComponentVm> items,
            IDictionary<Guid, Unit> componentUnitLookup)
        {
            ArgumentNullException.ThrowIfNull(componentUnitLookup);

            if (items is null)
                return new();

            return items
                .GroupBy(i => i.ComponentId)
                .Select(g => new ProductComponent
                {
                    ProductId = productId,
                    ComponentId = g.Key,
                    Unit = ResolveUnit(componentUnitLookup, g.Key),
                    QuantityRequired = NormalizeQuantity(g.Sum(x => x.QuantityRequired))
                })
                .ToList();
        }

        // ------------------------------------------------------------
        // Edit VM → Domain (apply to existing)
        // ------------------------------------------------------------

        /// <summary>
        /// Apply an edit line to an existing ProductComponent.<br/>
        /// Optionally refresh Unit via the lookup (e.g., if Component changed).
        /// <br/>
        /// <br/><b>Examples:</b>
        /// <code>
        /// Edit line → Apply to existing row
        /// var components = await _components.GetByIdsAsync(vm.Components.Select(c => c.ComponentId), ct);
        /// var unitLookup = components.ToDictionary(c => c.Id, c => c.Unit);
        ///
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

        // ------------------------------------------------------------
        // Utilities
        // ------------------------------------------------------------

        private static decimal NormalizeQuantity(decimal q)
        {
            return q < 0 ? 0 : Math.Round(q, 4, MidpointRounding.AwayFromZero);
        }

        private static Unit ResolveUnit(IDictionary<Guid, Unit> lookup, Guid componentId)
        {
            return lookup.TryGetValue(componentId, out var u) ? u : Unit.Piece;
        }
    }
}