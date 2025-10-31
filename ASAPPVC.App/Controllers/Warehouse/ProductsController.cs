using ASAPPVC.App.Models;
using ASAPPVC.App.Services;
using ASAPPVC.App.ViewModels.Warehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.App.Controllers.Warehouse
{
    [Area("Warehouse")]
    [Authorize]
    [AutoValidateAntiforgeryToken]
    [Route("Warehouse/[controller]")]
    public class ProductsController(IProductService products, IComponentService components) : Controller
    {
        private readonly IProductService _products = products;
        private readonly IComponentService _components = components;

        // Views

        public const string ViewRoot = WarehouseController.ViewRoot + "Products/";
        private const string ManageProductsViewName = ViewRoot + "ManageProducts.cshtml";
        private const string DetailsViewName = ViewRoot + "ViewProduct.cshtml";
        private const string UpsertViewName = ViewRoot + "UpsertProduct.cshtml";

        // GET /Warehouse/Products
        [HttpGet("")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _products.ListAsync(ct);
            if (!result.Ok || result.Value is null)
                return GoIndexWithError(result.Error ?? "Failed to load products.");

            return View(ManageProductsViewName, ManageProductsVm.Create(result.Value));
        }

        // GET /Warehouse/Products/Search?term=...
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string? term, CancellationToken ct)
        {
            var result = await _products.SearchAsync(term, ct);
            if (!result.Ok || result.Value is null)
                return GoIndexWithError(result.Error ?? "Failed to search products.");

            return View(ManageProductsViewName, ManageProductsVm.Create(result.Value, searchQuery: term));
        }

        // GET /Warehouse/Products/View/{id:guid}
        [HttpGet("View/{id:guid}")]
        public async Task<IActionResult> DetailsById([FromRoute] Guid id, CancellationToken ct)
        {
            var byId = await _products.GetDetailAsync(id: id, code: null, ct: ct);
            if (!byId.Ok || byId.Value is null)
                return GoIndexWithError(byId.Error ?? "Product not found.");

            return View(DetailsViewName, byId.Value);
        }

        // GET /Warehouse/Products/View/{code}
        [HttpGet("View/{code}")]
        public async Task<IActionResult> DetailsByCode([FromRoute] string code, CancellationToken ct)
        {
            code = (code ?? string.Empty).Trim();
            if (code.Length == 0)
                return GoIndexWithError("Product not found.");

            var byCode = await _products.GetDetailAsync(id: null, code: code, ct: ct);
            if (!byCode.Ok || byCode.Value is null)
                return GoIndexWithError(byCode.Error ?? "Product not found.");

            return View(DetailsViewName, byCode.Value);
        }

        // --- Create ---

        // GET /Warehouse/Products/AddNew
        [HttpGet("AddNew")]
        public async Task<IActionResult> AddNew(CancellationToken ct)
        {
            var vm = new ProductFormVm();

            var compsResult = await _products.GetAvailableComponentsAsync(ct);
            vm.AvailableProductComponents = compsResult.Ok && compsResult.Value is not null
                ? compsResult.Value
                : new();

            return View(UpsertViewName, vm);
        }

        // --- Edit ---

        // GET /Warehouse/Products/Edit/{id:guid}
        [HttpGet("Edit/{id:guid}")]
        public async Task<IActionResult> EditById([FromRoute] Guid id, CancellationToken ct)
        {
            var byId = await _products.GetFormAsync(id: id, ct: ct);
            if (!byId.Ok || byId.Value is null)
                return GoIndexWithError(byId.Error ?? "Product not found.");

            var vm = byId.Value;
            var compsResult = await _products.GetAvailableComponentsAsync(ct);
            vm.AvailableProductComponents = compsResult.Ok && compsResult.Value is not null
                ? compsResult.Value
                : new();

            return View(UpsertViewName, vm);
        }

        // GET /Warehouse/Products/Edit/{code}
        [HttpGet("Edit/{code}")]
        public async Task<IActionResult> EditByCode([FromRoute] string code, CancellationToken ct)
        {
            code = (code ?? string.Empty).Trim();
            if (code.Length == 0)
                return GoIndexWithError("Product not found.");

            var byCode = await _products.GetFormAsync(code: code, ct: ct);
            if (!byCode.Ok || byCode.Value is null)
                return GoIndexWithError(byCode.Error ?? "Product not found.");

            var vm = byCode.Value;
            var compsResult = await _products.GetAvailableComponentsAsync(ct);
            vm.AvailableProductComponents = compsResult.Ok && compsResult.Value is not null
                ? compsResult.Value
                : new();

            return View(UpsertViewName, vm);
        }

        // --- Save (create or update) ---

        // POST /Warehouse/Products/Upsert
        [HttpPost("Upsert")]
        public async Task<IActionResult> Upsert([FromForm] ProductFormVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(UpsertViewName, vm);

            var op = vm.IsEdit
                ? await _products.UpdateAsync(vm, ct)
                : await _products.CreateAsync(vm, ct);

            if (!op.Ok || op.Value is null)
            {
                ModelState.AddModelError(string.Empty, op.Error ?? (vm.IsEdit ? "Unable to update product." : "Unable to create product."));
                return View(UpsertViewName, vm);
            }

            var saved = op.Value;
            TempData["AlertMessage"] = vm.IsEdit
                ? $"Product '{saved.ProductName}' updated."
                : $"Product '{saved.ProductName}' created.";

            return !string.IsNullOrWhiteSpace(saved.ProductCode)
                ? RedirectToAction(nameof(DetailsByCode), new { code = saved.ProductCode })
                : RedirectToAction(nameof(DetailsById), new { id = saved.Id });
        }

        // Centralized: set error + go back to Warehouse index
        private RedirectToActionResult GoIndexWithError(string message)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(
                actionName: "Index",
                controllerName: "Warehouse",
                routeValues: new { area = "Warehouse" }
            );
        }
    }
}