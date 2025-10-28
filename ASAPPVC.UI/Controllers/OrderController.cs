using ASAPPVC.UI.Models;
using ASAPPVC.UI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    public class OrderController : Controller
    {
        //─────────── Dependencies ───────────\\
        private readonly IOrderService _orders;

        private readonly ICustomerService _customers;
        private readonly IProductService _products;

        //constructor
        public OrderController(IOrderService orders, ICustomerService customers, IProductService products)
        {
            _orders = orders;
            _customers = customers;
            _products = products;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays the add order view with customer and product selections
        [HttpGet]
        public async Task<IActionResult> AddOrder(CancellationToken ct)
        {
            var customersResult = await _customers.ListAsync(ct);
            var productsResult = await _products.ListAsync(ct);

            if (!productsResult.Ok)
            {
                TempData["ErrorMessage"] = productsResult.Error;
                return RedirectToAction(nameof(ViewOrders));
            }

            ViewData["Customers"] = customersResult;
            ViewData["Products"] = productsResult.Value;
            return View();
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //handles the submission of the add order form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrder(OrderFormVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var customersResult = await _customers.ListAsync(ct);
                var productsResult = await _products.ListAsync(ct);
                ViewData["Customers"] = customersResult;
                ViewData["Products"] = productsResult.Ok ? productsResult.Value : new List<ProductListVm>();
                return View(vm);
            }

            var result = await _orders.CreateAsync(vm, ct);
            if (!result.Ok || result.Value is null)
            {
                var customersResult = await _customers.ListAsync(ct);
                var productsResult = await _products.ListAsync(ct);
                ViewData["Customers"] = customersResult;
                ViewData["Products"] = productsResult.Ok ? productsResult.Value : new List<ProductListVm>();
                ModelState.AddModelError(string.Empty, result.Error ?? "Unable to create order.");
                return View(vm);
            }

            TempData["AlertMessage"] = $"Order '{result.Value.OrderCode}' created successfully.";
            return RedirectToAction(nameof(ViewOrders));
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays the list of orders
        [HttpGet]
        public async Task<IActionResult> ViewOrders(CancellationToken ct)
        {
            var result = await _orders.ListAsync(ct);
            if (!result.Ok)
            {
                TempData["ErrorMessage"] = result.Error;
                return View(new List<OrderListVm>());
            }

            return View(result.Value);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays a specific order by its ID
        [HttpGet]
        public async Task<IActionResult> ViewOrder(Guid id, CancellationToken ct)
        {
            var result = await _orders.GetDetailAsync(id, ct);
            if (!result.Ok || result.Value is null)
            {
                TempData["ErrorMessage"] = result.Error ?? "Order not found.";
                return RedirectToAction(nameof(ViewOrders));
            }

            return View(result.Value);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays the edit order view
        [HttpGet]
        public async Task<IActionResult> EditOrder(Guid id, CancellationToken ct)
        {
            var orderResult = await _orders.GetDomainAsync(id, ct);
            if (!orderResult.Ok || orderResult.Value is null)
            {
                TempData["ErrorMessage"] = orderResult.Error ?? "Order not found.";
                return RedirectToAction(nameof(ViewOrders));
            }

            var customersResult = await _customers.ListAsync(ct);
            var productsResult = await _products.ListAsync(ct);

            ViewData["Customers"] = customersResult;
            ViewData["Products"] = productsResult.Ok ? productsResult.Value : new List<ProductListVm>();

            var order = orderResult.Value;
            var vm = new OrderFormVm
            {
                Id = order.Id,
                OrderCode = order.OrderCode,
                CustomerId = order.CustomerId,
                OrderDate = order.OrderDate,
                OrderStatus = order.OrderStatus,
                Products = order.OrderProducts.Select(op => new OrderProductFormVm
                {
                    ProductId = op.ProductId,
                    Quantity = op.Quantity
                }).ToList()
            };

            return View(vm);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //handles the submission of the edit order form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrder(OrderFormVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                var customersResult = await _customers.ListAsync(ct);
                var productsResult = await _products.ListAsync(ct);
                ViewData["Customers"] = customersResult;
                ViewData["Products"] = productsResult.Ok ? productsResult.Value : new List<ProductListVm>();
                return View(vm);
            }

            var result = await _orders.UpdateAsync(vm, ct);
            if (!result.Ok || result.Value is null)
            {
                var customersResult = await _customers.ListAsync(ct);
                var productsResult = await _products.ListAsync(ct);
                ViewData["Customers"] = customersResult;
                ViewData["Products"] = productsResult.Ok ? productsResult.Value : new List<ProductListVm>();
                ModelState.AddModelError(string.Empty, result.Error ?? "Unable to update order.");
                return View(vm);
            }

            TempData["AlertMessage"] = $"Order '{result.Value.OrderCode}' updated successfully.";
            return RedirectToAction(nameof(ViewOrder), new { id = result.Value.Id });
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //deletes an order by ID
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrder(Guid id, CancellationToken ct)
        {
            var result = await _orders.DeleteAsync(id, ct);
            if (!result.Ok)
            {
                TempData["ErrorMessage"] = result.Error ?? "Unable to delete order.";
            }
            else
            {
                TempData["AlertMessage"] = "Order deleted successfully.";
            }

            return RedirectToAction(nameof(ViewOrders));
        }
    }
}