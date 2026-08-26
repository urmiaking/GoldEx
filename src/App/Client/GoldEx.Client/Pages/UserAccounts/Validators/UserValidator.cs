using FluentValidation;
using GoldEx.Client.Pages.UserAccounts.ViewModels;

namespace GoldEx.Client.Pages.UserAccounts.Validators;

public class UserValidator : AbstractValidator<UserEditorVm>
{
    public UserValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام و نام خانوادگی الزامی است");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("نام کاربری الزامی است");

        RuleFor(x => x.PhoneNumber)
            .Length(11).When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber)).WithMessage("طول شماره تماس باید 11 کاراکتر باشد");

        RuleFor(x => x.Password)
            .NotEmpty().When(x => !x.Id.HasValue).WithMessage("رمز عبور الزامی است")
            .MinimumLength(4).When(x => !string.IsNullOrEmpty(x.Password)).WithMessage("طول رمز عبور نمی تواند کمتر از 4 کاراکتر باشد");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("فرمت ایمیل وارد شده اشتباه است");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("نقش کاربر الزامی است");
    }

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<UserEditorVm>.CreateWithOptions((UserEditorVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}