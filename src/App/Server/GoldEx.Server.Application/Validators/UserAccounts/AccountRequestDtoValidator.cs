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
            .MustAsync(BeUniqueUsername).WithMessage("نام کاربری الزامی است");

        RuleFor(x => x.PhoneNumber)
            .Length(11).WithMessage("طول شماره تماس باید 11 کاراکتر باشد")
            .MustAsync(BeUniquePhoneNumber).WithMessage("شماره تماس وارد شده تکراری است");

        RuleFor(x => x.Password)
            .Must(NotEmptyInCreateMode).WithMessage("رمز عبور الزامی است")
            .MinimumLength(4).WithMessage("طول رمز عبور نمی تواند کمتر از 4 کاراکتر باشد");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("فرمت ایمیل وارد شده اشتباه است")
            .MustAsync(BeUniqueEmail).WithMessage("شماره تماس وارد شده تکراری است");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("نقش کاربر الزامی است");
    }

    private async Task<bool> BeUniqueEmail(UserAccountRequestDto request, string email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(email))
            return true;

        var user = await _userManager.FindByEmailAsync(request.Email!);

        return user == null || user.Email?.ToLower() == request.Email?.ToLower();
    }

    private async Task<bool> BeUniquePhoneNumber(UserAccountRequestDto request, string phoneNumber, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(phoneNumber))
            return true;

        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber, cancellationToken);

        return user == null || user.PhoneNumber == request.PhoneNumber;
    }

    private async Task<bool> BeUniqueUsername(UserAccountRequestDto request, string username, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(username))
            return true;

        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == request.Username, cancellationToken);

        return user == null || user.UserName == request.Username;
    }

    private static bool NotEmptyInCreateMode(UserAccountRequestDto request, string? password) =>
        request.Id.HasValue || !string.IsNullOrEmpty(password);
}