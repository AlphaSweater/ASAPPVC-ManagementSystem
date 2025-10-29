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

        // GET /Warehouse/Components/Search?term=...
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string? term, CancellationToken ct)
        {
            var result = await _components.SearchAsync(term, ct);
            if (!result.Ok || result.Value is null)
                return GoIndexWithError(result.Error ?? "Failed to search components.");

            return View(ManageComponentsViewName, ManageComponentsVm.Create(result.Value, searchQuery: term));
        }

        // GET /Warehouse/Components/View/{id:guid}
        [HttpGet("View/{id:guid}")]
        public async Task<IActionResult> DetailsById([FromRoute] Guid id, CancellationToken ct)
        {
            var byId = await _components.GetDetailAsync(id: id, code: null, ct: ct);
            if (!byId.Ok || byId.Value is null)
                return GoIndexWithError(byId.Error ?? "Component not found.");

            return View(DetailsViewName, byId.Value);
        }

        // GET /Warehouse/Components/View/{code}
        [HttpGet("View/{code}")]
        public async Task<IActionResult> DetailsByCode([FromRoute] string code, CancellationToken ct)
        {
            code = (code ?? string.Empty).Trim();
            if (code.Length == 0)
                return GoIndexWithError("Component not found.");

            var byCode = await _components.GetDetailAsync(id: null, code: code, ct: ct);
            if (!byCode.Ok || byCode.Value is null)
                return GoIndexWithError(byCode.Error ?? "Component not found.");

            return View(DetailsViewName, byCode.Value);
        }

        // --- Create ---

        // GET /Warehouse/Components/AddNew
        [HttpGet("AddNew")]
        public IActionResult AddNew()
        {
            return View(UpsertViewName, new ComponentFormVm());
        }

        // GET /Warehouse/Components/Edit/{id:guid}
        [HttpGet("Edit/{id:guid}")]
        public async Task<IActionResult> EditById([FromRoute] Guid id, CancellationToken ct)
        {
            var byId = await _components.GetFormAsync(id: id, ct: ct);
            if (!byId.Ok || byId.Value is null)
                return GoIndexWithError(byId.Error ?? "Component not found.");

            return View(UpsertViewName, byId.Value);
        }

        // GET /Warehouse/Components/Edit/{code}
        [HttpGet("Edit/{code}")]
        public async Task<IActionResult> EditByCode([FromRoute] string code, CancellationToken ct)
        {
            code = (code ?? string.Empty).Trim();
            if (code.Length == 0)
                return GoIndexWithError("Component not found.");

            var byCode = await _components.GetFormAsync(code: code, ct: ct);
            if (!byCode.Ok || byCode.Value is null)
                return GoIndexWithError(byCode.Error ?? "Component not found.");

            return View(UpsertViewName, byCode.Value);
        }

        // --- Save (create or update) ---

        // POST /Warehouse/Components/Upsert
        [HttpPost("Upsert")]
        public async Task<IActionResult> Upsert([FromForm] ComponentFormVm vm, CancellationToken ct)
        {
            if (!ModelState.IsValid)
                return View(UpsertViewName, vm);

            var op = vm.IsEdit
                ? await _components.UpdateAsync(vm, ct)
                : await _components.CreateAsync(vm, ct);

            if (!op.Ok || op.Value is null)
            {
                ModelState.AddModelError(string.Empty, op.Error ?? (vm.IsEdit ? "Unable to update component." : "Unable to create component."));
                return View(UpsertViewName, vm);
            }

            var saved = op.Value;
            TempData["AlertMessage"] = vm.IsEdit
                ? $"Component '{saved.Name}' updated."
                : $"Component '{saved.Name}' created.";

            // Prefer friendly code when available
            return !string.IsNullOrWhiteSpace(saved.ComponentCode)
                ? RedirectToAction(nameof(DetailsByCode), new { code = saved.ComponentCode })
                : RedirectToAction(nameof(DetailsById), new { id = saved.Id });
        }

        // Centralized: set error + go back to index
        private RedirectToActionResult GoIndexWithError(string message)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(nameof(Index));
        }
    }
}