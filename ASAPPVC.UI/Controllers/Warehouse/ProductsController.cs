using ASAPPVC.UI.Models;
using ASAPPVC.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers.Warehouse
{
    [Authorize]
    public class ProductsController(IProductService products, IComponentService components) : Controller
    {
        private readonly IProductService _products = products;
        private readonly IComponentService _components = components;

        // Products view paths - reuse WarehouseController.ViewRoot
        public const string ViewRoot = WarehouseController.ViewRoot + "Products/";

        private const string ManageProductsViewName = ViewRoot + "ManageProducts";
        private const string ViewProductViewName = ViewRoot + "ViewProduct";
        private const string AddProductViewName = ViewRoot + "AddProduct";

        // Views the products main page
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _products.ListAsync(ct);
            if (!result.Ok || result.Value is null)
            {
                TempData["ErrorMessage"] = result.Error ?? "Failed to load products.";
                return View(ManageProductsViewName, new List<ProductListVm>());
            }

            return View(ManageProductsViewName, result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> ViewProductInventory(CancellationToken ct)
        {
            var result = await _products.ListAsync(ct);
            if (!result.Ok || result.Value is null)
            {
                TempData["ErrorMessage"] = result.Error ?? "Failed to load products.";
                return View(ManageProductsViewName, new List<ProductListVm>());
            }

            return View(ManageProductsViewName, result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> AddProductGet(CancellationToken ct)
        {
            var result = await _components.GetComponentsListAsync(ct);
            var componentList = result.Ok && result.Value is not null ? result.Value : new List<Component>();
            ViewData["Components"] = componentList;
            return View(AddProductViewName, new CreateProductVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductPost(CreateProductVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
            {
                // Reload components for the form
                var componentsResult = await _components.GetComponentsListAsync(ct);
                ViewData["Components"] = componentsResult.Ok && componentsResult.Value is not null
                    ? componentsResult.Value
                    : new List<Component>();
                return View(AddProductViewName, vm);
            }

            var result = await _products.CreateAsync(vm, ct);
            if (!result.Ok || result.Value is null)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Unable to create product.");

                // Reload components for the form
                var componentsResult = await _components.GetComponentsListAsync(ct);
                ViewData["Components"] = componentsResult.Ok && componentsResult.Value is not null
                    ? componentsResult.Value
                    : new List<Component>();
                return View(AddProductViewName, vm);
            }

            TempData["AlertMessage"] = $"Product '{vm.Name}' created successfully.";
            return RedirectToAction(nameof(ViewProductInventory));
        }

        [HttpGet]
        public async Task<IActionResult> ViewProduct(Guid id, CancellationToken ct)
        {
            var result = await _products.GetDetailAsync(id: id, ct: ct);
            if (!result.Ok || result.Value is null)
            {
                TempData["ErrorMessage"] = result.Error ?? "Product not found.";
                return RedirectToAction(nameof(ViewProductInventory));
            }

            return View(ViewProductViewName, result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? term, CancellationToken ct)
        {
            var result = await _products.SearchAsync(term, ct);
            if (!result.Ok || result.Value is null)
            {
                TempData["ErrorMessage"] = result.Error ?? "Failed to search products.";
                return View(ManageProductsViewName, new List<ProductListVm>());
            }

            return View(ManageProductsViewName, result.Value);
        }
    }
}