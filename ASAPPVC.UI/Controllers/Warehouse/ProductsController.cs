using ASAPPVC.UI.Models;
using ASAPPVC.UI.Services;
using ASAPPVC.UI.ViewModels.Warehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers.Warehouse
{
    [Area("Warehouse")]
    [Authorize]
    public class ProductsController(IProductService products, IComponentService components) : Controller
    {
        private readonly IProductService _products = products;
        private readonly IComponentService _components = components;

        // Products view paths - reuse WarehouseController.ViewRoot
        public const string ViewRoot = WarehouseController.ViewRoot + "Products/";

        private const string ManageProductsViewName = ViewRoot + "ManageProducts.cshtml";
        private const string ViewProductViewName = ViewRoot + "ViewProduct.cshtml";
        private const string AddProductViewName = ViewRoot + "AddProduct.cshtml";

        // Views the products main page
        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _products.ListAsync(ct);
            if (!result.Ok || result.Value is null)
            {
                TempData["ErrorMessage"] = result.Error ?? "Failed to load products.";
                return View(ManageProductsViewName, ManageProductsVm.Create());
            }

            var vm = ManageProductsVm.Create(result.Value);
            return View(ManageProductsViewName, vm);
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct(CancellationToken ct)
        {
            var result = await _components.ListAsync(ct);
            var componentList = result.Ok && result.Value is not null ? result.Value : new List<ComponentListVm>();
            ViewData["Components"] = componentList;
            return View(AddProductViewName, new CreateProductVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(CreateProductVm vm, CancellationToken ct)
        {
            // If a file was uploaded via the form input named 'ImageFile', read it into the VM
            if (HttpContext.Request?.Form?.Files?.Count > 0)
            {
                var file = HttpContext.Request.Form.Files.FirstOrDefault(f => f.Name == "ImageFile" || f.Name == "imageFile");
                if (file is not null && file.Length > 0)
                {
                    using var ms = new MemoryStream();
                    await file.CopyToAsync(ms, ct);
                    vm.ImageData = ms.ToArray();
                    vm.ImageType = file.ContentType;
                }
            }

            if (!ModelState.IsValid)
            {
                // Reload components for the form
                var componentsResult = await _components.ListAsync(ct);
                ViewData["Components"] = componentsResult.Ok && componentsResult.Value is not null
                    ? componentsResult.Value
                    : new List<ComponentListVm>();
                return View(AddProductViewName, vm);
            }

            var result = await _products.CreateAsync(vm, ct);
            if (!result.Ok || result.Value is null)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Unable to create product.");

                // Reload components for the form
                var componentsResult = await _components.ListAsync(ct);
                ViewData["Components"] = componentsResult.Ok && componentsResult.Value is not null
                    ? componentsResult.Value
                    : new List<ComponentListVm>();
                return View(AddProductViewName, vm);
            }

            var product = result.Value;
            TempData["AlertMessage"] = $"Product '{vm.Name}' created successfully.";
            return RedirectToAction(nameof(ViewProduct), new { id = product.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ViewProduct(Guid id, CancellationToken ct)
        {
            var result = await _products.GetDetailAsync(id: id, ct: ct);
            if (!result.Ok || result.Value is null)
            {
                TempData["ErrorMessage"] = result.Error ?? "Product not found.";
                return RedirectToAction(nameof(Index));
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
                return View(ManageProductsViewName, ManageProductsVm.Create());
            }

            var vm = ManageProductsVm.Create(result.Value, searchQuery: term);
            return View(ManageProductsViewName, vm);
        }
    }
}