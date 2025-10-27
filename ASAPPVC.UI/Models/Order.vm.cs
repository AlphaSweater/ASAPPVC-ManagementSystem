using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models
{
    //-----------------------------------------------\\
    //  Order ViewModels (Read + Write)
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
    // Create form (used in POST / create order)
    //-----------------------------------------------\\
    /// <summary>
    /// Form view model used when creating a new order (POST).<br/>
    /// Requires a customer and at least one product line.<br/>
    /// </summary>
    public sealed class CreateOrderVm
    {
        [Display(Name = "Customer")]
        [Required(ErrorMessage = "Customer is required.")]
        public Guid CustomerId { get; set; }

        [Display(Name = "Order Date")]
        public DateTime? OrderDate { get; set; } // default to now if null in handler

        [Display(Name = "Order Status")]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        [Display(Name = "Products")]
        [MinLength(1, ErrorMessage = "An order requires at least 1 product line.")]
        public List<CreateOrderProductVm> Products { get; set; } = new();

        [Display(Name = "Notes (optional)")]
        [StringLength(500)]
        public string? Notes { get; set; }
    }

    //-----------------------------------------------\\
    // Edit form (used in PUT / update order)
    //-----------------------------------------------\\
    /// <summary>
    /// Form view model used when editing an existing order (PUT).<br/>
    /// Includes Id, mutable status/date, and editable product lines (with bridge Ids).<br/>
    /// </summary>
    public sealed class EditOrderVm
    {
        [Required(ErrorMessage = "Order ID is required.")]
        public Guid Id { get; set; }

        [Display(Name = "Order Code")]
        [Required(ErrorMessage = "Order code is required.")]
        [StringLength(64)]
        public string OrderCode { get; set; } = string.Empty;

        [Display(Name = "Customer")]
        [Required(ErrorMessage = "Customer is required.")]
        public Guid CustomerId { get; set; }

        [Display(Name = "Order Date")]
        [Required(ErrorMessage = "Order date is required.")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Order Status")]
        [Required(ErrorMessage = "Order status is required.")]
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Pending;

        [Display(Name = "Products")]
        [MinLength(1, ErrorMessage = "An order requires at least 1 product line.")]
        public List<EditOrderProductVm> Products { get; set; } = new();

        [Display(Name = "Notes (optional)")]
        [StringLength(500)]
        public string? Notes { get; set; }
    }
}