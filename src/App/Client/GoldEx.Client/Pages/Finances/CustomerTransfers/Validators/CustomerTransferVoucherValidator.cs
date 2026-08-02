using FluentValidation;
using GoldEx.Client.Pages.Finances.CustomerTransfers.ViewModels;
using GoldEx.Sdk.Common.DependencyInjections;

namespace GoldEx.Client.Pages.Finances.CustomerTransfers.Validators;

[ScopedService]
public class CustomerTransferVoucherValidator : AbstractValidator<CustomerTransferVoucherVm>
{
    public CustomerTransferVoucherValidator()
    {
        RuleFor(x => x.TransferDate)
            .NotNull().WithMessage("تاریخ حواله الزامی است.");

        RuleFor(x => x.SourceCustomer)
            .NotNull().WithMessage("مشتری حواله‌دهنده الزامی است.");

        RuleFor(x => x.DestinationCustomer)
            .NotNull().WithMessage("مشتری دریافت‌کننده الزامی است.")
            .Must((model, dest) => dest == null || model.SourceCustomer == null || dest.Id != model.SourceCustomer.Id)
            .WithMessage("مشتری فرستنده و گیرنده نمی‌توانند یکسان باشند.");

        RuleFor(x => x.PriceUnit)
            .NotNull().WithMessage("واحد قیمت الزامی است.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("مبلغ یا وزن حواله باید بزرگتر از صفر باشد.");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CustomerTransferVoucherVm>.CreateWithOptions((CustomerTransferVoucherVm)model, x => x.IncludeProperties(propertyName)));
        if (result.IsValid)
            return Array.Empty<string>();
        return result.Errors.Select(e => e.ErrorMessage);
    };
}
