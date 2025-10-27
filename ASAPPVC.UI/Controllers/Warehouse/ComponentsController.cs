using ASAPPVC.UI.Models;
using ASAPPVC.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers.Warehouse
{
    [Authorize]
    public class ComponentsController : Controller
    {
        private readonly IComponentService _components;

        // Component view paths - reuse WarehouseController.ViewRoot
        public const string ViewRoot = WarehouseController.ViewRoot + "Components/";

        private const string AddComponentPath = ViewRoot + "AddComponent.cshtml";
        private const string ViewComponentPath = ViewRoot + "ViewComponent.cshtml";
        private const string ComponentInventoryPath = ViewRoot + "ViewComponentInventory.cshtml";

        public ComponentsController(IComponentService components)
        {
            _components = components;
        }

        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult AddComponent()
        {
            return View(AddComponentPath);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComponent(CreateComponentVm vm, CancellationToken ct)
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

        [HttpGet]
        public async Task<IActionResult> ViewComponent(Guid id, CancellationToken ct)
        {
            var result = await _components.GetComponentByIdOrCodeAsync(id, null, ct);
            if (!result.Ok || result.Value is null)
                return NotFound();

            return View(ViewComponentPath, result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> ViewComponentInventory(CancellationToken ct)
        {
            var result = await _components.GetComponentsListAsync(ct);
            var list = result.Ok && result.Value is not null ? result.Value : new List<Component>();
            return View(ComponentInventoryPath, list);
        }
    }
}