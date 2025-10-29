using ASAPPVC.App.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.App.Models
{
    //-----------------------------------------------\\
    //  ProductComponent ViewModel
    //-----------------------------------------------\\

    /// <summary>
    /// Unified view model for ProductComponent that works for both display and editing.<br/>
    /// When used in <see cref="ProductDetailVm"/>: shows read-only component info with computed costs.<br/>
    /// When used in <see cref="ProductFormVm"/>: binds editable quantity and removal flag.<br/>
    /// </summary>
    public sealed class ProductComponentVm : IValidatableObject
    {
        private const decimal MaxQuantity = 1_000_000_000_000m; // 1 trillion
        private const decimal MinQuantity = 0m;

        // Core identifiers
        public Guid ProductId { get; init; }

        [Required]
        public Guid ComponentId { get; set; }

        // Display fields
        public string ComponentCode { get; init; } = string.Empty;

        public string ComponentName { get; init; } = string.Empty;
        public Unit Unit { get; init; }
        public decimal UnitCost { get; init; }

        // Editable quantity
        [Required]
        [Range(typeof(decimal), "0.01", "999999", ErrorMessage = "Quantity must be greater than zero")]
        public decimal Quantity { get; set; } = 1m;

        // Edit-only flag: mark for removal during updates
        public bool Remove { get; set; }

        // Computed properties for display
        public decimal TotalCost => UnitCost * Quantity;

        public string ShortFormattedQuantity => Unit.ToDisplay(Quantity, shortForm: true);

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // ComponentId must not be empty
            if (ComponentId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "Component is required.",
                    new[] { nameof(ComponentId) });
            }

            // Quantity must be greater than 0
            if (Quantity <= MinQuantity)
            {
                yield return new ValidationResult(
                    $"Quantity must be greater than {MinQuantity:N0}.",
                    new[] { nameof(Quantity) });
            }

            // Quantity must be less than 1 trillion
            if (Quantity >= MaxQuantity)
            {
                yield return new ValidationResult(
                    $"Quantity must be less than {MaxQuantity:N0}.",
                    new[] { nameof(Quantity) });
            }
        }
    }
}