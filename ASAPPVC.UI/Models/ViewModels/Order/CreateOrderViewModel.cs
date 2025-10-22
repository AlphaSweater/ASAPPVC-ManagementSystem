using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ASAPPVC.UI.Models.ViewModels.Order
{
    public class CreateOrderViewModel : IValidatableObject
    {
        [Required]
        public int CustomerId { get; set; }

        [Required]
        public List<ProductQuantityViewModel> ProductQuantities { get; set; } = new();

        [Required]
        public string OrderStatus { get; set; } = string.Empty;

        public DateTime? OrderDate { get; set; } = DateTime.UtcNow;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ProductQuantities == null || ProductQuantities.Count == 0)
            {
                yield return new ValidationResult("At least one product must be included.", new[] { nameof(ProductQuantities) });
            }

            for (int i = 0; i < ProductQuantities.Count; i++)
            {
                var pq = ProductQuantities[i];
                if (pq.ProductId <= 0)
                {
                    yield return new ValidationResult($"ProductId must be a positive integer.", new[] { $"ProductQuantities[{i}].ProductId" });
                }
                if (pq.Quantity < 1)
                {
                    yield return new ValidationResult($"Quantity for product at index {i} must be at least 1.", new[] { $"ProductQuantities[{i}].Quantity" });
                }
            }
        }
    }
}
