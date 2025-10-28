using ASAPPVC.UI.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace ASAPPVC.UI.Models
{
    //-----------------------------------------------\\
    //  ProductComponent ViewModels (Read + Write)
    //-----------------------------------------------\\

    //-----------------------------------------------\\
    // Component inside product (read-only view)
    //-----------------------------------------------\\
    /// <summary>
    /// Read-only representation of a product component used inside product detail views.<br/>
    /// Contains display fields and computed values useful for UI presentation.<br/>
    /// Use this VM only for display purposes inside a ProductDetailVm; it is not intended<br/>
    /// for create/edit form binding.<br/>
    /// </summary>
    public sealed class ProductComponentVm
    {
        public Guid ProductId { get; init; }
        public Guid ComponentId { get; init; }

        public string ComponentCode { get; init; } = string.Empty;
        public string ComponentName { get; init; } = string.Empty;

        public Unit Unit { get; init; }
        public decimal QuantityRequired { get; init; }

        public decimal UnitCost { get; init; }
        public decimal TotalCost => UnitCost * QuantityRequired;

        public string ShortFormattedQuantity => Unit.ToDisplay(QuantityRequired, shortForm: true);
    }

    //-----------------------------------------------\\
    // Create form (used in POST / add product)
    //-----------------------------------------------\\
    public sealed class ProductComponentFormVm : IValidatableObject
    {
        private const decimal MaxQuantity = 1_000_000_000_000m; // 1 trillion
        private const decimal MinQuantity = 0m;

        [Required]
        public Guid ComponentId { get; set; }

        // Use decimal for consistency with Unit (supports fractional units)
        public decimal Quantity { get; set; } = 1m;

        // Mark a persisted line for deletion on edit
        public bool Remove { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // 1) ComponentId must not be empty
            if (ComponentId == Guid.Empty)
            {
                yield return new ValidationResult(
                    "Component is required.",
                    new[] { nameof(ComponentId) });
            }

            // 2) Quantity must be greater than 0
            if (Quantity <= MinQuantity)
            {
                yield return new ValidationResult(
                    $"Quantity must be greater than {MinQuantity:N0}.",
                    new[] { nameof(Quantity) });
            }

            // 3) Quantity must be less than 1 trillion
            if (Quantity >= MaxQuantity)
            {
                yield return new ValidationResult(
                    $"Quantity must be less than {MaxQuantity:N0}.",
                    new[] { nameof(Quantity) });
            }
        }
    }
}