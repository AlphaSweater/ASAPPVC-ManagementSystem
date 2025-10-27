using ASAPPVC.UI.Models;
using ASAPPVC.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers.Warehouse
{
    [Authorize]
    public class ProductsController : Controller
    {
        private readonly IProductService _products;
        private readonly IComponentService _components;

        // Products view paths
        private const string AddProductPath = "~/Views/Inventory/Products/AddProduct.cshtml";

        private const string ViewProductPath = "~/Views/Inventory/Products/ViewProduct.cshtml";
        private const string ProductInventoryPath = "~/Views/Inventory/Products/ViewProductInventory.cshtml";

        public ProductsController(IProductService products, IComponentService components)
        {
            _products = products;
            _components = components;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddProductGet(CancellationToken ct)
        {
            var result = await _components.GetComponentsListAsync(ct);
            var components = result.Ok && result.Value is not null ? result.Value : new List<Component>();
            ViewData["Components"] = components;
            return View(AddProductPath, new CreateProductVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProductPost(CreateProductVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(AddProductPath, vm);

            var (ok, error, productId) = await _products.CreateAsync(vm, ct);
            if (!ok || productId is null)
            {
                ModelState.AddModelError(string.Empty, error ?? "Unable to create product.");
                return View(AddProductPath, vm);
            }

            TempData["AlertMessage"] = $"Product '{vm.Name}' created.";
            return RedirectToAction(nameof(ViewProductInventory));
        }

        [HttpGet]
        public async Task<IActionResult> ViewProduct(Guid id, CancellationToken ct)
        {
            var product = await _products.GetAsync(id, ct);
            if (product is null)
                return NotFound();
            return View(ViewProductPath, product);
        }

        [HttpGet]
        public async Task<IActionResult> ViewProductInventory(CancellationToken ct)
        {
            var list = await _products.ListAsync(ct);
            return View(ProductInventoryPath, list);
        }
    }
}