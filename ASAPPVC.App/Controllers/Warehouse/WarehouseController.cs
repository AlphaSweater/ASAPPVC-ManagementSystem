using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.App.Controllers.Warehouse
{
    [Authorize]
    public class WarehouseController : Controller
    {
        // Base path for warehouse views - append view file names or subfolders to this
        public const string ViewRoot = "~/Views/Warehouse/";

        // View path constants

        private const string WarehouseDashboardViewName = ViewRoot + "WarehouseDashboard.cshtml";
        private const string StockTakeViewName = ViewRoot + "StockTake.cshtml";

        // Show warehouse dashboard - GET
        [HttpGet]
        public IActionResult Index()
        {
            // TODO: populate dashboard view model
            return View(WarehouseDashboardViewName);
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