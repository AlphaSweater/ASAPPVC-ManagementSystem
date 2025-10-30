namespace ASAPPVC.App.Models
{
    #region Interface

    /// <summary>
    /// Converts between <see cref="OrderProduct"/> bridge entities and their unified ViewModel.
    /// Simplified mapper that uses a single VM type for both display and editing scenarios.
    /// </summary>
    public interface IOrderProductMapper
    {
     /// <summary>
     /// Converts a single OrderProduct domain entity into a view model.
      /// Works for both display (detail view) and editing (form) scenarios.
        /// </summary>
  OrderProductVm ToVm(OrderProduct orderProduct);

        /// <summary>
  /// Converts a collection of OrderProduct entities to view models.
  /// Returns an empty list if items is null.
  /// </summary>
    List<OrderProductVm> ToVms(IEnumerable<OrderProduct> items);

        /// <summary>
        /// Creates a new OrderProduct from a view model.
  /// </summary>
        OrderProduct FromVm(Guid orderId, OrderProductVm vm);

    /// <summary>
        /// Bulk create: merges duplicates (by ProductId), sums quantities and returns OrderProduct rows.
     /// Used for both create and update operations.
   /// </summary>
  List<OrderProduct> FromVms(Guid orderId, IEnumerable<OrderProductVm> items);

 /// <summary>
   /// Applies a view model to an existing OrderProduct (mutates the target).
    /// </summary>
  void ApplyVm(OrderProduct target, OrderProductVm vm);

      /// <summary>
/// Bulk update: updates existing OrderProduct rows in-place (preserving instances when possible),
   /// creates missing rows and drops lines marked for removal.
        /// </summary>
   List<OrderProduct> ApplyVms(IEnumerable<OrderProduct> existingProducts, IEnumerable<OrderProductVm> items);
    }

    #endregion Interface

    /// <summary>
    /// Implementation of <see cref="IOrderProductMapper"/>.
    /// Simplified to work with a single unified OrderProductVm.
    /// </summary>
    public class OrderProductMapper : IOrderProductMapper
    {
    // ------------------------------------------------------------
   // Domain → VM (unified)
        // ------------------------------------------------------------

        public OrderProductVm ToVm(OrderProduct orderProduct)
        {
            ArgumentNullException.ThrowIfNull(orderProduct);

            return new OrderProductVm
      {
           OrderId = orderProduct.OrderId,
    ProductId = orderProduct.ProductId,
          ProductCode = orderProduct.Product?.ProductCode ?? string.Empty,
       ProductName = orderProduct.Product?.Name ?? string.Empty,
       UnitPrice = orderProduct.Product?.Price ?? 0m,
       Quantity = orderProduct.OrderedQuantity,
      Remove = false // Default for display/edit scenarios
   };
    }

        public List<OrderProductVm> ToVms(IEnumerable<OrderProduct> items)
   {
   if (items is null)
      return new();

     return items.Select(ToVm).ToList();
     }

     // ------------------------------------------------------------
   // VM → Domain (create)
        // ------------------------------------------------------------

        public OrderProduct FromVm(Guid orderId, OrderProductVm vm)
        {
     ArgumentNullException.ThrowIfNull(vm);

         return new OrderProduct
            {
    OrderId = orderId,
         ProductId = vm.ProductId,
     OrderedQuantity = NormalizeQuantity(vm.Quantity)
   };
  }

   public List<OrderProduct> FromVms(Guid orderId, IEnumerable<OrderProductVm> items)
        {
   return (items ?? Enumerable.Empty<OrderProductVm>())
                .Where(i => !i.Remove) // Drop lines marked for removal
       .GroupBy(i => i.ProductId)
        .Select(g => new OrderProductVm
          {
    ProductId = g.Key,
    Quantity = g.Sum(x => x.Quantity)
      })
      .Select(vm => FromVm(orderId, vm))
                .ToList();
        }

        // ------------------------------------------------------------
   // Apply updates (mutate existing domain entity)
        // ------------------------------------------------------------

        public void ApplyVm(OrderProduct target, OrderProductVm vm)
        {
 ArgumentNullException.ThrowIfNull(target);
      ArgumentNullException.ThrowIfNull(vm);

       target.ProductId = vm.ProductId;
         target.OrderedQuantity = NormalizeQuantity(vm.Quantity);
   }

        public List<OrderProduct> ApplyVms(IEnumerable<OrderProduct> existingProducts, IEnumerable<OrderProductVm> items)
 {
            var existingByProduct = (existingProducts ?? Enumerable.Empty<OrderProduct>())
     .ToDictionary(x => x.ProductId, x => x);

    return (items ?? Enumerable.Empty<OrderProductVm>())
    .Where(i => !i.Remove) // Drop lines marked for removal
          .GroupBy(vm => vm.ProductId)
    .Select(g => new OrderProductVm
          {
         ProductId = g.Key,
           Quantity = g.Sum(x => x.Quantity)
        })
    .Select(vm =>
                {
          var line = existingByProduct.TryGetValue(vm.ProductId, out var existing)
         ? existing
   : new OrderProduct();

    ApplyVm(line, vm);
              return line;
        })
        .ToList();
      }

        // ------------------------------------------------------------
        // Utilities
   // ------------------------------------------------------------

        private static int NormalizeQuantity(int q)
        {
     return q < 1 ? 1 : q;
      }
    }
}