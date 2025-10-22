using ASAPPVC.UI.Models.ViewModels.Inventory;
using ASAPPVC.UI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        //─────────── Dependencies ───────────\\
        private readonly IPartService _parts;
        private readonly IProductService _products;

        public InventoryController(IPartService parts, IProductService products)
        {
            _parts = parts;
            _products = products;
        }

        // Parts
        private const string AddPartPath = "~/Views/Inventory/Parts/AddPart.cshtml";
        private const string ViewPartPath = "~/Views/Inventory/Parts/ViewPart.cshtml";
        private const string PartInventoryPath = "~/Views/Inventory/Parts/ViewPartInventory.cshtml";

        // Products
        private const string AddProductPath = "~/Views/Inventory/Products/AddProduct.cshtml";
        private const string ViewProductPath = "~/Views/Inventory/Products/ViewProduct.cshtml";
        private const string ProductInventoryPath = "~/Views/Inventory/Products/ViewProductInventory.cshtml";

        //─────────── Parts ───────────\\
        //displays the add part view
        [HttpGet]
        public IActionResult AddPart() => View(AddPartPath);

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //handles the submission of the add part form
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPart(CreatePartViewModel vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(AddPartPath, vm);

            var (ok, error, part) = await _parts.CreateAsync(vm, ct);
            if (!ok || part is null)
            {
                ModelState.AddModelError(string.Empty, error ?? "Unable to create part.");
                return View(AddPartPath, vm);
            }

            TempData["AlertMessage"] = $"Part '{part.Name}' created.";
            return RedirectToAction(nameof(ViewPart), new { id = part.PartID });
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays a specific part by its ID
        [HttpGet]
        public async Task<IActionResult> ViewPart(int id, CancellationToken ct)
        {
            var part = await _parts.GetAsync(id, ct);
            if (part is null) return NotFound();
            //Returns the view with the part view path and the part details
            return View(ViewPartPath, part);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //calls view to display part inventory
        [HttpGet]
        public async Task<IActionResult> ViewPartInventory(CancellationToken ct)
        {
            var list = await _parts.ListAsync(ct);
            //returns the view with the part inventory path and the list of parts
            return View(PartInventoryPath, list);
        }

        //─────────── Products ───────────\\
        //displays the add product view
        [HttpGet]
        public async Task<IActionResult> AddProductGet(CancellationToken ct)
        {
            var parts = await _parts.ListAsync(ct);
            ViewData["Parts"] = parts;
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
            return RedirectToAction(nameof(ViewPartInventory));
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //displays a specific product by its ID
        [HttpGet]
        public async Task<IActionResult> ViewProduct(int id, CancellationToken ct)
        {
            var product = await _products.GetAsync(id, ct);
            if (product is null) return NotFound();
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