using ASAPPVC.UI.Models.ViewModels.Order;
using ASAPPVC.UI.Services.Interfaces;
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
            ViewData["Customers"] = await _customers.ListAsync(ct);
            ViewData["Products"] = await _products.ListAsync(ct);
            return View();
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //handles the submission of the add order form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddOrder(CreateOrderViewModel vm, CancellationToken ct)
        {
            var (ok, error, order) = await _orders.CreateAsync(vm, ct);
            if (!ok || order is null)
            {
                ViewData["Customers"] = await _customers.ListAsync(ct);
                ViewData["Products"] = await _products.ListAsync(ct);
                ModelState.AddModelError(string.Empty, error ?? "Unable to create order.");
                return View(vm);
            }

            TempData["AlertMessage"] = $"Order #{order.OrderID} created.";
            return RedirectToAction(nameof(ViewOrders));
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays the list of orders
        [HttpGet]
        public async Task<IActionResult> ViewOrders(CancellationToken ct)
        {
            var list = await _orders.ListAsync(ct);
            return View(list);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays a specific order by its ID
        [HttpGet]
        public async Task<IActionResult> ViewOrder(int id, CancellationToken ct)
        {
            var order = await _orders.GetAsync(id, ct);
            if (order is null) return NotFound();
            return View(order);
        }
    }
}
