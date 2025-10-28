using ASAPPVC.UI.Models;
using ASAPPVC.UI.Services;
using ASAPPVC.UI.ViewModels.Warehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.UI.Controllers.Warehouse
{
    [Area("Warehouse")]
    [Authorize]
    public class ComponentsController(IComponentService components) : Controller
    {
        private readonly IComponentService _components = components;

        // Component view paths - reuse WarehouseController.ViewRoot
        public const string ViewRoot = WarehouseController.ViewRoot + "Components/";

        private const string ManageComponentsViewName = ViewRoot + "ManageComponents.cshtml";
        private const string ViewComponentViewName = ViewRoot + "ViewComponent.cshtml";
        private const string AddComponentViewName = ViewRoot + "AddComponent.cshtml";

        [HttpGet]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _components.ListAsync(ct);
            if (!result.Ok || result.Value is null)
            {
                TempData["ErrorMessage"] = result.Error ?? "Failed to load components.";
                return View(ManageComponentsViewName, ManageComponentsVm.Create());
            }

            var vm = ManageComponentsVm.Create(result.Value);
            return View(ManageComponentsViewName, vm);
        }

        [HttpGet]
        public IActionResult AddComponent()
        {
            return View(AddComponentViewName, new ComponentFormVm());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComponent(ComponentFormVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(AddComponentViewName, vm);

            var result = await _components.CreateAsync(vm, ct);
            if (!result.Ok || result.Value is null)
            {
                ModelState.AddModelError(string.Empty, result.Error ?? "Unable to create component.");
                return View(AddComponentViewName, vm);
            }

            var component = result.Value;
            TempData["AlertMessage"] = $"Component '{component.Name}' created.";
            return RedirectToAction(nameof(ViewComponent), new { id = component.Id });
        }

        [HttpGet]
        public async Task<IActionResult> ViewComponent(Guid id, CancellationToken ct)
        {
            var result = await _components.GetDetailAsync(id: id, ct: ct);
            if (!result.Ok || result.Value is null)
            {
                TempData["ErrorMessage"] = result.Error ?? "Component not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(ViewComponentViewName, result.Value);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string? term, CancellationToken ct)
        {
            var result = await _components.SearchAsync(term, ct);
            if (!result.Ok || result.Value is null)
            {
                TempData["ErrorMessage"] = result.Error ?? "Failed to search components.";
                return View(ManageComponentsViewName, ManageComponentsVm.Create());
            }

            var vm = ManageComponentsVm.Create(result.Value, searchQuery: term);
            return View(ManageComponentsViewName, vm);
        }
    }
}