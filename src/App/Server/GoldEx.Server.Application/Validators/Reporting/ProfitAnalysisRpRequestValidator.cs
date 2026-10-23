using FluentValidation;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Validators.Reporting;

public class ProfitAnalysisRpRequestValidator : AbstractValidator<ProfitAnalysisRpRequest>
{
    public ProfitAnalysisRpRequestValidator()
    {
        RuleFor(x => x).Must(x => !x.FromDate.HasValue || !x.ToDate.HasValue ||
            x.ToDate.Value.Date >= x.FromDate.Value.Date)
            .WithMessage("تاریخ پایان نباید قبل از تاریخ شروع باشد.");
        RuleFor(x => x.ProductType).Must(x => x is null or ProductType.Gold or ProductType.Jewelry)
            .WithMessage("این گزارش برای طلای ساخته‌شده و جواهر است.");
    }
}
