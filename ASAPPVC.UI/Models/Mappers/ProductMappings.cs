using ASAPPVC.UI.Models.ViewModels.Inventory.Product;

namespace ASAPPVC.UI.Models.Mappers
{
    public static class ProductMappings
    {
        public static ProductModel ToDomain(this CreateProductViewModel vm)
        {
            var product = new ProductModel
            {
                Name = vm.ProductName ?? string.Empty,
                Price = vm.BasePrice,
                Description = vm.Description ?? string.Empty,
            };

            if (vm.ImageFile is not null)
            {
                using var ms = new MemoryStream();
                vm.ImageFile.CopyToAsync(ms);
                product.ImageBytes = ms.ToArray();
                product.ImageContentType = vm.ImageFile.ContentType;
            }

            product.ProductComponents = vm.Components
                .Select(c => new ProductComponentModel { ComponentId = c.ComponentId, Quantity = c.Quantity })
                .ToList();

            return product;
        }

        public static ProductViewModel ToViewModel(this ProductModel model)
        {
            return new ProductViewModel
            {
                Id = model.Id,
                ProductCode = model.ProductCode,
                Name = model.Name,
                Price = model.Price,
                Description = model.Description,
                ImageContentType = model.ImageContentType,
                ImageBase64 = model.ImageBytes?.Length > 0
                    ? $"data:{model.ImageContentType};base64,{Convert.ToBase64String(model.ImageBytes)}"
                    : null,
                Components = model.ProductComponents?.Select(pc => new ProductComponentViewModel
                {
                    ComponentId = pc.ComponentId,
                    Quantity = pc.Quantity
                }).ToList() ?? new()
            };
        }

        // Map a single ProductComponentViewModel to ProductComponentModel.
        public static ProductComponentModel ToDomain(this ProductComponentViewModel vm, Guid productId = default)
        {
            ArgumentNullException.ThrowIfNull(vm);

            return new ProductComponentModel
            {
                ComponentId = vm.ComponentId,
                Quantity = vm.Quantity,
                ProductId = productId
            };
        }

        // Map a single ProductComponentModel to ProductComponentViewModel.
        public static ProductComponentViewModel ToViewModel(this ProductComponentModel model)
        {
            ArgumentNullException.ThrowIfNull(model);

            return new ProductComponentViewModel
            {
                ComponentId = model.ComponentId,
                Quantity = model.Quantity
            };
        }
    }
}
