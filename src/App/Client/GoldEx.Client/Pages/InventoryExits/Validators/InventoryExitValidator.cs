using FluentValidation;
using GoldEx.Client.Pages.InventoryExits.ViewModels;
using GoldEx.Sdk.Common.Extensions;

namespace GoldEx.Client.Pages.InventoryExits.Validators;

public class InventoryExitValidator : AbstractValidator<InventoryExitVm>
{
    public InventoryExitValidator()
    {
        RuleFor(x => x.ExitDate)
            .NotNull().WithMessage("وارد کردن تاریخ خروج الزامی است")
            .LessThanOrEqualTo(DateTime.Now.GetDayEnd()).WithMessage("تاریخ خروج نمی تواند در آینده باشد");

        RuleFor(x => x.ExitReason)
            .NotNull().WithMessage("وارد کردن دلیل خروج الزامی است");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("طول توضیحات نمی تواند بیشتر از 500 کارکتر باشد");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<InventoryExitVm>.CreateWithOptions((InventoryExitVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}