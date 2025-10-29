using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.App.Models
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
    // Upsert form (used for both Add and Edit order lines)
    //-----------------------------------------------\\
    /// <summary>
    /// Unified form view model used when adding or editing a product line in an order.
    /// If <see cref="OrderProductId"/> is null → Add; if it has value → Edit.
    /// </summary>
    public sealed class OrderProductFormVm
    {
        [Display(Name = "Product")]
        [Required(ErrorMessage = "Product is required.")]
        public Guid ProductId { get; set; }

        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 999999, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;

        // Optional: pre-fetched product data for display (not posted)
        public string? ProductName { get; init; }

        public decimal? UnitPrice { get; init; }

        // Mark for removal (for edit mode)
        public bool Remove { get; set; }
    }
}