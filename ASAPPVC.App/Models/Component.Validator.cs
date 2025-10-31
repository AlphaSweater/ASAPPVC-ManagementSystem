using ASAPPVC.App.Models.Validation;
using FluentValidation;
using System.Text.RegularExpressions;

namespace ASAPPVC.App.Models
{
    public sealed class ComponentFormVmValidator : BaseValidator<ComponentFormVm>
    {
        public ComponentFormVmValidator()
        {
            // ---------- Enums ----------
            RuleFor(vm => vm.UnitOfMeasure).IsInEnum().WithMessage("Unit of Measure is invalid.");
            RuleFor(vm => vm.MaterialType).IsInEnum().WithMessage("Material is invalid.");
            RuleFor(vm => vm.ColourOption).IsInEnum().WithMessage("Colour is invalid.");

            // ---------- ComponentName (normalize + validate) ----------
            RuleFor(vm => vm.ComponentName)
                .Custom((value, context) =>
                {
                    string? normalizedComponentName = Normalize(value);

                    context.InstanceToValidate.ComponentName = normalizedComponentName ?? string.Empty;

                    if (string.IsNullOrEmpty(normalizedComponentName))
                    {
                        context.AddFailure("Component name is required.");
                        return;
                    }
                    if (normalizedComponentName.Length is < 2 or > 100)
                        context.AddFailure("Component name must be between 2 and 100 characters.");
                });

            // ---------- QuantityOnHand ----------
            RuleFor(vm => vm.QuantityOnHand)
                .GreaterThanOrEqualTo(0m).WithMessage("Quantity on hand cannot be negative.")
                .LessThan(MaxDecimal).WithMessage($"Quantity on hand must be less than {MaxDecimal:N0}.")
                .Must((vm, quantity) => !IsIntegerOnlyUnit(vm.UnitOfMeasure) || quantity == decimal.Floor(quantity))
                .WithMessage("This unit does not allow fractional quantities.");

            // ---------- UnitCost ----------
            RuleFor(vm => vm.UnitCost)
                .GreaterThan(0m).WithMessage("Unit cost must be a positive amount.")
                .LessThan(1_000_000_000m);

            // ---------- LocationCode (normalize to UPPER + validate) ----------
            RuleFor(vm => vm.LocationCode)
                .Custom((value, context) =>
                {
                    string? normalizedLocationCode = Normalize(value, CaseMode.ToUpper);
                    context.InstanceToValidate.LocationCode = normalizedLocationCode ?? string.Empty;

                    if (string.IsNullOrEmpty(normalizedLocationCode))
                    {
                        context.AddFailure("Location code is required.");
                        return;
                    }
                    if (normalizedLocationCode.Length > 32)
                    {
                        context.AddFailure("Location code must be 32 characters or fewer.");
                        return;
                    }
                    if (!Regex.IsMatch(normalizedLocationCode, "^[A-Z0-9]+(?:-[A-Z0-9]+)*$"))
                        context.AddFailure("Use A–Z/0–9 with optional dashes, e.g. A-2 or B-12.");
                });

            // ---------- LocationNote (normalize only, still optional) ----------
            RuleFor(vm => vm.LocationNote)
                .Custom((value, context) =>
                {
                    string? normalizedLocationNote = Normalize(value); // trim or null
                    context.InstanceToValidate.LocationNote = normalizedLocationNote; // keep null allowed
                    if (normalizedLocationNote is { Length: > 100 })
                        context.AddFailure("Location note must be 100 characters or fewer.");
                });

            // ---------- Reorder Level ----------
            RuleFor(vm => vm.ReorderLevel)
                .GreaterThanOrEqualTo(0m).WithMessage("Reorder level cannot be negative.")
                .LessThan(MaxDecimal).WithMessage($"Reorder level must be less than {MaxDecimal:N0}.")
                .Must((vm, reorderLevel) => !IsIntegerOnlyUnit(vm.UnitOfMeasure) || reorderLevel == decimal.Floor(reorderLevel))
                .WithMessage("This unit does not allow fractional reorder levels.");

            // ---------- Edit-only (normalize code first) ----------
            RuleFor(vm => vm.ComponentCode)
                .Custom((value, context) =>
                {
                    if (!context.InstanceToValidate.IsEdit)
                        return;

                    string? normalizedComponentCode = Normalize(value);
                    context.InstanceToValidate.ComponentCode = normalizedComponentCode ?? string.Empty;

                    if (string.IsNullOrEmpty(normalizedComponentCode))
                    {
                        context.AddFailure("Component code is required when editing.");
                        return;
                    }
                    if (normalizedComponentCode.Length > 64)
                        context.AddFailure("Component code must be 64 characters or fewer.");
                });
        }
    }
}