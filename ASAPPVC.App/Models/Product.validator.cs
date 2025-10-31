using ASAPPVC.App.Models.Validation;
using FluentValidation;

namespace ASAPPVC.App.Models
{
    public sealed class ProductFormVmValidator : BaseValidator<ProductFormVm>
    {
        public ProductFormVmValidator()
        {
            // ---------- Enums ----------
            RuleFor(vm => vm.Category).IsInEnum().WithMessage("Category is invalid.");
            RuleFor(vm => vm.MaterialType).IsInEnum().WithMessage("Material is invalid.");
            RuleFor(vm => vm.ColourOption).IsInEnum().WithMessage("Colour is invalid.");

            // ---------- ProductName (normalize + validate) ----------
            RuleFor(vm => vm.ProductName)
                .Custom((value, context) =>
                {
                    string? normalized = Normalize(value);
                    context.InstanceToValidate.ProductName = normalized ?? string.Empty;

                    if (string.IsNullOrEmpty(normalized))
                    {
                        context.AddFailure("Product name is required.");
                        return;
                    }
                    if (normalized.Length is < 2 or > 100)
                        context.AddFailure("Product name must be between 2 and 100 characters.");
                });

            // ---------- Description ----------
            RuleFor(vm => vm.Description)
                .Custom((value, context) =>
                {
                    string? normalized = Normalize(value);
                    context.InstanceToValidate.Description = normalized ?? string.Empty;

                    if (string.IsNullOrEmpty(normalized))
                    {
                        context.AddFailure("Description is required.");
                        return;
                    }
                    if (normalized.Length < 10)
                        context.AddFailure("Description must be at least 10 characters.");
                    if (normalized.Length > 500)
                        context.AddFailure("Description must be 500 characters or fewer.");
                });

            // ---------- SellingPrice ----------
            RuleFor(vm => vm.SellingPrice)
                .GreaterThan(0m).WithMessage("Selling Price must be a positive amount.")
                .LessThan(1_000_000_000m).WithMessage("Selling Price is unrealistically high.")
                .Must(v => decimal.Round(v, 2) == v)
                .WithMessage("Selling Price must have at most 2 decimal places.");

            // ---------- ReorderLevel ----------
            RuleFor(vm => vm.ReorderLevel)
                .GreaterThanOrEqualTo(0m).WithMessage("Reorder level cannot be negative.")
                .LessThan(MaxDecimal).WithMessage($"Reorder level must be less than {MaxDecimal:N0}.");

            // ---------- Components ----------
            RuleFor(vm => vm.SelectedProductComponents)
                .NotNull().WithMessage("Components are required.")
                .Must(list => list is { Count: > 0 })
                .WithMessage("A product requires at least one component.");

            RuleForEach(vm => vm.SelectedProductComponents).ChildRules(line =>
            {
                line.RuleFor(x => x.ComponentId)
                    .Must(id => id != Guid.Empty)
                    .WithMessage("Component is required.");

                line.RuleFor(x => x.RequiredQuantity)
                    .GreaterThan(0m).WithMessage("Quantity of Component must be greater than zero.");
            });

            // ---------- Edit-only (normalize ProductCode first) ----------
            RuleFor(vm => vm.ProductCode)
                .Custom((value, context) =>
                {
                    if (!context.InstanceToValidate.IsEdit)
                        return;

                    string? normalized = Normalize(value);
                    context.InstanceToValidate.ProductCode = normalized ?? string.Empty;

                    if (string.IsNullOrEmpty(normalized))
                    {
                        context.AddFailure("Product code is required when editing.");
                        return;
                    }
                    if (normalized.Length > 64)
                        context.AddFailure("Product code must be 64 characters or fewer.");
                });
        }
    }
}