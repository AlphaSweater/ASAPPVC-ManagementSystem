using ASAPPVC.App.Models.Enums;

namespace ASAPPVC.App.Models
{
    #region Interface

    /// <summary>
    /// Converts between <see cref="ProductComponent"/> bridge entities and their unified ViewModel.
    /// Simplified mapper that uses a single VM type for both display and editing scenarios.
    /// </summary>
    public interface IProductComponentMapper
    {
        /// <summary>
        /// Converts a single ProductComponent domain entity into a view model.
        /// Works for both display (detail view) and editing (form) scenarios.
        /// </summary>
        ProductComponentVm ToBridgeVm(ProductComponent productComponent);

        /// <summary>
        /// Converts a collection of ProductComponent entities to view models.
        /// Returns an empty list if <paramref name="items"/> is null.
        /// </summary>
        List<ProductComponentVm> ToBridgeVms(IEnumerable<ProductComponent> items);

        /// <summary>
        /// Creates a new ProductComponent from a view model.
        /// </summary>
        ProductComponent FromBridgeVm(Guid productId, ProductComponentVm vm);

        /// <summary>
        /// Bulk create: merges duplicates (by ComponentId), sums quantities and returns ProductComponent rows.
        /// Used for both create and update operations.
        /// </summary>
        List<ProductComponent> FromBridgeVms(Guid productId, IEnumerable<ProductComponentVm> items);

        /// <summary>
        /// Applies a view model to an existing ProductComponent (mutates the target).
        /// </summary>
        void ApplyUpdateToBridgeVm(ProductComponent target, ProductComponentVm vm);

        /// <summary>
        /// Bulk update: updates existing ProductComponent rows in-place (preserving instances when possible),
        /// creates missing rows and drops lines marked for removal.
        /// </summary>
        List<ProductComponent> ApplyUpdateToBridgeVms(IEnumerable<ProductComponent> existingComponents, Guid productId, IEnumerable<ProductComponentVm> items);
    }

    #endregion Interface

    /// <summary>
    /// Implementation of <see cref="IProductComponentMapper"/>.
    /// Simplified to work with a single unified ProductComponentVm.
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
                ComponentName = productComponent.Component?.ComponentName ?? string.Empty,
                UnitOfMeasure = productComponent.Component?.UnitOfMeasure ?? Unit.Piece,
                RequiredQuantity = productComponent.RequiredQuantity,
                UnitCost = productComponent.Component?.UnitCost ?? 0m,
                Remove = false // Default for display/edit scenarios
            };
        }

        public List<ProductComponentVm> ToBridgeVms(IEnumerable<ProductComponent> items)
        {
            if (items is null)
                return new();

            return items.Select(ToBridgeVm).ToList();
        }

        // ------------------------------------------------------------
        // VM → Domain (upsert)
        // ------------------------------------------------------------

        public ProductComponent FromBridgeVm(Guid productId, ProductComponentVm vm)
        {
            ArgumentNullException.ThrowIfNull(vm);

            return new ProductComponent
            {
                ProductId = productId,
                ComponentId = vm.ComponentId,
                RequiredQuantity = NormalizeQuantity(vm.RequiredQuantity),
                UnitCost = vm.UnitCost
            };
        }

        public List<ProductComponent> FromBridgeVms(Guid productId, IEnumerable<ProductComponentVm> items)
        {
            return (items ?? Enumerable.Empty<ProductComponentVm>())
                .GroupBy(i => i.ComponentId)
                .Select(g => new ProductComponentVm
                {
                    ComponentId = g.Key,
                    RequiredQuantity = g.Sum(x => x.RequiredQuantity)
                })
                .Select(vm => FromBridgeVm(productId, vm))
                .ToList();
        }

        // ------------------------------------------------------------
        // Apply updates
        // ------------------------------------------------------------

        public void ApplyUpdateToBridgeVm(ProductComponent target, ProductComponentVm vm)
        {
            ArgumentNullException.ThrowIfNull(target);
            ArgumentNullException.ThrowIfNull(vm);

            target.ComponentId = vm.ComponentId;
            target.UnitOfMeasure = vm.UnitOfMeasure;
            target.RequiredQuantity = NormalizeQuantity(vm.RequiredQuantity);
        }

        public List<ProductComponent> ApplyUpdateToBridgeVms(IEnumerable<ProductComponent> existingComponents, Guid productId, IEnumerable<ProductComponentVm> items)
        {
            var existingByComponent = (existingComponents ?? Enumerable.Empty<ProductComponent>())
                .ToDictionary(x => x.ComponentId, x => x);

            return (items ?? Enumerable.Empty<ProductComponentVm>())
                .Where(i => !i.Remove) // Drop lines marked for removal
                .GroupBy(i => i.ComponentId)
                .Select(g => new ProductComponentVm
                {
                    ComponentId = g.Key,
                    RequiredQuantity = g.Sum(x => x.RequiredQuantity)
                })
                .Select(vm =>
                {
                    var line = existingByComponent.TryGetValue(vm.ComponentId, out var existing)
                        ? existing
                        : new ProductComponent { ProductId = productId };

                    ApplyUpdateToBridgeVm(line, vm);
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
    }
}