using ASAPPVC.UI.Models;

namespace ASAPPVC.UI.ViewModels.Warehouse
{
    /// <summary>
    /// View model for the Manage Components view.
    /// Holds a list of lightweight component summary VMs and simple UI state (search, paging).
    /// </summary>
    public class ManageComponentsVm
    {
        // List of components to render in the table (uses ComponentListVm from Models)
        public List<ComponentListVm> Components { get; set; } = new();

        // Optional search/filter text coming from the UI
        public string? SearchQuery { get; set; }

        // Simple paging state

        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 25;
        public int TotalCount { get; set; }

        // Convenience property for the view
        public bool HasComponents => Components != null && Components.Count > 0;

        // Parameterless constructor
        public ManageComponentsVm()
        {
        }

        public ManageComponentsVm(IEnumerable<ComponentListVm>? components = null, string? searchQuery = null, int page = 1, int pageSize = 25, int totalCount = 0)
        {
            Components = components?.ToList() ?? new List<ComponentListVm>();
            SearchQuery = searchQuery;
            Page = page;
            PageSize = pageSize;
            TotalCount = totalCount;
        }

        // Small helper factories for controller convenience
        public static ManageComponentsVm Create()
        {
            return new ManageComponentsVm();
        }

        public static ManageComponentsVm Create(IEnumerable<ComponentListVm>? components, string? searchQuery = null, int page = 1, int pageSize = 25, int totalCount = 0)
        {
            return new ManageComponentsVm(components, searchQuery, page, pageSize, totalCount);
        }
    }
}