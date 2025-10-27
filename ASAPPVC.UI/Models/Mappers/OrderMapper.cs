namespace ASAPPVC.UI.Models.Mappers
{
    /// <summary>
    /// Converts between <see cref="Order"/> domain entities and view models
    /// defined in `Order.vm.cs` (e.g. <see cref="OrderListVm"/>, <see cref="OrderDetailVm"/>).
    /// Delegates bridge line mapping to <see cref="OrderProductMapper"/>.
    /// </summary>
    public static class OrderMapper
    {
        // ------------------------------------------------------------
        // Domain → ViewModels
        // ------------------------------------------------------------

        public static OrderListVm ToListVm(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            var lines = order.OrderProducts ?? Enumerable.Empty<OrderProduct>();

            var itemCount = lines.Sum(x => x.Quantity);
            var total = lines.Sum(x => (x.Product?.Price ?? 0m) * x.Quantity);

            return new OrderListVm
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                CustomerId = order.CustomerId,
                CustomerName = (order.Customer is null) ? string.Empty : $"{order.Customer.Name} {order.Customer.Surname}".Trim(),
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                ItemCount = itemCount,
                TotalAmount = total
            };
        }

        public static OrderDetailVm ToDetailVm(Order order, bool includeProducts = true)
        {
            ArgumentNullException.ThrowIfNull(order);

            var lines = order.OrderProducts ?? Enumerable.Empty<OrderProduct>();
            var products = includeProducts ? OrderProductMapper.ToVms(lines) : new List<OrderProductVm>();

            var itemCount = lines.Sum(x => x.Quantity);
            var subtotal = lines.Sum(x => (x.Product?.Price ?? 0m) * x.Quantity);
            var tax = 0m; // keep zero for now — compute later if needed
            var grand = subtotal + tax;

            return new OrderDetailVm
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                CustomerId = order.CustomerId,
                CustomerName = (order.Customer is null) ? string.Empty : $"{order.Customer.Name} {order.Customer.Surname}".Trim(),
                CustomerEmail = order.Customer?.Email,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                Products = products,
                ItemCount = itemCount,
                Subtotal = subtotal,
                TaxAmount = tax,
                GrandTotal = grand
            };
        }

        // ------------------------------------------------------------
        // Create ViewModel → Domain
        // ------------------------------------------------------------

        /// <summary>
        /// Builds an Order domain entity from a CreateOrderVm. Generates an order code
        /// if not supplied using the optional generator.
        /// </summary>
        public static Order FromCreateVm(CreateOrderVm vm, Func<string>? codeGenerator = null)
        {
            ArgumentNullException.ThrowIfNull(vm);

            var order = new Order
            {
                OrderCode = NormalizeCodeOrGenerate(null, codeGenerator),
                CustomerId = vm.CustomerId,
                OrderDate = vm.OrderDate ?? DateTime.UtcNow,
                OrderStatus = vm.OrderStatus
            };

            order.OrderProducts = OrderProductMapper.FromCreateVms(order.Id, vm.Products ?? Enumerable.Empty<CreateOrderProductVm>());

            return order;
        }

        // ------------------------------------------------------------
        // Utilities
        // ------------------------------------------------------------

        private static string NormalizeCodeOrGenerate(string? code, Func<string>? generator)
        {
            var trimmed = (code ?? string.Empty).Trim();
            if (!string.IsNullOrWhiteSpace(trimmed))
                return trimmed;

            if (generator is not null)
            {
                var gen = (generator() ?? string.Empty).Trim();
                if (!string.IsNullOrWhiteSpace(gen))
                    return gen;
            }

            // Fallback stable-ish prefix
            return $"ORD-{Guid.NewGuid():N}".Substring(0, 13);
        }

        // ------------------------------------------------------------
        // Utility helpers (kept consistent with other mappers)
        // ------------------------------------------------------------

        private static string NormalizeString(string? s)
        {
            return (s ?? string.Empty).Trim();
        }

        private static decimal NormalizeMoney(decimal amount)
        {
            return amount < 0 ? 0 : decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
        }

        private static string? AsDataUrlOrNull(byte[]? data, string? mime)
        {
            if (data is not { Length: > 0 })
                return null;
            var safeMime = string.IsNullOrWhiteSpace(mime) ? "image/png" : mime.Trim();
            var b64 = Convert.ToBase64String(data);
            return $"data:{safeMime};base64,{b64}";
        }
    }
}