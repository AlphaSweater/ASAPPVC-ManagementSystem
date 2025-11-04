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

        // Views
        public const string ViewRoot = WarehouseController.ViewRoot + "Components/";

        private const string ManageComponentsViewName = ViewRoot + "ManageComponents.cshtml";
        private const string DetailsViewName = ViewRoot + "ViewComponent.cshtml";
        private const string UpsertViewName = ViewRoot + "UpsertComponent.cshtml";

        // GET /Warehouse/Components?term=...
        [HttpGet("")]
        public async Task<IActionResult> Index([FromQuery] string? term, CancellationToken ct)
        {
            var query = term?.Trim();
            var result = string.IsNullOrWhiteSpace(query)
                ? await _components.ListAsync(ct)
                : await _components.SearchAsync(query, ct);

            if (!result.Ok || result.Value is null)
                return GoIndexWithError(result.Error ?? "Failed to load components.");

            return View(ManageComponentsViewName, ManageComponentsVm.Create(result.Value, searchQuery: query));
        }

        // GET /Warehouse/Components/Search?term=...
        [HttpGet("Search")]
        public async Task<IActionResult> Search([FromQuery] string? term, CancellationToken ct)
        {
            var query = term?.Trim();
            var result = string.IsNullOrWhiteSpace(query)
                ? await _components.ListAsync(ct)
                : await _components.SearchAsync(query, ct);

            if (!result.Ok || result.Value is null)
            {
                return Json(new { success = false, error = result.Error ?? "Failed to search components." });
            }

            var list = result.Value;

            return PartialView("~/Views/Shared/Partials/_ComponentRowsPartial.cshtml", list);
        }

        // GET /Warehouse/Components/View/{id:guid}
        [HttpGet("View/{id:guid}")]
        public Task<IActionResult> DetailsById([FromRoute] Guid id, CancellationToken ct)
        {
            return GetAndShowDetails(id: id, code: null, ct);
        }

        // GET /Warehouse/Components/View/{code}
        [HttpGet("View/{code}")]
        public Task<IActionResult> DetailsByCode([FromRoute] string code, CancellationToken ct)
        {
            var c = (code ?? string.Empty).Trim();
            return string.IsNullOrEmpty(c)
                ? Task.FromResult<IActionResult>(GoIndexWithError("Component not found."))
                : GetAndShowDetails(id: null, code: c, ct);
        }

        // --- Create ---

        // GET /Warehouse/Components/AddNew
        [HttpGet("AddNew")]
        public IActionResult AddNew()
        {
            return View(UpsertViewName, new ComponentFormVm());
        }

        // --- Edit ---

        // GET /Warehouse/Components/Edit/{id:guid}
        [HttpGet("Edit/{id:guid}")]
        public Task<IActionResult> EditById([FromRoute] Guid id, CancellationToken ct)
        {
            return GetAndShowForm(id: id, code: null, ct);
        }

        // GET /Warehouse/Components/Edit/{code}
        [HttpGet("Edit/{code}")]
        public Task<IActionResult> EditByCode([FromRoute] string code, CancellationToken ct)
        {
            var c = (code ?? string.Empty).Trim();
            return string.IsNullOrEmpty(c)
                ? Task.FromResult<IActionResult>(GoIndexWithError("Component not found."))
                : GetAndShowForm(id: null, code: c, ct);
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
                ? $"Component '{saved.ComponentName}' updated."
                : $"Component '{saved.ComponentName}' created.";

            return !string.IsNullOrWhiteSpace(saved.ComponentCode)
                ? RedirectToAction(nameof(DetailsByCode), new { code = saved.ComponentCode })
                : RedirectToAction(nameof(DetailsById), new { id = saved.Id });
        }

        // ===== Helpers =====

        private async Task<IActionResult> GetAndShowDetails(Guid? id, string? code, CancellationToken ct)
        {
            var res = await _components.GetDetailAsync(id: id, code: code, ct: ct);
            if (!res.Ok || res.Value is null)
                return GoIndexWithError(res.Error ?? "Component not found.");
            return View(DetailsViewName, res.Value);
        }

        private async Task<IActionResult> GetAndShowForm(Guid? id, string? code, CancellationToken ct)
        {
            var res = await _components.GetFormAsync(id: id, code: code, ct: ct);
            if (!res.Ok || res.Value is null)
                return GoIndexWithError(res.Error ?? "Component not found.");
            return View(UpsertViewName, res.Value);
        }

        // Centralized: set error + go back to index
        private RedirectToActionResult GoIndexWithError(string message)
        {
            TempData["ErrorMessage"] = message;
            return RedirectToAction(
                actionName: "Index",
                controllerName: "Warehouse",
                routeValues: new { area = "Warehouse" }
            );
        }
    }
}