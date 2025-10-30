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

        // ===============================
        // Core Identification
        // ===============================

        public Guid ProductId { get; init; }

        [Required]
        public Guid ComponentId { get; set; }

        // ===============================
        // SnapShot of Component Info
        // ===============================

        public string ComponentCode { get; init; } = string.Empty;

        public string ComponentName { get; init; } = string.Empty;

        public decimal UnitCost { get; init; }

        // ===============================
        // Editable fields
        // ===============================

        // Editable quantity
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Quantity of Component must be greater than zero")]
        public decimal RequiredQuantity { get; set; } = 1m;

        public Unit UnitOfMeasure { get; init; }

        // ===============================
        // Edit-only values

        public bool Remove { get; set; }
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        // ------------------------------
        // Computed properties for display
        public decimal TotalCost => UnitCost * RequiredQuantity;

        public string ShortFormattedQuantity => UnitOfMeasure.ToDisplay(RequiredQuantity, shortForm: true);

        // ------------------------------
        // Validation
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
            if (RequiredQuantity <= MinQuantity)
            {
                yield return new ValidationResult(
                    $"Quantity must be greater than {MinQuantity:N0}.",
                    new[] { nameof(RequiredQuantity) });
            }

            // Quantity must be less than 1 trillion
            if (RequiredQuantity >= MaxQuantity)
            {
                yield return new ValidationResult(
                    $"Quantity must be less than {MaxQuantity:N0}.",
                    new[] { nameof(RequiredQuantity) });
            }
        }
    }
}