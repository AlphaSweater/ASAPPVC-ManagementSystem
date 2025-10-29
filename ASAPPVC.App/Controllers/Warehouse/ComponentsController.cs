using ASAPPVC.App.Models;
using ASAPPVC.App.Services;
using ASAPPVC.App.ViewModels.Warehouse;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ASAPPVC.App.Controllers.Warehouse
{
    [Area("Warehouse")]
    [Authorize]
    [AutoValidateAntiforgeryToken]
    [Route("Warehouse/[controller]")]
    public class ComponentsController(IComponentService components) : Controller
    {
        private readonly IComponentService _components = components;

        public const string ViewRoot = WarehouseController.ViewRoot + "Components/";
        private const string ManageComponentsViewName = ViewRoot + "ManageComponents.cshtml";
        private const string DetailsViewName = ViewRoot + "ViewComponent.cshtml";
        private const string UpsertViewName = ViewRoot + "UpsertComponent.cshtml";

        // GET /Warehouse/Components
        [HttpGet("")]
        public async Task<IActionResult> Index(CancellationToken ct)
        {
            var result = await _components.ListAsync(ct);
            if (!result.Ok || result.Value is null)
                return GoIndexWithError(result.Error ?? "Failed to load components.");

            return View(ManageComponentsViewName, ManageComponentsVm.Create(result.Value));
        }

        // GET /Warehouse/Components/search?term=...
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string? term, CancellationToken ct)
        {
            var result = await _components.SearchAsync(term, ct);
            if (!result.Ok || result.Value is null)
                return GoIndexWithError(result.Error ?? "Failed to search components.");

            return View(ManageComponentsViewName, ManageComponentsVm.Create(result.Value, searchQuery: term));
        }

        // GET /Warehouse/Components/details/{identifier}
        // {identifier} may be a GUID (Id) or a human-friendly code (ComponentCode).
        [HttpGet("Details/{identifier}")]
        public async Task<IActionResult> Details(string identifier, CancellationToken ct)
        {
            identifier = (identifier ?? string.Empty).Trim();
            if (identifier.Length == 0)
                return GoIndexWithError("Component not found.");

            var isGuid = Guid.TryParse(identifier, out var id);
            var result = await _components.GetDetailAsync(
                id: isGuid ? id : null,
                code: isGuid ? null : identifier,
                ct: ct
            );

            if (!result.Ok || result.Value is null)
                return GoIndexWithError(result.Error ?? "Component not found.");

            return View(DetailsViewName, result.Value);
        }

        // GET /Warehouse/Components/AddNew         -> create
        // GET /Warehouse/Components/Edit/{code}    -> edit by code
        [HttpGet("AddNew")]
        [HttpGet("Edit/{code}")]
        public async Task<IActionResult> Upsert(string? code, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(code))
                return View(UpsertViewName, new ComponentFormVm());

            var result = await _components.GetFormAsync(code: code.Trim(), ct: ct);
            if (!result.Ok || result.Value is null)
                return GoIndexWithError(result.Error ?? "Component not found.");

            return View(UpsertViewName, result.Value);
        }

        // POST /Warehouse/Components/upsert
        [HttpPost("Upsert")]
        public async Task<IActionResult> Upsert([FromForm] ComponentFormVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(UpsertViewName, vm);

            var operation = vm.IsEdit
                ? await _components.UpdateAsync(vm, ct)
                : await _components.CreateAsync(vm, ct);

            if (!operation.Ok || operation.Value is null)
            {
                ModelState.AddModelError(string.Empty, operation.Error ?? (vm.IsEdit ? "Unable to update component." : "Unable to create component."));
                return View(UpsertViewName, vm);
            }

            var saved = operation.Value;
            TempData["AlertMessage"] = vm.IsEdit
                ? $"Component '{saved.Name}' updated."
                : $"Component '{saved.Name}' created.";

            var identifier = string.IsNullOrWhiteSpace(saved.ComponentCode)
                ? saved.Id.ToString()
                : saved.ComponentCode;

            return RedirectToAction(nameof(Details), new { identifier });
        }

        // Centralized: set error + go back to index
        private RedirectToActionResult GoIndexWithError(string message)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}