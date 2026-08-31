using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Shared.DTOs.UserAccounts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.UserAccounts;

[ScopedService]
internal sealed class AccountRequestDtoValidator : AbstractValidator<UserAccountRequestDto>
{
    private readonly UserManager<AppUser> _userManager;

    public AccountRequestDtoValidator(UserManager<AppUser> userManager)
    {
        _userManager = userManager;

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("نام و نام خانوادگی الزامی است");

        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("نام کاربری الزامی است")
            .MustAsync(BeUniqueUsername).WithMessage("نام کاربری وارد شده تکراری است");

        RuleFor(x => x.PhoneNumber)
            .Length(11).When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber)).WithMessage("طول شماره تماس باید 11 کاراکتر باشد")
            .MustAsync(BeUniquePhoneNumber).When(x => !string.IsNullOrWhiteSpace(x.PhoneNumber)).WithMessage("شماره تماس وارد شده تکراری است");

        RuleFor(x => x.Password)
            .NotEmpty().When(x => !x.Id.HasValue).WithMessage("رمز عبور الزامی است")
            .MinimumLength(4).When(x => !string.IsNullOrEmpty(x.Password)).WithMessage("طول رمز عبور نمی تواند کمتر از 4 کاراکتر باشد");

        RuleFor(x => x.Email)
            .EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("فرمت ایمیل وارد شده اشتباه است")
            .MustAsync(BeUniqueEmail).When(x => !string.IsNullOrWhiteSpace(x.Email)).WithMessage("ایمیل وارد شده تکراری است");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("نقش کاربر الزامی است");
    }

    private async Task<bool> BeUniqueEmail(UserAccountRequestDto request, string? email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email))
            return true;

        var user = await _userManager.FindByEmailAsync(email);

        return user == null || (request.Id.HasValue && user.Id == request.Id.Value);
    }

    private async Task<bool> BeUniquePhoneNumber(UserAccountRequestDto request, string? phoneNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return true;

        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == phoneNumber, cancellationToken);

        return user == null || (request.Id.HasValue && user.Id == request.Id.Value);
    }

    private async Task<bool> BeUniqueUsername(UserAccountRequestDto request, string? username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(username))
            return true;

        var user = await _userManager.FindByNameAsync(username);

        return user == null || (request.Id.HasValue && user.Id == request.Id.Value);
    }
}