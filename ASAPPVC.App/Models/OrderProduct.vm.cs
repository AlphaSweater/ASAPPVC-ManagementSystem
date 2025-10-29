using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.App.Models
{
    //-----------------------------------------------\\
    //  OrderProduct ViewModel (Unified)
    //-----------------------------------------------\\

    /// <summary>
    /// Unified view model for OrderProduct that works for both display and editing.<br/>
    /// When used in <see cref="OrderDetailVm"/>: shows read-only product line info with computed costs.<br/>
    /// When used in <see cref="OrderFormVm"/>: binds editable quantity and removal flag.<br/>
    /// Eliminates the need for separate FormVm and display-only VM types.
    /// </summary>
    public sealed class OrderProductVm : IValidatableObject
    {
        // Core identifiers
        public Guid OrderId { get; init; }

        [Display(Name = "Product")]
        [Required(ErrorMessage = "Product is required.")]
        public Guid ProductId { get; set; }

        // Display fields (populated from navigation properties)
        public string ProductCode { get; init; } = string.Empty;
        public string ProductName { get; init; } = string.Empty;
        public decimal UnitPrice { get; init; }

        // Editable quantity (used in both display and forms)
        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity is required.")]
        [Range(1, 999999, ErrorMessage = "Quantity must be at least 1.")]
        public int Quantity { get; set; } = 1;

        // Edit-only flag: mark for removal during updates
        public bool Remove { get; set; }

        // Computed property for display
        /// <summary>Total cost for this line item (UnitPrice × Quantity).</summary>
        public decimal TotalCost => UnitPrice * Quantity;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // ProductId must not be empty
            if (ProductId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "Product is required.",
                    new[] { nameof(ProductId) });
            }

            // Quantity must be at least 1
            if (Quantity < 1)
            {
                yield return new ValidationResult(
                    "Quantity must be at least 1.",
                    new[] { nameof(Quantity) });
            }
        }
    }
}