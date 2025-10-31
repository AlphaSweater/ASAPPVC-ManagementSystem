using ASAPPVC.App.Models.Validation;
using FluentValidation;
using System.Text.RegularExpressions;

namespace ASAPPVC.App.Models
{
    public sealed class OrderFormVmValidator : BaseValidator<OrderFormVm>
    {
        public OrderFormVmValidator()
        {
            // ---------- Enums ----------
            RuleFor(vm => vm.OrderStatus).IsInEnum().WithMessage("Order status is invalid.");

            // ---------- Customer ----------
            RuleFor(vm => vm.CustomerId)
                .Must(id => id != Guid.Empty)
                .WithMessage("Customer is required.");

            // ---------- OrderDate ----------
            RuleFor(vm => vm.OrderDate)
                .Must(d => d == null || d.Value >= new DateTime(2000, 1, 1))
                    .WithMessage("Order date is unrealistically old.")
                .Must(d => d == null || d.Value <= DateTime.UtcNow.AddDays(1))
                    .WithMessage("Order date cannot be far in the future.");

            // ---------- Notes ----------
            RuleFor(vm => vm.Notes)
                .Custom((value, context) =>
                {
                    var normalized = Normalize(value);
                    context.InstanceToValidate.Notes = normalized;
                    if (normalized is { Length: > 500 })
                        context.AddFailure("Notes must be 500 characters or fewer.");
                });

            // ---------- Products ----------
            RuleFor(vm => vm.Products)
                .NotNull().WithMessage("Products are required.")
                .Must(list => list is { Count: > 0 })
                .WithMessage("An order requires at least 1 product line.");

            // ---------- Edit-only ----------
            RuleFor(vm => vm.OrderCode)
                .Custom((value, context) =>
                {
                    if (!context.InstanceToValidate.IsEdit)
                        return;

                    string? normalized = Normalize(value);
                    context.InstanceToValidate.OrderCode = normalized ?? string.Empty;

                    if (string.IsNullOrEmpty(normalized))
                    {
                        context.AddFailure("Order code is required when editing.");
                        return;
                    }
                    if (normalized.Length > 64)
                        context.AddFailure("Order code must be 64 characters or fewer.");
                });
        }
    }
}
