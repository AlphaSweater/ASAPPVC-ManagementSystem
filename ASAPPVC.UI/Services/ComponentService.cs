using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.Mappers;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Utils;

namespace ASAPPVC.UI.Services
{
    public class ComponentService(
          IComponentRepository componentRepository,
          IComponentMapper componentMapper) : IComponentService
    {
        private readonly IComponentRepository _components = componentRepository;
        private readonly IComponentMapper _mapper = componentMapper;

        // ---------- Input validation + normalization helpers ----------

        private static Result ValidateCreateVm(ComponentFormVm? vm)
        {
            if (vm is null)
                return Result.Fail("Create view model is required.");
            if (string.IsNullOrWhiteSpace(vm.Name))
                return Result.Fail("Component name is required.");
            if (string.IsNullOrWhiteSpace(vm.StorageLocation))
                return Result.Fail("Storage location is required.");
            if (vm.UnitCost <= 0)
                return Result.Fail("Unit cost must be greater than zero.");
            if (vm.CurrentAmount < 0)
                return Result.Fail("Current amount cannot be negative.");

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
            if (string.IsNullOrWhiteSpace(vm.Name))
                return Result.Fail("Component name is required.");
            if (string.IsNullOrWhiteSpace(vm.StorageLocation))
                return Result.Fail("Storage location is required.");
            if (vm.UnitCost <= 0)
                return Result.Fail("Unit cost must be greater than zero.");
            if (vm.CurrentAmount < 0)
                return Result.Fail("Current amount cannot be negative.");

            return Result.Success();
        }

        private static void Normalize(ComponentFormVm vm)
        {
            vm.Name = vm.Name.Trim();
            vm.StorageLocation = vm.StorageLocation.Trim();
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
                await _mapper.ApplyUpdateVmAsync(existing, vm, ct);

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