using FluentValidation;
using GoldEx.Client.Pages.Customers.ViewModels;

namespace GoldEx.Client.Pages.Customers.Validators;

public class CustomerValidator : AbstractValidator<CustomerVm>
{
    public CustomerValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("لطفا نام مشتری را وارد کنید")
            .MaximumLength(100).WithMessage("حداکثر طول نام 100 کاراکتر می باشد");

        RuleFor(x => x.NationalId)
            .NotEmpty().WithMessage("لطفا کد یکتای مشتری را وارد کنید")
            .MaximumLength(25).WithMessage("حداکثر طول شناسه یکتا 25 کاراکتر می باشد");

        RuleFor(x => x.CustomerType).IsInEnum().WithMessage("لطفا نوع مشتری را انتخاب کنید");

        RuleFor(x => x.PhoneNumber)
            .MaximumLength(25).WithMessage("حداکثر طول تلفن تماس 25 کاراکتر می باشد");

        RuleFor(x => x.Address)
            .MaximumLength(200).WithMessage("حداکثر طول آدرس 200 کاراکتر می باشد");

        When(x => x.CreditLimitUnit.HasValue || x.CreditLimit.HasValue, () =>
        {
            RuleFor(x => x.CreditLimitUnit).NotEmpty().WithMessage("لطفا واحد را وارد کنید");
            RuleFor(x => x.CreditLimit).NotEmpty().WithMessage("لطفا سقف اعتبار را وارد کنید");
        });
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<CustomerVm>.CreateWithOptions((CustomerVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}