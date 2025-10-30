namespace ASAPPVC.App.ViewModels.Reports
{
    public class PickingSlipViewModel
    {
        public string PickingSlipNumber { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime CreatedDateTime { get; set; }
        public string Priority { get; set; } = string.Empty;

        public FromInfo From { get; set; } = new();
        public ForInfo For { get; set; } = new();
        public OrderInfo Order { get; set; } = new();

        public List<string> LocationSummary { get; set; } = new();
        public List<ProductGroup> ProductGroups { get; set; } = new();

        public string SpecialInstructions { get; set; } = string.Empty;

        public int TotalLines { get; set; }
        public int TotalUnits { get; set; }
        public DateTime GeneratedDateTime { get; set; }

    }

    public class FromInfo
    {
        public string Company { get; set; } = string.Empty;
        public string Warehouse { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
    }

    public class ForInfo
    {
        public string Client { get; set; } = string.Empty;
        public string ProjectSite { get; set; } = string.Empty;
        public string Contact { get; set; } = string.Empty;
    }

    public class OrderInfo
    {
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime Created { get; set; }
        public DateTime PickBy { get; set; }
    }

    public class ProductGroup
    {
        public bool IsProduct { get; set; }
        public string? ProductCode { get; set; }
        public string ProductDescription { get; set; } = string.Empty;
        public int? RequiredSets { get; set; }
        public List<PickingItem> Items { get; set; } = new();
    }

    public class PickingItem
    {
        public string PartNumber { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Unit { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Location { get; set; } = string.Empty;
        public string? Notes { get; set; }
    }
}