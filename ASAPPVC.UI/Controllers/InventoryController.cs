using ASAPPVC.UI.Models.ViewModels.Inventory;
using ASAPPVC.UI.Repositories.Interfaces;
using ASAPPVC.UI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        // ─────────── Dependencies ───────────
        private readonly IPartService _parts;
        private readonly IProductService _products;
        private readonly IPartRepository _partsRepo;

        public InventoryController(IPartService parts, IProductService products, IPartRepository partsRepo)
        {
            _parts = parts;
            _products = products;
            _partsRepo = partsRepo;
        }

        // ─────────── View Paths ───────────
        // Parts
        private const string AddPartPath = "~/Views/Inventory/Parts/AddPart.cshtml";
        private const string ViewPartPath = "~/Views/Inventory/Parts/ViewPart.cshtml";
        private const string PartInventoryPath = "~/Views/Inventory/Parts/ViewPartInventory.cshtml";

        // Products
        private const string AddProductPath = "~/Views/Inventory/Products/AddProduct.cshtml";
        private const string ViewProductPath = "~/Views/Inventory/Products/ViewProduct.cshtml";
        private const string ProductInventoryPath = "~/Views/Inventory/Products/ViewProductInventory.cshtml";

        // ─────────── Parts: Create ───────────
        [HttpGet]
        public IActionResult AddPart() => View(AddPartPath);

        [HttpPost, ValidateAntiForgeryToken]
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

        // ─────────── Parts: Read/List ───────────
        [HttpGet]
        public async Task<IActionResult> ViewPart(int id, CancellationToken ct)
        {
            var part = await _parts.GetAsync(id, ct);
            if (part is null) return NotFound();
            return View(ViewPartPath, part);
        }

        [HttpGet]
        public async Task<IActionResult> ViewPartInventory(CancellationToken ct)
        {
            var list = await _parts.ListAsync(ct);
            return View(PartInventoryPath, list);
        }

        // ─────────── Products: Create ───────────
        // Use ActionName so view can post to asp-action="AddProduct"
        [HttpGet]
        public async Task<IActionResult> AddProductGet(CancellationToken ct)
        {
            // Provide real parts to the view so you can populate the <select>
            var parts = await _partsRepo.ListAsync(ct);
            ViewData["Parts"] = parts;
            return View(AddProductPath, new CreateProductViewModel());
        }

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
            // TODO: replace with a real Product detail/inventory action if desired
            return RedirectToAction(nameof(ViewPartInventory));
        }

        [HttpGet]
        public IActionResult ViewProduct(int id)
            => View(ViewProductPath);

        [HttpGet]
        public IActionResult ViewProductInventory()
            => View(ProductInventoryPath);
    }
}
