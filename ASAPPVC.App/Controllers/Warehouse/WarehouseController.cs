using ASAPPVC.App.Models.Enums;
using ASAPPVC.App.Services;
using ASAPPVC.App.ViewModels.Warehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.App.Controllers.Warehouse
{
    [Authorize]
    public class WarehouseController : Controller
    {
        private readonly IProductService _productService;
        private readonly IComponentService _componentService;
        private readonly IOrderService _orderService;

        // Base path for warehouse views - append view file names or subfolders to this
        public const string ViewRoot = "~/Views/Warehouse/";

        // View path constants
        private const string WarehouseDashboardViewName = ViewRoot + "WarehouseDashboard.cshtml";
        private const string StockTakeViewName = ViewRoot + "StockTake.cshtml";

        public WarehouseController(
            IProductService productService,
            IComponentService componentService,
            IOrderService orderService)
        {
            _productService = productService;
            _componentService = componentService;
            _orderService = orderService;
        }

        // Show warehouse dashboard - GET
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var vm = new WarehouseDashVm();

            // Get product count
            var productsResult = await _productService.ListAsync(ct);
            vm.ProductCount = productsResult.Ok && productsResult.Value != null
                ? productsResult.Value.Count
                : 0;

            // Get component count
            var componentsResult = await _componentService.ListAsync(ct);
            vm.ComponentCount = componentsResult.Ok && componentsResult.Value != null
                ? componentsResult.Value.Count
                : 0;

            // Get open orders count (Pending + Processing)
            var ordersResult = await _orderService.ListAsync(ct);
            vm.OpenOrdersCount = ordersResult.Ok && ordersResult.Value != null
                ? ordersResult.Value.Count(o => o.OrderStatus == OrderStatus.Pending || o.OrderStatus == OrderStatus.Picked)
                : 0;

            // Get low stock count (components with status Low, Critical, or OutOfStock)
            if (componentsResult.Ok && componentsResult.Value != null)
            {
                vm.LowStockCount = 0;
                foreach (var component in componentsResult.Value)
                {
                    var status = ReorderStatusPolicy.Evaluate(component.QuantityOnHand, component.ReorderLevel);
                    if (status == ReorderStatus.Low || status == ReorderStatus.Critical || status == ReorderStatus.OutOfStock || status == ReorderStatus.Approaching)
                    {
                        vm.LowStockCount++;
                    }
                }
            }

            return View(WarehouseDashboardViewName, vm);
        }

        // Show stocktake page - GET
        [HttpGet]
        public IActionResult StockTake()
        {
            // TODO: call service to start stocktake process
            TempData["Message"] = "Stocktake initiated.";

            // TODO: populate stocktake view model
            return View(StockTakeViewName);
        }

        // Finish stocktake - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult StockTake(IFormCollection form)
        {
            // TODO: handle posted stocktake data
            return RedirectToAction(nameof(Index));
        }

        // Reconcile inventory - GET
        [HttpGet]
        public IActionResult ReconcileInventory()
        {
            // TODO: call reconciliation service
            TempData["Message"] = "Inventory reconciliation started.";
            // TODO: show reconcile UI / populate model

            // Reuse dashboard view for now or create a dedicated view
            return View(WarehouseDashboardViewName);
        }

        // Reconcile inventory - POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ReconcileInventory(IFormCollection form)
        {
            // TODO: perform reconciliation work
            return RedirectToAction(nameof(Index));
        }
    }
}