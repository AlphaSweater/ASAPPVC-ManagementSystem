using ASAPPVC.UI.Models.Enums;

namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Implementation of <see cref="IProductComponentMapper"/>.
    /// </summary>
    public class ProductComponentMapper : IProductComponentMapper
    {
        // ------------------------------------------------------------
        // Domain → VM
        // ------------------------------------------------------------

        public ProductComponentVm ToBridgeVm(ProductComponent productComponent)
        {
            ArgumentNullException.ThrowIfNull(productComponent);

            return new ProductComponentVm
            {
                ProductId = productComponent.ProductId,
                ComponentId = productComponent.ComponentId,
                ComponentCode = productComponent.Component?.ComponentCode ?? string.Empty,
                ComponentName = productComponent.Component?.Name ?? string.Empty,
                Unit = productComponent.Unit,
                QuantityRequired = productComponent.QuantityRequired,
                UnitCost = productComponent.Component?.UnitCost ?? 0m
            };
        }

        public List<ProductComponentVm> ToBridgeVms(IEnumerable<ProductComponent> items)
        {
            if (items is null)
                return new();

            return items.Select(ToBridgeVm).ToList();
        }

        // ------------------------------------------------------------
        // Create from form VMs → Domain
        // ------------------------------------------------------------

        public ProductComponent FromCreateBridgeVm(Guid productId, ProductComponentFormVm vm, IDictionary<Guid, Unit> componentUnitLookup)
        {
            ArgumentNullException.ThrowIfNull(vm);
            ArgumentNullException.ThrowIfNull(componentUnitLookup);

            return new ProductComponent
            {
                ProductId = productId,
                ComponentId = vm.ComponentId,
                Unit = ResolveUnit(componentUnitLookup, vm.ComponentId),
                QuantityRequired = NormalizeQuantity(vm.Quantity)
            };
        }

        public List<ProductComponent> FromCreateBridgeVms(Guid productId, IEnumerable<ProductComponentFormVm> items, IDictionary<Guid, Unit> componentUnitLookup)
        {
            ArgumentNullException.ThrowIfNull(componentUnitLookup);

            return (items ?? Enumerable.Empty<ProductComponentFormVm>())
                .Where(i => !i.Remove) // drop lines marked for removal
                .GroupBy(i => i.ComponentId)
                .Select(g => new ProductComponentFormVm
                {
                    ComponentId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .Select(vm => FromCreateBridgeVm(productId, vm, componentUnitLookup))
                .ToList();
        }

        // ------------------------------------------------------------
        // Apply (update existing collection in-place)
        // ------------------------------------------------------------

        public void ApplyUpdateBridgeVm(ProductComponent target, ProductComponentFormVm vm, IDictionary<Guid, Unit> componentUnitLookup)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(vm);
            ArgumentNullException.ThrowIfNull(componentUnitLookup);

            target.ComponentId = vm.ComponentId;
            target.Unit = ResolveUnit(componentUnitLookup, vm.ComponentId);
            target.QuantityRequired = NormalizeQuantity(vm.Quantity);
        }

        public List<ProductComponent> ApplyUpdateBridgeVms(IEnumerable<ProductComponent> existingComponents, Guid productId, IEnumerable<ProductComponentFormVm> items, IDictionary<Guid, Unit> componentUnitLookup)
        {
            ArgumentNullException.ThrowIfNull(componentUnitLookup);

            var existingByComponent = (existingComponents ?? Enumerable.Empty<ProductComponent>())
                .ToDictionary(x => x.ComponentId, x => x);

            return (items ?? Enumerable.Empty<ProductComponentFormVm>())
                .Where(i => !i.Remove) // drop lines marked for removal
                .GroupBy(i => i.ComponentId)
                .Select(g => new ProductComponentFormVm
                {
                    ComponentId = g.Key,
                    Quantity = g.Sum(x => x.Quantity)
                })
                .Select(vm =>
                {
                    var line = existingByComponent.TryGetValue(vm.ComponentId, out var existing)
                        ? existing
                        : new ProductComponent { ProductId = productId };

                    ApplyUpdateBridgeVm(line, vm, componentUnitLookup);
                    return line;
                })
                .ToList();
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