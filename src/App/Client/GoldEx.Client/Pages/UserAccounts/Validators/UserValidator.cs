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
            .Length(11).WithMessage("طول شماره تماس باید 11 کاراکتر باشد");

        RuleFor(x => x.Password)
            .Must(NotEmptyInCreateMode).WithMessage("رمز عبور الزامی است")
            .MinimumLength(4).WithMessage("طول رمز عبور نمی تواند کمتر از 4 کاراکتر باشد");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("فرمت ایمیل وارد شده اشتباه است");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("نقش کاربر الزامی است");
    }

    private static bool NotEmptyInCreateMode(UserEditorVm vm, string? password) => 
        vm.Id.HasValue || !string.IsNullOrEmpty(password);

    public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
    {
        var result = await ValidateAsync(ValidationContext<UserEditorVm>.CreateWithOptions((UserEditorVm)model,
            x => x.IncludeProperties(propertyName)));
        return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
    };
}