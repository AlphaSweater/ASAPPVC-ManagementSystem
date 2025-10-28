using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models
{
    //-----------------------------------------------\\
    //  OrderProduct ViewModels (Read + Write)
    //-----------------------------------------------\\

    //-----------------------------------------------\\
    // Product inside an order (read-only view)
    //-----------------------------------------------\\
    /// <summary>
    /// Read-only representation of a product line inside an order detail view.<br/>
    /// Contains display-friendly product info and computed totals for UI presentation.<br/>
    /// Use this VM only for display purposes inside an <see cref="OrderDetailVm"/>; not for form binding.<br/>
    /// </summary>
    public sealed class OrderProductVm
    {
        public Guid Id { get; init; }
        public Guid OrderId { get; init; }
        public Guid ProductId { get; init; }

        public string ProductCode { get; init; } = string.Empty;
        public string ProductName { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }
        public int Quantity { get; init; }

        /// <summary>Total cost for this line item (UnitPrice × Quantity).</summary>
        public decimal TotalCost => UnitPrice * Quantity;
    }

    //-----------------------------------------------\\
    // Create form (used in POST / create order)
    //-----------------------------------------------\\
    /// <summary>
    /// Product line entry used when creating an order. Represents the selected product and quantity.<br/>
    /// Use as items of <see cref="CreateOrderVm.Products"/> when submitting a new order.<br/>
    /// </summary>
    public sealed class CreateOrderProductVm
    {
        [Display(Name = "Product")]
        [Required(ErrorMessage = "Product is required.")]
        public Guid ProductId { get; set; }

        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 999999, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;
    }

    //-----------------------------------------------\\
    // Edit form (used in PUT / update order)
    //-----------------------------------------------\\
    /// <summary>
    /// Product line entry used when editing an order.<br/>
    /// Includes the bridge entity Id for persistence tracking.<br/>
    /// Use as items of <see cref="EditOrderVm.Products"/> when updating an existing order.<br/>
    /// </summary>
    public sealed class EditOrderProductVm
    {
        /// <summary>Existing bridge entity Id if persisted (null for newly added lines).</summary>
        public Guid? OrderProductId { get; set; }

        [Display(Name = "Product")]
        [Required(ErrorMessage = "Product is required.")]
        public Guid ProductId { get; set; }

        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 999999, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;
    }
}