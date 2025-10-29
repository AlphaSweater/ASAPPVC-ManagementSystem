using ASAPPVC.App.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.App.Models
{
    //-----------------------------------------------\\
    // Order ViewModels (Read + Write)
    //-----------------------------------------------\\

    //-----------------------------------------------\\
    // Summary (used in order lists, search results)
    //-----------------------------------------------\\
    /// <summary>
    /// Lightweight summary view model used in order lists and search results.<br/>
    /// Includes quick display fields and computed totals for table/card views.<br/>
    /// </summary>
    public sealed class OrderListVm
    {
        public Guid Id { get; init; }
        public string OrderCode { get; init; } = string.Empty;

        // Customer display (populate in mapper from navigation)
        public Guid CustomerId { get; init; }

        public string CustomerName { get; init; } = string.Empty;

        public DateTime OrderDate { get; init; }
        public OrderStatus OrderStatus { get; init; } = OrderStatus.Pending;

        // Aggregates (populate in mapper)
        public int ItemCount { get; init; }

        public decimal TotalAmount { get; init; }

        // UI helpers
        public string DisplayDate => OrderDate.ToString("yyyy-MM-dd HH:mm");

        public string ItemsBadge => $"{ItemCount} item{(ItemCount == 1 ? "" : "s")}";
        public string DisplayTotal => TotalAmount.ToString("C");
    }

    //-----------------------------------------------\\
    // Detail (used for view screen)
    //-----------------------------------------------\\
    /// <summary>
    /// Detailed order view for order detail pages/modals.<br/>
    /// Includes customer display info and read-only product lines.<br/>
    /// Not intended for form binding.<br/>
    /// </summary>
    public sealed class OrderDetailVm
    {
        public Guid Id { get; init; }
        public string OrderCode { get; init; } = string.Empty;

        // Customer (map from navigation)
        public Guid CustomerId { get; init; }

        public string CustomerName { get; init; } = string.Empty;
        public string? CustomerEmail { get; init; } // optional if available

        public DateTime OrderDate { get; init; }
        public OrderStatus OrderStatus { get; init; } = OrderStatus.Pending;

        // Line items
        public List<OrderProductVm> Products { get; init; } = new();

        // Totals (compute in mapper/service)
        public int ItemCount { get; init; }

        public decimal Subtotal { get; init; }
        public decimal TaxAmount { get; init; }           // if you apply VAT later
        public decimal GrandTotal { get; init; }

        // UI helpers
        public string DisplayDate => OrderDate.ToString("yyyy-MM-dd HH:mm");

        public string DisplaySubtotal => Subtotal.ToString("C");
        public string DisplayTax => TaxAmount.ToString("C");
        public string DisplayGrandTotal => GrandTotal.ToString("C");
    }

    //-----------------------------------------------\\
    // Upsert form (used for both Create and Edit order)
    //-----------------------------------------------\\
    /// <summary>
    /// One form VM for both Create (Add) and Edit (Upsert).
    /// If Id is null → Create; if Id has value → Edit.
    /// Implements custom validation for edit-mode requirements and line validation.
    /// </summary>
    public sealed class OrderFormVm : IValidatableObject
    {
        // Mode
        public Guid? Id { get; set; }

        public bool IsEdit => Id.HasValue;

        [Display(Name = "Order Code")]
        [StringLength(64)]
        public string? OrderCode { get; set; }

        [Display(Name = "Customer")]
        [Required(ErrorMessage = "Customer is required.")]
        public Guid CustomerId { get; set; }

        [Display(Name = "Order Date")]
        public DateTime? OrderDate { get; set; } // default to now if null in handler

        [Display(Name = "Order Status")]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        [Display(Name = "Products")]
        [MinLength(1, ErrorMessage = "An order requires at least 1 product line.")]
        public List<OrderProductVm> Products { get; set; } = new();

        [Display(Name = "Notes (optional)")]
        [StringLength(500)]
        public string? Notes { get; set; }

        // Lookup collections for form UI
        // Holds lightweight product list view models that can be added to the order
        public List<ProductListVm> AvailableProducts { get; set; } = new();

        // Holds customers for selection in the form
        public List<Customer> AvailableCustomers { get; set; } = new();

        // Parameterless constructor (kept for model binding)
        public OrderFormVm()
        { }

        // Convenience constructor to initialize lookup collections and sensible defaults
        public OrderFormVm(IEnumerable<ProductListVm>? availableProducts, IEnumerable<Customer>? availableCustomers)
        {
            AvailableProducts = availableProducts?.ToList() ?? new List<ProductListVm>();
            AvailableCustomers = availableCustomers?.ToList() ?? new List<Customer>();
            OrderDate = DateTime.Now;
            Products = new List<OrderProductVm>();
        }

        // Static factory for creating a new form pre-populated with lookups
        public static OrderFormVm CreateNew(IEnumerable<ProductListVm>? availableProducts = null, IEnumerable<Customer>? availableCustomers = null)
        {
            return new OrderFormVm(availableProducts, availableCustomers)
            {
                Id = null,
                OrderStatus = OrderStatus.Pending
            };
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (IsEdit && string.IsNullOrWhiteSpace(OrderCode))
            {
                yield return new ValidationResult(
                    "Order code is required when editing.",
                    new[] { nameof(OrderCode) });
            }

            if (Products is { Count: > 0 })
            {
                for (int i = 0; i < Products.Count; i++)
                {
                    var p = Products[i];
                }
            }
        }
    }
}