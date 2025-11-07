using ASAPPVC.App.Models;
using ASAPPVC.App.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.App.Controllers
{
    [Authorize]
    [AutoValidateAntiforgeryToken]
    [Route("[controller]")]
    public class OrdersController(IOrderService orders, ICustomerService customers, IProductService products) : Controller
    {
        private readonly IOrderService _orders = orders;
        private readonly ICustomerService _customers = customers;
        private readonly IProductService _products = products;

        // Views
        public const string ViewRoot = "Views/Orders/";

        private const string ManageOrdersViewName = ViewRoot + "ManageOrders.cshtml";
        private const string DetailsViewName = ViewRoot + "ViewOrder.cshtml";
        private const string UpsertViewName = ViewRoot + "UpsertOrder.cshtml";

        // GET /Orders?term=...
        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] string? term, CancellationToken ct)
        {
            var query = term?.Trim();

            // Prefer SearchAsync if available for consistency with Products/Components
            var result = string.IsNullOrWhiteSpace(query)
                ? await _orders.ListAsync(ct)
                : await _orders.SearchAsync(query, ct);

            if (!result.Ok || result.Value is null)
            {
                TempData["ErrorMessage"] = result.Error ?? "Failed to load orders.";
                return View(ManageOrdersViewName, new List<OrderListVm>());
            }

            ViewData["SearchQuery"] = query;
            return View(ManageOrdersViewName, result.Value);
        }

        // GET /Orders/View/{id:guid}
        [HttpGet("View/{id:guid}")]
        public Task<IActionResult> DetailsById([FromRoute] Guid id, CancellationToken ct)
        {
            return GetAndShowDetails(id, ct);
        }

        // GET /Orders/AddNew
        [HttpGet("AddNew")]
        public async Task<IActionResult> AddNew(CancellationToken ct)
        {
            var vm = new OrderFormVm();
            await PopulateLookupsAsync(vm, ct);
            return View(UpsertViewName, vm);
        }

        // GET /Orders/Edit/{id:guid}
        [HttpGet("Edit/{id:guid}")]
        public Task<IActionResult> EditById([FromRoute] Guid id, CancellationToken ct)
        {
            return GetAndShowForm(id, ct);
        }

        // POST /Orders/Upsert
        [HttpPost("Upsert")]
        public async Task<IActionResult> Upsert([FromForm] OrderFormVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                await PopulateLookupsAsync(vm, ct);
                return View(UpsertViewName, vm);
            }

            var op = vm.IsEdit
                ? await _orders.UpdateAsync(vm, ct)
                : await _orders.CreateAsync(vm, ct);

            if (!op.Ok || op.Value is null)
            {
                await PopulateLookupsAsync(vm, ct);
                ModelState.AddModelError(string.Empty, op.Error ?? (vm.IsEdit ? "Unable to update order." : "Unable to create order."));
                return View(UpsertViewName, vm);
            }

            var saved = op.Value;
            TempData["AlertMessage"] = vm.IsEdit
                ? $"Order '{saved.OrderCode}' updated successfully."
                : $"Order '{saved.OrderCode}' created successfully.";

            return RedirectToAction(nameof(DetailsById), new { id = saved.Id });
        }

        // POST /Orders/Delete/{id:guid}
        [HttpPost("Delete/{id:guid}")]
        public async Task<IActionResult> Delete([FromRoute] Guid id, CancellationToken ct)
        {
            var result = await _orders.DeleteAsync(id, ct);
            if (!result.Ok)
                TempData["ErrorMessage"] = result.Error ?? "Unable to delete order.";
            else
                TempData["AlertMessage"] = "Order deleted successfully.";

            return RedirectToAction(nameof(Index));
        }

        // POST /Orders/MarkPicked/{id}
        [HttpPost("MarkPicked/{id:guid}")]
        public async Task<IActionResult> MarkPicked([FromRoute] Guid id, CancellationToken ct)
        {
            var op = await _orders.MarkPickedAsync(id, ct);
            TempData[op.Ok ? "AlertMessage" : "ErrorMessage"] =
                op.Ok ? "Order marked as picked and stock reduced." : op.Error;
            return RedirectToAction(nameof(DetailsById), new { id });
        }

        // POST /Orders/MarkCompleted/{id}
        [HttpPost("MarkCompleted/{id:guid}")]
        public async Task<IActionResult> MarkCompleted([FromRoute] Guid id, CancellationToken ct)
        {
            var op = await _orders.MarkCompletedAsync(id, ct);
            TempData[op.Ok ? "AlertMessage" : "ErrorMessage"] =
                op.Ok ? "Order marked as completed." : op.Error;
            return RedirectToAction(nameof(DetailsById), new { id });
        }

        // POST /Orders/Cancel/{id}
        [HttpPost("Cancel/{id:guid}")]
        public async Task<IActionResult> Cancel([FromRoute] Guid id, CancellationToken ct)
        {
            var op = await _orders.CancelAsync(id, ct);
            TempData[op.Ok ? "AlertMessage" : "ErrorMessage"] =
                op.Ok ? "Order cancelled." : op.Error;
            return RedirectToAction(nameof(DetailsById), new { id });
        }

        // ===== Helpers =====

        private async Task<IActionResult> GetAndShowDetails(Guid id, CancellationToken ct)
        {
            var res = await _orders.GetDetailAsync(id, ct);
            if (!res.Ok || res.Value is null)
                return GoIndexWithError(res.Error ?? "Order not found.");
            return View(DetailsViewName, res.Value);
        }

        private async Task<IActionResult> GetAndShowForm(Guid id, CancellationToken ct)
        {
            var orderRes = await _orders.GetDomainAsync(id, ct);
            if (!orderRes.Ok || orderRes.Value is null)
                return GoIndexWithError(orderRes.Error ?? "Order not found.");

            var o = orderRes.Value;
            var vm = new OrderFormVm
            {
                Id = o.Id,
                OrderCode = o.OrderCode,
                CustomerId = o.CustomerId,
                OrderDate = o.OrderDate,
                OrderStatus = o.OrderStatus,
                Products = o.OrderProducts.Select(op => new OrderProductVm
                {
                    ProductId = op.ProductId,
                    Quantity = op.OrderedQuantity
                }).ToList()
            };

            await PopulateLookupsAsync(vm, ct);
            return View(UpsertViewName, vm);
        }

        /// <summary>
        /// Populate form VM lookup lists via GetAvailable{Model}Async services.
        /// Assumes OrderFormVm exposes AvailableCustomers / AvailableProducts.
        /// </summary>
        private async Task PopulateLookupsAsync(OrderFormVm vm, CancellationToken ct)
        {
            var customers = await _customers.GetAvailableCustomersAsync(ct);
            var products = await _products.GetAvailableProductsAsync(ct);

            vm.AvailableCustomers = customers.Ok && customers.Value is not null
                ? customers.Value
                : new List<Customer>();

            vm.AvailableProducts = products.Ok && products.Value is not null
                ? products.Value
                : new List<ProductListVm>();
        }

        private RedirectToActionResult GoIndexWithError(string message)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}