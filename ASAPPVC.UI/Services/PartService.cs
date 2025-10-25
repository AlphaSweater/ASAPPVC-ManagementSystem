using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.ViewModels.Inventory;
using ASAPPVC.UI.Repositories;

namespace ASAPPVC.UI.Services
{
    public class PartService : IPartService
    {
        //─────────── Dependencies ───────────\\
        private readonly IPartRepository _repo;

        public PartService(IPartRepository repo)
        {
            _repo = repo;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //creates a new part in the database
        public async Task<(bool Ok, string? Error, PartModel? Part)> CreateAsync(CreatePartViewModel vm, CancellationToken ct = default)
        {
            //validates input and returns an error message if invalid
            if (string.IsNullOrWhiteSpace(vm.Name) || string.IsNullOrWhiteSpace(vm.StorageLocation))
                return (false, "Name and storage location are required.", null);

            var part = new PartModel
            {
                Name = vm.Name.Trim(),
                StorageLocation = vm.StorageLocation.Trim(),
                UnitCost = vm.UnitCost,
                CurrentAmount = vm.CurrentAmount
            };

            if (vm.ImageFile is { Length: > 0 })
            {
                using var ms = new MemoryStream();
                await vm.ImageFile.CopyToAsync(ms, ct);
                part.ImageBytes = ms.ToArray();
                part.ImageContentType = vm.ImageFile.ContentType;
            }

            await _repo.AddAsync(part, ct);
            await _repo.SaveAsync(ct);

            return (true, null, part);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //retrieves a single part by ID
        public async Task<PartModel?> GetAsync(int id, CancellationToken ct = default)
        {
            return await _repo.GetByIdAsync(id, ct);
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\
        //retrieves a list of parts from the database
        public async Task<List<PartModel>> ListAsync(CancellationToken ct = default)
        {
            return await _repo.ListAsync(ct);
        }
    }
}
//~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~EOF~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~\\