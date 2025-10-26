using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.ViewModels.Inventory.Component;
using ASAPPVC.UI.Repositories;
using ASAPPVC.UI.Utils;

namespace ASAPPVC.UI.Services
{
    public class ComponentService(IComponentRepository componentRepository) : IComponentService
    {
        private readonly IComponentRepository _components = componentRepository;

        // ---------- Input validation + normalization helpers ----------

        private static (bool ok, string? error) ValidateCreateVm(CreateComponentViewModel? vm)
        {
            if (vm is null)
                return (false, "Create view model is required.");
            if (string.IsNullOrWhiteSpace(vm.Name))
                return (false, "Component name is required.");
            if (string.IsNullOrWhiteSpace(vm.StorageLocation))
                return (false, "Storage location is required.");
            return (true, null);
        }

        private static void Normalize(CreateComponentViewModel vm)
        {
            vm.Name = vm.Name.Trim();
            vm.StorageLocation = vm.StorageLocation.Trim();
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Creates a new component (reads image once, defers Save to repo)
        public async Task<Result<Component>> CreateComponentAsync(CreateComponentViewModel vm, CancellationToken ct = default)
        {
            var (ok, error) = ValidateCreateVm(vm);
            if (!ok)
                return Result<Component>.Fail(error!);

            Normalize(vm);

            var component = new Component
            {
                Name = vm.Name,
                StorageLocation = vm.StorageLocation,
                UnitCost = vm.UnitCost,
                CurrentAmount = vm.CurrentAmount
            };

            if (vm.ImageFile is { Length: > 0 })
            {
                try
                {
                    using var ms = new MemoryStream();
                    await vm.ImageFile.CopyToAsync(ms, ct);
                    component.ImageBytes = ms.ToArray();
                    component.ImageContentType = vm.ImageFile.ContentType;
                }
                catch (OperationCanceledException) when (ct.IsCancellationRequested)
                {
                    return Result<Component>.Fail("Operation was canceled.");
                }
                catch (Exception ex)
                {
                    return Result<Component>.Fail($"Failed to read image file: {ex.Message}");
                }
            }

            try
            {
                var added = await _components.AddAsync(component, ct);
                await _components.SaveAsync(ct);
                return Result<Component>.Success(added);
            }
            catch (Exception ex)
            {
                return Result<Component>.Fail($"Failed to create component: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves a single component by Id OR Code (repo decides precedence)
        public async Task<Result<Component>> GetComponentByIdOrCodeAsync(
            Guid? id = null,
            string? code = null,
            CancellationToken ct = default)
        {
            try
            {
                var component = await _components.GetByIdOrCodeAsync(id, code, ct);
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
        // Retrieves multiple components by Ids OR Codes (repo decides precedence)
        public async Task<Result<List<Component>>> GetComponentsListByIdOrCodeAsync(
            IEnumerable<Guid>? ids = null,
            IEnumerable<string>? codes = null,
            CancellationToken ct = default)
        {
            try
            {
                var filteredIds = ids?.Where(g => g != Guid.Empty);
                var filteredCodes = codes?.Where(s => !string.IsNullOrWhiteSpace(s));

                var list = await _components.GetListByIdOrCodeAsync(filteredIds, filteredCodes, ct);
                return Result<List<Component>>.Success(list);
            }
            catch (Exception ex)
            {
                return Result<List<Component>>.Fail($"Failed to retrieve components: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Retrieves full components list
        public async Task<Result<List<Component>>> GetComponentsListAsync(CancellationToken ct = default)
        {
            try
            {
                var list = await _components.GetListOrderedByCodeAsync(ct);
                return Result<List<Component>>.Success(list);
            }
            catch (Exception ex)
            {
                return Result<List<Component>>.Fail($"Failed to list components: {ex.Message}");
            }
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        // Searches components by term (name/code contains, case-insensitive)
        public async Task<Result<List<Component>>> SearchComponentsAsync(string? term, CancellationToken ct = default)
        {
            try
            {
                term ??= string.Empty;
                var list = await _components.SearchAsync(term, ct);
                return Result<List<Component>>.Success(list);
            }
            catch (Exception ex)
            {
                return Result<List<Component>>.Fail($"Failed to search components: {ex.Message}");
            }
        }
    }
}

//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\