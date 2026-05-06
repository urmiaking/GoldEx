using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.UserAccounts;

namespace GoldEx.Server.Application.Validators.UserAccounts;

[ScopedService]
internal sealed class UpdateUserFullNameRequestValidator : AbstractValidator<UpdateUserFullNameRequest>
{
    public UpdateUserFullNameRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام و نام خانوادگی نمی تواند خالی باشد")
            .MaximumLength(100).WithMessage("نام و نام خانوادگی نمی تواند بیشتر از 100 کاراکتر باشد");
    }
}