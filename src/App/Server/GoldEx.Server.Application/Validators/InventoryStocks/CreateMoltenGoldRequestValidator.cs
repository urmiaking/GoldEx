using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Validators.InventoryStocks;

[ScopedService]
internal class CreateMoltenGoldRequestValidator : AbstractValidator<(MeltingBatch MeltingBatch, string AssayNumber, decimal Fineness, decimal Weight)>
{
    public CreateMoltenGoldRequestValidator()
    {
        RuleFor(x => x.AssayNumber)
            .NotEmpty().WithMessage("شماره انگ الزامی است.")
            .MaximumLength(50).WithMessage("شماره انگ نباید بیشتر از 50 کاراکتر باشد.");

        RuleFor(x => x.Fineness)
            .InclusiveBetween(0, 1000).WithMessage("عیار باید بین 0 تا 1000 باشد.");

        RuleFor(x => x.Weight)
            .GreaterThan(0).WithMessage("وزن باید بزرگتر از صفر باشد.");

        RuleFor(x => x.MeltingBatch)
            .Must(x => x.CurrentStatus == MeltingBatchStatus.SentToLab)
            .WithMessage("وضعیت درخواست ذوب معتبر نمی باشد.");
    }
}