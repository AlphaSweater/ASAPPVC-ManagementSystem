using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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

        // Parts view paths (kept explicit to match existing views)
        private const string AddComponentPath = "~/Views/Inventory/Components/AddComponent.cshtml";
        private const string ViewComponentPath = "~/Views/Inventory/Components/ViewComponent.cshtml";
        private const string ComponentInventoryPath = "~/Views/Inventory/Components/ViewComponentInventory.cshtml";

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
}}