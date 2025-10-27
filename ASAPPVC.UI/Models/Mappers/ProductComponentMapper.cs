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

        public ProductComponentVm ToVm(ProductComponent productComponent)
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

        public List<ProductComponentVm> ToVms(IEnumerable<ProductComponent> items)
        {
            if (items is null)
                return new();

            return items.Select(ToVm).ToList();
        }

        // ------------------------------------------------------------
        // Create VM → Domain
        // ------------------------------------------------------------

        public ProductComponent FromCreateVm(Guid productId, CreateProductComponentVm vm, IDictionary<Guid, Unit> componentUnitLookup)
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

        public List<ProductComponent> FromCreateVms(Guid productId, IEnumerable<CreateProductComponentVm> items, IDictionary<Guid, Unit> componentUnitLookup)
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

        public void ApplyEditVm(ProductComponent target, EditProductComponentVm vm, IDictionary<Guid, Unit> componentUnitLookup)
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

        public List<ProductComponent> ApplyEditVms(IEnumerable<ProductComponent> existingComponents, IEnumerable<EditProductComponentVm> editVms, IDictionary<Guid, Unit> componentUnitLookup)
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