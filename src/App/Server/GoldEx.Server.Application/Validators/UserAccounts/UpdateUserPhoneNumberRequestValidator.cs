using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.UserAccounts;

namespace GoldEx.Server.Application.Validators.UserAccounts;

[ScopedService]
internal sealed class UpdateUserPhoneNumberRequestValidator : AbstractValidator<UpdateUserPhoneNumberRequest>
{
    public UpdateUserPhoneNumberRequestValidator()
    {
        RuleFor(x => x.NewPhoneNumber)
            .NotEmpty().WithMessage("شماره تماس الزامی است")
            .Matches(@"^09\d{9}$").WithMessage("شماره تلفن باید با 09 شروع شده و 11 رقم باشد");
    }
}