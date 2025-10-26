using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.ViewModels.Inventory.Component;
using ASAPPVC.UI.Models.ViewModels.Inventory.Product;
using ASAPPVC.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        //─────────── Dependencies ───────────\\
        private readonly IComponentService _components;

        private readonly IProductService _products;

        //constructor
        public InventoryController(IComponentService components, IProductService products)
        {
            _components = components;
            _products = products;
        }

        // Parts
        private const string AddComponentPath = "~/Views/Inventory/Components/AddComponent.cshtml";

        private const string ViewComponentPath = "~/Views/Inventory/Components/ViewComponent.cshtml";
        private const string ComponentInventoryPath = "~/Views/Inventory/Components/ViewComponentInventory.cshtml";

        // Products
        private const string AddProductPath = "~/Views/Inventory/Products/AddProduct.cshtml";

        private const string ViewProductPath = "~/Views/Inventory/Products/ViewProduct.cshtml";
        private const string ProductInventoryPath = "~/Views/Inventory/Products/ViewProductInventory.cshtml";

        [HttpGet]
        public IActionResult WarehouseDashboard()
        {
            return View();
        }

        //─────────── Parts ───────────\\
        //displays the add part view
        [HttpGet]
        public IActionResult AddComponent()
        {
            return View(AddComponentPath);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //handles the submission of the add part form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComponent(CreateComponentViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(AddComponentPath, vm);

            var result = await _components.CreateComponentAsync(vm, ct);
            if (!result.Ok || result.Value is null)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Unable to create component.");
                return View(AddComponentPath, vm);
            }

            var component = result.Value;
            TempData["AlertMessage"] = $"Component '{component.Name}' created.";
            return RedirectToAction(nameof(ViewComponent), new { id = component.Id });
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays a specific part by its ID
        [HttpGet]
        public async Task<IActionResult> ViewComponent(Guid id, CancellationToken ct)
        {
            var result = await _components.GetComponentByIdOrCodeAsync(id, null, ct);
            if (!result.Ok || result.Value is null)
                return NotFound();
            //Returns the view with the part view path and the part details
            return View(ViewComponentPath, result.Value);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //calls view to display part inventory
        [HttpGet]
        public async Task<IActionResult> ViewComponentInventory(CancellationToken ct)
        {
            var result = await _components.GetComponentsListAsync(ct);
            var list = result.Ok && result.Value is not null ? result.Value : new List<ComponentModel>();
            //returns the view with the part inventory path and the list of parts
            return View(ComponentInventoryPath, list);
        }

        //─────────── Products ───────────\\
        //displays the add product view
        [HttpGet]
        public async Task<IActionResult> AddProductGet(CancellationToken ct)
        {
            var result = await _components.GetComponentsListAsync(ct);
            var components = result.Ok && result.Value is not null ? result.Value : new List<ComponentModel>();
            ViewData["Components"] = components;
            return View(AddProductPath, new CreateProductViewModel());
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //handles the submission of the add product form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductPost(CreateProductViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(AddProductPath, vm);

            var (ok, error, productId) = await _products.CreateAsync(vm, ct);
            if (!ok || productId is null)
            {
                ModelState.AddModelError(string.Empty, error ?? "Unable to create product.");
                return View(AddProductPath, vm);
            }

            TempData["AlertMessage"] = $"Product '{vm.ProductName}' created.";
            return RedirectToAction(nameof(ViewProductInventory));
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays a specific product by its ID
        [HttpGet]
        public async Task<IActionResult> ViewProduct(Guid id, CancellationToken ct)
        {
            var product = await _products.GetAsync(id, ct);
            if (product is null)
                return NotFound();
            return View(ViewProductPath, product);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //calls view to display product inventory
        [HttpGet]
        public async Task<IActionResult> ViewProductInventory(CancellationToken ct)
        {
            var list = await _products.ListAsync(ct);
            return View(ProductInventoryPath, list);
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\