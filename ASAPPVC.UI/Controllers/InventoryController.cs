using ASAPPVC.UI.Models.ViewModels.Inventory;
using ASAPPVC.UI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers
{
    [Authorize]
    public class InventoryController : Controller
    {
        private readonly IPartService _parts;

        public InventoryController(IPartService parts)
        {
            _parts = parts;
        }

        //part paths
        private const string AddPartPath = "~/Views/Inventory/Parts/AddPart.cshtml";
        private const string ViewPartPath = "~/Views/Inventory/Parts/ViewPart.cshtml";
        private const string PartInventoryPath = "~/Views/Inventory/Parts/ViewPartInventory.cshtml";


        //Product paths
        private const string AddProductPath = "~/Views/Inventory/Products/AddProduct.cshtml";
        private const string ViewProductPath = "~/Views/Inventory/Products/ViewProduct.cshtml";
        private const string ProductInventoryPath = "~/Views/Inventory/Products/ViewProductInventory.cshtml";

        [HttpGet]
        public IActionResult AddPart()
            => View(AddPartPath);

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


        public IActionResult AddProduct() => View(AddProductPath);
        public IActionResult ViewProduct() => View(ViewProductPath);
        public IActionResult ViewProductInventory() => View(ProductInventoryPath);
    }
}
