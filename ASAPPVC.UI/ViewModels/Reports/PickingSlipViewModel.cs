namespace ASAPPVC.UI.ViewModels.Reports
{
    public class PickingSlipViewModel
    {
        public string PickingSlipNumber { get; set; } = string.Empty;
        public string OrderNumber { get; set; } = string.Empty;
        public DateTime CreatedDateTime { get; set; }
        public string Priority { get; set; } = string.Empty;
        public string LogoPath { get; set; } = string.Empty;

        public FromInfo From { get; set; } = new();
        public ForInfo For { get; set; } = new();
        public OrderInfo Order { get; set; } = new();

        public List<string> LocationSummary { get; set; } = new();
        public List<ProductGroup> ProductGroups { get; set; } = new();

        public string SpecialInstructions { get; set; } = string.Empty;

        public int TotalLines { get; set; }
        public int TotalUnits { get; set; }
        public DateTime GeneratedDateTime { get; set; }

        // Constructor with dummy data
        public PickingSlipViewModel() {
            PickingSlipNumber = "PS-000123";
            OrderNumber = "ORD-45782";
            CreatedDateTime = new DateTime(2025, 10, 22, 14, 6, 0);
            Priority = "Standard";
            LogoPath = "/Assets/Images/Logos/asap-pvc-logo.png";

            From = new FromInfo {
                Company = "ASAPPVC (Pty) Ltd",
                Warehouse = "Killarney Gardens",
                Contact = "info@asappvc.co.za"
            };

            For = new ForInfo {
                Client = "GreenTech Projects",
                ProjectSite = "Midrand Solar Farm",
                Contact = "Sipho Ndlovu • +27 82 555 0199"
            };

            Order = new OrderInfo {
                OrderNumber = "ORD-45782",
                Created = new DateTime(2025, 10, 22, 13, 52, 0),
                PickBy = new DateTime(2025, 10, 23, 10, 0, 0)
            };

            LocationSummary = new List<string>
            {
                "A1 / B04 ×5",
                "A1 / B08 ×2",
                "A3 / B02 ×4",
                "B2 / B12 ×6",
                "C1 / B01 ×3"
            };

            ProductGroups = new List<ProductGroup>
            {
                new ProductGroup
                {
                    IsProduct = true,
                    ProductCode = "SOL-KIT-2000",
                    ProductDescription = "2kW Solar Starter Kit",
                    RequiredSets = 2,
                    Items = new List<PickingItem>
                    {
                        new PickingItem
                        {
                            PartNumber = "PV-PANEL-330",
                            Description = "Photovoltaic Panel 330W (Mono)",
                            Unit = "ea",
                            Quantity = 4,
                            Location = "A1 / B04",
                            Notes = "Handle carefully"
                        },
                        new PickingItem
                        {
                            PartNumber = "INV-2000-HYB",
                            Description = "Hybrid Inverter 2kW",
                            Unit = "ea",
                            Quantity = 2,
                            Location = "A3 / B02"
                        },
                        new PickingItem
                        {
                            PartNumber = "CAB-PV-10M",
                            Description = "PV Cable 6mm² – 10m",
                            Unit = "roll",
                            Quantity = 2,
                            Location = "C1 / B01"
                        },
                        new PickingItem
                        {
                            PartNumber = "MC4-SET",
                            Description = "Connector Set (MC4 male + female)",
                            Unit = "set",
                            Quantity = 2,
                            Location = "A1 / B08"
                        }
                    }
                },
                new ProductGroup
                {
                    IsProduct = true,
                    ProductCode = "MNT-KIT-UNIV",
                    ProductDescription = "Universal Roof Mount Kit",
                    RequiredSets = 2,
                    Items = new List<PickingItem>
                    {
                        new PickingItem
                        {
                            PartNumber = "RAIL-1.8M",
                            Description = "Aluminium Rail 1.8m",
                            Unit = "ea",
                            Quantity = 8,
                            Location = "B2 / B12"
                        },
                        new PickingItem
                        {
                            PartNumber = "CLAMP-MID",
                            Description = "Mid Clamp Set",
                            Unit = "set",
                            Quantity = 6,
                            Location = "B2 / B12"
                        },
                        new PickingItem
                        {
                            PartNumber = "CLAMP-END",
                            Description = "End Clamp Set",
                            Unit = "set",
                            Quantity = 4,
                            Location = "B2 / B12"
                        }
                    }
                },
                new ProductGroup
                {
                    IsProduct = false,
                    ProductDescription = "Additional Items (Not part of kit)",
                    Items = new List<PickingItem>
                    {
                        new PickingItem
                        {
                            PartNumber = "TAPE-INS",
                            Description = "Insulation Tape (Black)",
                            Unit = "roll",
                            Quantity = 3,
                            Location = "C1 / B01"
                        }
                    }
                }
            };

            SpecialInstructions = "Check inverter firmware version ≥ 1.12. Ensure panels are palletized. Separate clamps by type in tote.";

            TotalLines = 9;
            TotalUnits = 31;
            GeneratedDateTime = new DateTime(2025, 10, 22, 14, 6, 0);
        }
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
