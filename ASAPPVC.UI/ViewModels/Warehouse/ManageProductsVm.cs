using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.ViewModels.Warehouse
{
    /// <summary>
    /// View model for the Manage Products view.
    /// Holds a list of lightweight product summary VMs and simple UI state (search, paging).
    /// </summary>
    public class ManageProductsVm
    {
        // List of products to render in the table (uses ProductListVm from Models)
        public List<ProductListVm> Products { get; set; } = new();

        // Optional search/filter text coming from the UI
        public string? SearchQuery { get; set; }

        // Simple paging state

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }

        // Convenience property for the view
        public bool HasProducts => Products != null && Products.Count > 0;

        // Parameterless constructor
        public ManageProductsVm()
        {
        }

        public ManageProductsVm(IEnumerable<ProductListVm>? products = null, string? searchQuery = null, int page = 1, int pageSize = 25, int totalCount = 0)
        {
            Products = products?.ToList() ?? new List<ProductListVm>();
            SearchQuery = searchQuery;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        // Very small helper factories for controller convenience
        public static ManageProductsVm Create()
        {
            return new ManageProductsVm();
        }

        public static ManageProductsVm Create(IEnumerable<ProductListVm>? products, string? searchQuery = null, int page = 1, int pageSize = 25, int totalCount = 0)
        {
            return new ManageProductsVm(products, searchQuery, page, pageSize, totalCount);
        }
    }
}