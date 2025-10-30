using ASAPPVC.App.Models.Enums;
using ASAPPVC.App.Models.Mappers;
using ASAPPVC.App.Services;
using ASAPPVC.App.ViewModels.Reports;

namespace ASAPPVC.App.Models
{
    #region Interface

    /// <summary>
    /// Converts between <see cref="Order"/> domain entities and view models
    /// defined in `Order.vm.cs` (e.g. <see cref="OrderListVm"/>, <see cref="OrderDetailVm"/>).
    /// Delegates bridge line mapping to <see cref="IOrderProductMapper"/>.
    /// </summary>
    public interface IOrderMapper
    {
        /// <summary>
        /// Converts an Order to a lightweight list VM for dashboards/grids.
        /// </summary>
        OrderListVm ToListVm(Order order);

        /// <summary>
        /// Converts an Order to a full detail VM with optional product lines.
        /// </summary>
        OrderDetailVm ToDetailVm(Order order, bool includeProducts = true);

        /// <summary>
        /// Converts an Order domain entity into an OrderFormVm for use in edit forms.
        /// </summary>
        OrderFormVm ToFormVm(Order order);

        /// <summary>
        /// Builds a PickingSlipViewModel from a fully-loaded Order aggregate.
        /// Expects: Order -> Customer, OrderProducts -> Product -> ProductComponents -> Component.
        /// </summary>
        PickingSlipViewModel ToPickingSlipVm(Order order);

        /// <summary>
        /// Builds an Order domain entity from the unified order form VM. Generates an order code
        /// if not supplied using the optional generator.
        /// </summary>
        Order FromFormVm(OrderFormVm vm);

        /// <summary>
        /// Applies an upsert form VM to an existing Order domain entity.
        /// Updates scalar fields and delegates product-line merging to <see cref="IOrderProductMapper"/>.
        /// Returns the updated Order instance (same reference as <paramref name="existing"/>).
        /// </summary>
        Order ApplyUpdate(Order existing, OrderFormVm vm);
    }

    #endregion Interface

    /// <summary>
    /// Implementation of <see cref="IOrderMapper"/>. Inherits shared helpers from <see cref="MapperBase"/>.
    /// </summary>
    public class OrderMapper : MapperBase, IOrderMapper
    {
        private readonly IOrderProductMapper _orderProductMapper;

        public OrderMapper(IOrderProductMapper orderProductMapper, ICodeGenerationService? codeGenerationService = null, IImageService? imageService = null)
          : base(codeGenerationService, imageService)
        {
            _orderProductMapper = orderProductMapper ?? throw new ArgumentNullException(nameof(orderProductMapper));
        }

        // ------------------------------------------------------------
        // Domain → ViewModels
        // ------------------------------------------------------------

        public OrderListVm ToListVm(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            var lines = order.OrderProducts ?? Enumerable.Empty<OrderProduct>();

            var itemCount = lines.Sum(x => x.OrderedQuantity);
            var total = lines.Sum(x => (x.Product?.SellingPrice ?? 0m) * x.OrderedQuantity);

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

        public OrderDetailVm ToDetailVm(Order order, bool includeProducts = true)
        {
            ArgumentNullException.ThrowIfNull(order);

            var lines = order.OrderProducts ?? Enumerable.Empty<OrderProduct>();
            var products = includeProducts ? _orderProductMapper.ToVms(lines) : new List<OrderProductVm>();

            var itemCount = lines.Sum(x => x.OrderedQuantity);
            var subtotal = lines.Sum(x => (x.Product?.SellingPrice ?? 0m) * x.OrderedQuantity);
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

        public OrderFormVm ToFormVm(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            return new OrderFormVm
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                Products = _orderProductMapper.ToVms(order.OrderProducts ?? Enumerable.Empty<OrderProduct>())
            };
        }

        // ------------------------------------------------------------
        // Create ViewModel → Domain
        // ------------------------------------------------------------

        public Order FromFormVm(OrderFormVm vm)
        {
            ArgumentNullException.ThrowIfNull(vm);

            var order = new Order
            {
                OrderCode = NormalizeCodeOrGenerate(null, "ORD"),
                CustomerId = vm.CustomerId,
                OrderDate = vm.OrderDate ?? DateTime.UtcNow,
                OrderStatus = vm.OrderStatus
            };

            // Use the simplified mapper
            order.OrderProducts = _orderProductMapper.FromVms(order.Id, vm.Products ?? Enumerable.Empty<OrderProductVm>());

            return order;
        }
        public PickingSlipViewModel ToPickingSlipVm(Order order)
        {
            ArgumentNullException.ThrowIfNull(order);

            var vm = new PickingSlipViewModel
            {
                PickingSlipNumber = $"PSL-{order.OrderCode}",
                OrderNumber = order.OrderCode,
                CreatedDateTime = order.OrderDate,
                GeneratedDateTime = DateTime.UtcNow,
                Priority = ((Enum)order.OrderStatus).GetAttributePropertyOrDefault<UnitAttr, string>("ShortName", order.OrderStatus.ToString()),

                From = new FromInfo
                {
                    Company = "ASAPPVC (Pty) Ltd",
                    Warehouse = "Main Warehouse",
                    Contact = "info@asappvc.co.za"
                },
                For = new ForInfo
                {
                    Client = (order.Customer is null)
                        ? string.Empty
                        : $"{order.Customer.Name} {order.Customer.Surname}".Trim(),
                    ProjectSite = string.Empty,
                    Contact = (order.Customer is null)
                        ? string.Empty
                        : string.Join(" • ", new[]
                          {
                      order.Customer.Email?.Trim(),
                      order.Customer.PhoneNumber?.Trim()
                          }.Where(s => !string.IsNullOrWhiteSpace(s)))
                },
                Order = new OrderInfo
                {
                    OrderNumber = order.OrderCode,
                    Created = order.OrderDate,
                    PickBy = order.OrderDate.AddDays(1)
                },
                SpecialInstructions = string.Empty
            };

            var productGroups = new List<ProductGroup>();
            var locationTotals = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var lines = order.OrderProducts ?? Enumerable.Empty<OrderProduct>();
            foreach (var line in lines)
            {
                var product = line.Product;
                var group = new ProductGroup
                {
                    IsProduct = true,
                    ProductCode = product?.ProductCode ?? string.Empty,
                    ProductDescription = product?.ProductName ?? string.Empty,
                    RequiredSets = line.OrderedQuantity,
                    Items = new List<PickingItem>()
                };

                var pcs = product?.ProductComponents;

                if (pcs is not null && pcs.Count > 0)
                {
                    foreach (var pc in pcs)
                    {
                        var comp = pc.Component;
                        if (comp is null) continue;

                        var qty = line.OrderedQuantity;
                        var loc = (comp.LocationCode ?? string.Empty).Trim();

                        group.Items.Add(new PickingItem
                        {
                            PartNumber = comp.ComponentCode,
                            Description = comp.ComponentName,
                            Unit = ((Enum)comp.UnitOfMeasure).GetAttributePropertyOrDefault<UnitAttr, string>("ShortName", comp.UnitOfMeasure.ToString()),
                            Quantity = qty,
                            Location = string.IsNullOrWhiteSpace(loc) ? "-" : loc,
                            Notes = comp.LocationNote
                        });

                        if (!string.IsNullOrWhiteSpace(loc))
                            locationTotals[loc] = (locationTotals.TryGetValue(loc, out var c) ? c : 0) + qty;
                    }
                }
                else
                {
                    group.Items.Add(new PickingItem
                    {
                        PartNumber = product?.ProductCode ?? string.Empty,
                        Description = product?.ProductName ?? string.Empty,
                        Unit = "ea",
                        Quantity = line.OrderedQuantity,
                        Location = "-"
                    });
                }

                productGroups.Add(group);
            }

            vm.ProductGroups = productGroups;
            vm.LocationSummary = locationTotals
                .OrderBy(kv => kv.Key)
                .Select(kv => $"{kv.Key} ×{kv.Value}")
                .ToList();

            vm.TotalLines = productGroups.Sum(g => g.Items.Count);
            vm.TotalUnits = productGroups.Sum(g => g.Items.Sum(i => i.Quantity));

            return vm;
        }


        // ------------------------------------------------------------
        // Update existing domain entity from Edit VM
        // ------------------------------------------------------------

        public Order ApplyUpdate(Order existing, OrderFormVm vm)
        {
            ArgumentNullException.ThrowIfNull(existing);
            ArgumentNullException.ThrowIfNull(vm);

            if (existing.Id != vm.Id)
                throw new InvalidOperationException("Mismatched order Id.");

            // Scalars
            existing.OrderCode = NormalizeString(vm.OrderCode);
            existing.CustomerId = vm.CustomerId;
            existing.OrderDate = vm.OrderDate ?? existing.OrderDate;
            existing.OrderStatus = vm.OrderStatus;

            // Merge/update bridge lines using the simplified mapper
            var updatedLines = _orderProductMapper.ApplyVms(existing.OrderProducts ?? new List<OrderProduct>(), vm.Products ?? Enumerable.Empty<OrderProductVm>());

            // Ensure correct OrderId on all lines and replace collection
            foreach (var line in updatedLines)
            {
                line.OrderId = existing.Id;
            }

            existing.OrderProducts = updatedLines;

            return existing;
        }
    }
}