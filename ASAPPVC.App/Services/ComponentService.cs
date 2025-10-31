using ASAPPVC.App.Models;
using ASAPPVC.App.Repositories;
using ASAPPVC.App.Utils;

namespace ASAPPVC.App.Services
{
    #region Interface

    /// <summary>
    /// Service contract for component-related business logic.
    /// Provides higher-level operations that orchestrate validation, normalization, mapping,
    /// and repository interactions for <see cref="Component"/> instances.
    /// </summary>
    public interface IComponentService
    {
        /// <summary>
        /// Creates a new component from the supplied view model.
        /// The service validates input, maps the VM to a domain entity using the component mapper,
        /// processes any uploaded image, and delegates persistence to the repository.
        /// </summary>
        /// <param name="vm">Create view model containing component details. Must not be null.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> carrying the created <see cref="Component"/> on success
        /// or an error message on failure.
        /// </returns>
        Task<Result<Component>> CreateAsync(ComponentFormVm vm, CancellationToken ct = default);

        /// <summary>
        /// Updates an existing component from the supplied edit view model.
        /// The service validates input, fetches the existing component, applies the edit VM using the mapper,
        /// and saves changes to the repository.
        /// </summary>
        /// <param name="vm">Edit view model containing updated component details. Must not be null.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> carrying the updated <see cref="Component"/> on success
        /// or an error message on failure.
        /// </returns>
        Task<Result<Component>> UpdateAsync(ComponentFormVm vm, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a single component by its internal identifier or by its human-friendly code.
        /// Returns the component mapped to a detail view model suitable for display.
        /// </summary>
        /// <param name="id">Optional internal GUID identifier of the component.</param>
        /// <param name="code">Optional human-friendly component code.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the found <see cref="ComponentDetailVm"/>, or a failure result
        /// if the component does not exist or an error occurs.
        /// </returns>
        Task<Result<ComponentDetailVm>> GetDetailAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a single component entity (not mapped) by its internal identifier or by its human-friendly code.
        /// Useful for operations that need the raw domain entity.
        /// </summary>
        /// <param name="id">Optional internal GUID identifier of the component.</param>
        /// <param name="code">Optional human-friendly component code.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the found <see cref="Component"/>, or a failure result
        /// if the component does not exist or an error occurs.
        /// </returns>
        Task<Result<Component>> GetDomainAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves a single component mapped to the form view model used for create/edit screens.
        /// Useful when prefilling an edit form or preparing a blank create form.
        /// </summary>
        /// <param name="id">Optional internal GUID identifier of the component.</param>
        /// <param name="code">Optional human-friendly component code.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing the found <see cref="ComponentFormVm"/>, or a failure result
        /// if the component does not exist or an error occurs.
        /// </returns>
        Task<Result<ComponentFormVm>> GetFormAsync(Guid? id = null, string? code = null, CancellationToken ct = default);

        /// <summary>
        /// Retrieves the full list of components mapped to lightweight list view models.
        /// Suitable for display in tables, cards, or summary views.
        /// </summary>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of <see cref="ComponentListVm"/> instances.
        /// </returns>
        Task<Result<List<ComponentListVm>>> ListAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves only available (active) components mapped to lightweight list view models.
        /// Useful for pickers and dropdowns where only active components should be selectable.
        /// </summary>
        Task<Result<List<ComponentListVm>>> GetAvailableAsync(CancellationToken ct = default);

        /// <summary>
        /// Retrieves multiple components by a collection of internal ids and/or human-friendly codes.
        /// Returns components mapped to list view models.
        /// </summary>
        /// <param name="ids">Optional collection of internal component ids to retrieve.</param>
        /// <param name="codes">Optional collection of human-friendly component codes to retrieve.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of matching <see cref="ComponentListVm"/> instances.
        /// </returns>
        Task<Result<List<ComponentListVm>>> GetListByIdsOrCodesAsync(
            IEnumerable<Guid>? ids = null,
            IEnumerable<string>? codes = null,
            CancellationToken ct = default);

        /// <summary>
        /// Searches components using a free-text term against name and component code.
        /// Returns matching components mapped to list view models.
        /// </summary>
        /// <param name="term">Search term to match against component properties. Null or empty means no filter.</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing a list of components that match the search term;
        /// an empty list indicates no matches.
        /// </returns>
        Task<Result<List<ComponentListVm>>> SearchAsync(string? term, CancellationToken ct = default);

        /// <summary>
        /// Deletes a component by its internal identifier.
        /// </summary>
        /// <param name="id">The internal GUID identifier of the component to delete.</param>
        /// <returns>
        /// A <see cref="Result"/> indicating success or failure with an error message.
        /// </returns>
        Task<Result> DeleteAsync(Guid id, CancellationToken ct = default);

        /// <summary>
        /// Checks if a component with the given code already exists.
        /// Useful for validation before creating or updating components.
        /// </summary>
        /// <param name="code">The component code to check.</param>
        /// <param name="excludeId">Optional component ID to exclude from the check (useful for updates).</param>
        /// <returns>
        /// A <see cref="Result{T}"/> containing true if the code exists, false otherwise.
        /// </returns>
        Task<Result<bool>> ExistsAsync(string code, Guid? excludeId = null, CancellationToken ct = default);
    }

    #endregion Interface

    public class ComponentService(
          IComponentRepository componentRepository,
          IComponentMapper componentMapper,
          IStockAlertServices stockAlertServices) : IComponentService
    {
        private readonly IComponentRepository _components = componentRepository;
        private readonly IComponentMapper _mapper = componentMapper;
        private readonly IStockAlertServices _stockAlerts = stockAlertServices;

        // ---------- Input validation + normalization helpers ----------

        private static Result ValidateCreateVm(ComponentFormVm? vm)
        {
            if (vm is null)
                return Result.Fail("Create view model is required.");
            if (string.IsNullOrWhiteSpace(vm.ComponentName))
                return Result.Fail("Component name is required.");
            if (string.IsNullOrWhiteSpace(vm.LocationCode))
                return Result.Fail("Storage location is required.");
            if (vm.UnitCost <= 0)
                return Result.Fail("Unit cost must be greater than zero.");
            if (vm.QuantityOnHand < 0)
                return Result.Fail("Quantity On Hand cannot be negative.");

            return Result.Success();
        }

        private static Result ValidateEditVm(ComponentFormVm? vm)
        {
            if (vm is null)
                return Result.Fail("Edit view model is required.");
            if (vm.Id == Guid.Empty)
                return Result.Fail("Component ID is required.");
            if (string.IsNullOrWhiteSpace(vm.ComponentCode))
                return Result.Fail("Component code is required.");
            if (string.IsNullOrWhiteSpace(vm.ComponentName))
                return Result.Fail("Component name is required.");
            if (string.IsNullOrWhiteSpace(vm.LocationCode))
                return Result.Fail("Storage location is required.");
            if (vm.UnitCost <= 0)
                return Result.Fail("Unit cost must be greater than zero.");
            if (vm.QuantityOnHand < 0)
                return Result.Fail("Current amount cannot be negative.");

            return Result.Success();
        }

        private static void Normalize(ComponentFormVm vm)
        {
            vm.ComponentName = vm.ComponentName.Trim();
            vm.LocationCode = vm.LocationCode.Trim().ToUpper();
            if (!string.IsNullOrWhiteSpace(vm.ComponentCode))
                vm.ComponentCode = vm.ComponentCode.Trim();
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Creates a new component from view model
        public async Task<Result<Component>> CreateAsync(ComponentFormVm vm, CancellationToken ct = default)
        {
            var validation = ValidateCreateVm(vm);
            if (!validation.Ok)
                return Result<Component>.Fail(validation.Error!);

            Normalize(vm);

            try
            {
                // Map VM to domain entity (mapper handles code generation and image processing)
                var component = await _mapper.FromCreateVmAsync(vm, ct);

                // Persist
                var added = await _components.AddAsync(component, ct);
                await _components.SaveAsync(ct);

                // Fire-and-forget style notification (respecting CancellationToken)
                await _stockAlerts.NotifyOnCreateAsync(added, ct);

                return Result<Component>.Success(added);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return Result<Component>.Fail("Operation was canceled.");
            }
            catch (Exception ex)
            {
                return Result<Component>.Fail($"Failed to create component: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Updates an existing component from edit view model
        public async Task<Result<Component>> UpdateAsync(ComponentFormVm vm, CancellationToken ct = default)
        {
            var validation = ValidateEditVm(vm);
            if (!validation.Ok)
                return Result<Component>.Fail(validation.Error!);

            Normalize(vm);

            try
            {
                // Fetch existing component (tracking enabled for update)
                var existing = await _components.GetByIdOrCodeAsync(vm.Id, asNoTracking: false, ct: ct);
                if (existing is null)
                    return Result<Component>.Fail("Component not found.");

                // Check if code changed and conflicts with another component
                if (existing.ComponentCode != vm.ComponentCode)
                {
                    var existsResult = await ExistsAsync(vm.ComponentCode!, excludeId: vm.Id, ct);
                    if (!existsResult.Ok)
                        return Result<Component>.Fail($"Failed to check component code existence: {existsResult.Error}");

                    if (existsResult.Value)
                        return Result<Component>.Fail($"Component code '{vm.ComponentCode}' is already in use.");
                }

                // Apply changes via mapper (handles image processing if new image uploaded)
                await _mapper.ApplyUpdateAsync(existing, vm, ct);

                // Persist changes
                _components.Update(existing);
                await _components.SaveAsync(ct);

                return Result<Component>.Success(existing);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                return Result<Component>.Fail("Operation was canceled.");
            }
            catch (Exception ex)
            {
                return Result<Component>.Fail($"Failed to update component: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves a single component detail (mapped to detail VM)
        public async Task<Result<ComponentDetailVm>> GetDetailAsync(
            Guid? id = null,
            string? code = null,
            CancellationToken ct = default)
        {
            try
            {
                var component = await _components.GetByIdOrCodeAsync(id, code, asNoTracking: true, ct);
                if (component is null)
                    return Result<ComponentDetailVm>.Fail("Component not found.");

                var detailVm = _mapper.ToDetailVm(component, usedInProductsCount: null, includeImageDataUrl: true);
                return Result<ComponentDetailVm>.Success(detailVm);
            }
            catch (Exception ex)
            {
                return Result<ComponentDetailVm>.Fail($"Failed to retrieve component detail: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves a component mapped to the form VM (for create/edit screens)
        public async Task<Result<ComponentFormVm>> GetFormAsync(
            Guid? id = null,
            string? code = null,
            CancellationToken ct = default)
        {
            try
            {
                // If no id/code provided, return an empty form VM ready for create
                if (id == null && string.IsNullOrWhiteSpace(code))
                {
                    var empty = new ComponentFormVm();
                    return Result<ComponentFormVm>.Success(empty);
                }

                var component = await _components.GetByIdOrCodeAsync(id, code, asNoTracking: true, ct);
                if (component is null)
                    return Result<ComponentFormVm>.Fail("Component not found.");

                var formVm = _mapper.ToFormVm(component);
                return Result<ComponentFormVm>.Success(formVm);
            }
            catch (Exception ex)
            {
                return Result<ComponentFormVm>.Fail($"Failed to retrieve component form: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves a single component entity (raw domain model)
        public async Task<Result<Component>> GetDomainAsync(
            Guid? id = null,
            string? code = null,
            CancellationToken ct = default)
        {
            try
            {
                var component = await _components.GetByIdOrCodeAsync(id, code, asNoTracking: true, ct);
                if (component is null)
                    return Result<Component>.Fail("Component not found.");

                return Result<Component>.Success(component);
            }
            catch (Exception ex)
            {
                return Result<Component>.Fail($"Failed to retrieve component: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves full component list (mapped to list VMs)
        public async Task<Result<List<ComponentListVm>>> ListAsync(CancellationToken ct = default)
        {
            try
            {
                var components = await _components.GetListOrderedByCodeAsync(asNoTracking: true, ct);
                var listVms = components.Select(c => _mapper.ToListVm(c)).ToList();
                return Result<List<ComponentListVm>>.Success(listVms);
            }
            catch (Exception ex)
            {
                return Result<List<ComponentListVm>>.Fail($"Failed to list components: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves available (active) components only
        public async Task<Result<List<ComponentListVm>>> GetAvailableAsync(CancellationToken ct = default)
        {
            try
            {
                var components = await _components.GetListOrderedByCodeAsync(asNoTracking: true, ct);
                var available = components.Where(c => c.IsActive).ToList();
                var listVms = available.Select(c => _mapper.ToListVm(c)).ToList();
                return Result<List<ComponentListVm>>.Success(listVms);
            }
            catch (Exception ex)
            {
                return Result<List<ComponentListVm>>.Fail($"Failed to retrieve available components: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves multiple components by IDs or codes (mapped to list VMs)
        public async Task<Result<List<ComponentListVm>>> GetListByIdsOrCodesAsync(
            IEnumerable<Guid>? ids = null,
            IEnumerable<string>? codes = null,
            CancellationToken ct = default)
        {
            try
            {
                var filteredIds = ids?.Where(g => g != Guid.Empty);
                var filteredCodes = codes?.Where(s => !string.IsNullOrWhiteSpace(s));

                var components = await _components.GetListByIdOrCodeAsync(filteredIds, filteredCodes, asNoTracking: true, ct);
                var listVms = components.Select(c => _mapper.ToListVm(c)).ToList();
                return Result<List<ComponentListVm>>.Success(listVms);
            }
            catch (Exception ex)
            {
                return Result<List<ComponentListVm>>.Fail($"Failed to retrieve components: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Searches components by term (name/code contains, case-insensitive)
        public async Task<Result<List<ComponentListVm>>> SearchAsync(string? term, CancellationToken ct = default)
        {
            try
            {
                term ??= string.Empty;
                var components = await _components.SearchAsync(term, asNoTracking: true, ct);
                var listVms = components.Select(c => _mapper.ToListVm(c)).ToList();
                return Result<List<ComponentListVm>>.Success(listVms);
            }
            catch (Exception ex)
            {
                return Result<List<ComponentListVm>>.Fail($"Failed to search components: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Deletes a component by ID
        public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
        {
            if (id == Guid.Empty)
                return Result.Fail("Component ID is required.");

            try
            {
                var removed = await _components.RemoveByIdAsync(id, ct);
                if (!removed)
                    return Result.Fail("Component not found.");

                await _components.SaveAsync(ct);
                return Result.Success();
            }
            catch (Exception ex)
            {
                return Result.Fail($"Failed to delete component: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Checks if a component code already exists (useful for validation)
        public async Task<Result<bool>> ExistsAsync(
            string code,
            Guid? excludeId = null,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(code))
                return Result<bool>.Fail("Component code is required.");

            try
            {
                var component = await _components.GetByCodeAsync(code.Trim(), asNoTracking: true, ct);

                if (component is null)
                    return Result<bool>.Success(false);

                // If we're excluding an ID (for update scenarios), check if it's the same component
                if (excludeId.HasValue && component.Id == excludeId.Value)
                    return Result<bool>.Success(false);

                return Result<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Failed to check component existence: {ex.Message}");
            }
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\