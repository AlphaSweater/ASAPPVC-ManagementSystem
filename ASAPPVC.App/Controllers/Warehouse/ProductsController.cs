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
    public class ProductsController(IProductService products, IComponentService components, IProductComponentMapper productComponentMapper) : Controller
    {
        private readonly IProductService _products = products;
        private readonly IComponentService _components = components;
        private readonly IProductComponentMapper _productComponentMapper = productComponentMapper;

        public const string ViewRoot = WarehouseController.ViewRoot + "Products/";
        private const string ManageProductsViewName = ViewRoot + "ManageProducts.cshtml";
        private const string DetailsViewName = ViewRoot + "ViewProduct.cshtml";
        private const string UpsertViewName = ViewRoot + "UpsertProduct.cshtml";

        // GET /Warehouse/Products?term=...
        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] string? term, CancellationToken ct)
        {
            var result = string.IsNullOrWhiteSpace(term)
                ? await _products.ListAsync(ct)
                : await _products.SearchAsync(term, ct);

            if (!result.Ok || result.Value is null)
                return GoIndexWithError(result.Error ?? "Failed to load products.");

            return View(ManageProductsViewName, ManageProductsVm.Create(result.Value, searchQuery: term));
        }

        // GET /Warehouse/Products/Search?term=...
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string? term, CancellationToken ct)
        {
            var query = term?.Trim();
            var result = string.IsNullOrWhiteSpace(query)
                ? await _products.ListAsync(ct)
                : await _products.SearchAsync(query, ct);

            if (!result.Ok || result.Value is null)
                return Json(new { success = false, error = result.Error ?? "Failed to search products." });

            var list = result.Value;

            return PartialView("~/Views/Shared/Partials/_ProductRowsPartial.cshtml", list);
        }

        // GET /Warehouse/Products/View/{id:guid}
        [HttpGet("View/{id:guid}")]
        public Task<IActionResult> DetailsById([FromRoute] Guid id, CancellationToken ct)
        {
            return GetAndShowDetails(id: id, code: null, ct);
        }

        // GET /Warehouse/Products/View/{code}
        [HttpGet("View/{code}")]
        public Task<IActionResult> DetailsByCode([FromRoute] string code, CancellationToken ct)
        {
            return string.IsNullOrWhiteSpace(code)
                        ? Task.FromResult<IActionResult>(GoIndexWithError("Product not found."))
                        : GetAndShowDetails(id: null, code: code.Trim(), ct);
        }

        // ---- Create ----
        [HttpGet("AddNew")]
        public async Task<IActionResult> AddNew(CancellationToken ct)
        {
            return View(UpsertViewName, await PopulateLookupsAsync(new ProductFormVm(), ct));
        }

        // ---- Edit ----
        [HttpGet("Edit/{id:guid}")]
        public Task<IActionResult> EditById([FromRoute] Guid id, CancellationToken ct)
        {
            return GetAndShowForm(id: id, code: null, ct);
        }

        [HttpGet("Edit/{code}")]
        public Task<IActionResult> EditByCode([FromRoute] string code, CancellationToken ct)
        {
            return string.IsNullOrWhiteSpace(code)
                        ? Task.FromResult<IActionResult>(GoIndexWithError("Product not found."))
                        : GetAndShowForm(id: null, code: code.Trim(), ct);
        }

        // ---- Upsert ----
        [HttpPost("Upsert")]
        public async Task<IActionResult> Upsert([FromForm] ProductFormVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(UpsertViewName, await PopulateLookupsAsync(vm, ct)); // repopulate

            var op = vm.IsEdit
                ? await _products.UpdateAsync(vm, ct)
                : await _products.CreateAsync(vm, ct);

            if (!op.Ok || op.Value is null)
            {
                ModelState.AddModelError(string.Empty, op.Error ?? (vm.IsEdit ? "Unable to update product." : "Unable to create product."));
                return View(UpsertViewName, await PopulateLookupsAsync(vm, ct)); // repopulate on error too
            }

            var saved = op.Value;
            TempData["AlertMessage"] = vm.IsEdit
                ? $"Product '{saved.ProductName}' updated."
                : $"Product '{saved.ProductName}' created.";

            return !string.IsNullOrWhiteSpace(saved.ProductCode)
                ? RedirectToAction(nameof(DetailsByCode), new { code = saved.ProductCode })
                : RedirectToAction(nameof(DetailsById), new { id = saved.Id });
        }

        // ===== Helpers =====

        private async Task<IActionResult> GetAndShowDetails(Guid? id, string? code, CancellationToken ct)
        {
            var res = await _products.GetDetailAsync(id, code, ct);
            if (!res.Ok || res.Value is null)
                return GoIndexWithError(res.Error ?? "Product not found.");
            return View(DetailsViewName, res.Value);
        }

        private async Task<IActionResult> GetAndShowForm(Guid? id, string? code, CancellationToken ct)
        {
            var res = await _products.GetFormAsync(id: id, code: code, ct: ct);
            if (!res.Ok || res.Value is null)
                return GoIndexWithError(res.Error ?? "Product not found.");

            var vm = await PopulateLookupsAsync(res.Value, ct);
            return View(UpsertViewName, vm);
        }

        private async Task<ProductFormVm> PopulateLookupsAsync(ProductFormVm vm, CancellationToken ct)
        {
            var productComponents = await _components.GetAvailableAsync(ct);
            if (productComponents.Ok && productComponents.Value is not null)
            {
                vm.AvailableProductComponents = productComponents.Value
                    .Select(c => _productComponentMapper.FromComponentListVm(c))
                    .ToList();
            }
            else
            {
                vm.AvailableProductComponents = new();
            }

            return vm;
        }

        // Centralized: set error + go back to Warehouse index
        private RedirectToActionResult GoIndexWithError(string message)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(actionName: "Index", controllerName: "Warehouse", routeValues: new { area = "Warehouse" });
        }
    }
}