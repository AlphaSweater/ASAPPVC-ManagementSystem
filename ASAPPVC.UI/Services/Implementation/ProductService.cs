using ASAPPVC.UI.Models;
using ASAPPVC.UI.Models.ViewModels.Inventory;
using ASAPPVC.UI.Repositories.Interfaces;
using ASAPPVC.UI.Services.Interfaces;

namespace ASAPPVC.UI.Services.Implementation
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;
        private readonly IPartRepository _partsRepository;

        public ProductService(IProductRepository repo, IPartRepository partsRepo)
        {
            _productRepository = repo;
            _partsRepository = partsRepo;
        }


        // creates a new product based on the provided view model
        public async Task<(bool Ok, string? Error, int? ProductId)> CreateAsync(CreateProductViewModel vm, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(vm.ProductName))
                return (false, "Product name is required.", null);
            if (string.IsNullOrWhiteSpace(vm.Description))
                return (false, "Description is required.", null);

            var lines = vm.Parts
                .Where(l => l.PartId.HasValue && l.PartId.Value > 0 && l.Quantity > 0)
                .ToList();

            var partIds = lines.Select(l => l.PartId!.Value).Distinct().ToList();
            var parts = partIds.Count > 0
                ? await _productRepository.GetPartsByIdsAsync(partIds, ct)
                : new List<PartModel>();

            decimal partsTotal = 0m;
            foreach (var l in lines)
            {
                var part = parts.FirstOrDefault(p => p.PartID == l.PartId);
                if (part != null) partsTotal += (part.UnitCost * l.Quantity);
            }
            var finalPrice = vm.BasePrice + partsTotal;

            var product = new ProductModel
            {
                Name = vm.ProductName.Trim(),
                Price = finalPrice,
                Description = vm.Description.Trim()
            };


            if (vm.ImageFile is { Length: > 0 })
            {
                using var ms = new MemoryStream();
                await vm.ImageFile.CopyToAsync(ms, ct);
                product.ImageBytes = ms.ToArray();
                product.ImageContentType = vm.ImageFile.ContentType;
            }

            product = await _productRepository.AddProductAsync(product, ct);
            await _productRepository.SaveAsync(ct);

            var ppLines = lines.Select(l => new ProductPartModel
            {
                ProductID = product.ProductID,
                PartID = l.PartId!.Value,
                Quantity = l.Quantity
            }).ToList();

            if (ppLines.Count > 0)
            {
                await _productRepository.AddProductPartsAsync(ppLines, ct);
                await _productRepository.SaveAsync(ct);
            }

            return (true, null, product.ProductID);
        }

        // retrieves a product by its ID, including its associated parts
        public Task<ProductModel?> GetAsync(int id, CancellationToken ct = default)
            => _productRepository.GetProductWithPartsAsync(id, ct);

        // lists all products
        public Task<List<ProductModel>> ListAsync(CancellationToken ct = default)
            => _productRepository.ListAsync(ct);
    }
}
