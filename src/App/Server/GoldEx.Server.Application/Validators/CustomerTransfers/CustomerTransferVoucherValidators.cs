using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.CustomerTransfers;

namespace GoldEx.Server.Application.Validators.CustomerTransfers;

[ScopedService]
internal class CreateCustomerTransferVoucherRequestValidator : AbstractValidator<CreateCustomerTransferVoucherRequest>
{
    public CreateCustomerTransferVoucherRequestValidator()
    {
        RuleFor(x => x.TransferDate)
            .NotEmpty().WithMessage("تاریخ حواله الزامی است.");

        RuleFor(x => x.SourceCustomerId)
            .NotEmpty().WithMessage("مشتری حواله‌دهنده الزامی است.");

        RuleFor(x => x.DestinationCustomerId)
            .NotEmpty().WithMessage("مشتری دریافت‌کننده الزامی است.")
            .Must((request, destId) => destId != request.SourceCustomerId)
            .WithMessage("مشتری فرستنده و گیرنده نمی‌توانند یکسان باشند.");

        RuleFor(x => x.PriceUnitId)
            .NotEmpty().WithMessage("واحد قیمت الزامی است.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("مبلغ یا وزن حواله باید بزرگتر از صفر باشد.");
    }
}

[ScopedService]
internal class UpdateCustomerTransferVoucherRequestValidator : AbstractValidator<UpdateCustomerTransferVoucherRequest>
{
    public UpdateCustomerTransferVoucherRequestValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("شناسه سند حواله الزامی است.");

        RuleFor(x => x.TransferDate)
            .NotEmpty().WithMessage("تاریخ حواله الزامی است.");

        RuleFor(x => x.SourceCustomerId)
            .NotEmpty().WithMessage("مشتری حواله‌دهنده الزامی است.");

        RuleFor(x => x.DestinationCustomerId)
            .NotEmpty().WithMessage("مشتری دریافت‌کننده الزامی است.")
            .Must((request, destId) => destId != request.SourceCustomerId)
            .WithMessage("مشتری فرستنده و گیرنده نمی‌توانند یکسان باشند.");

        RuleFor(x => x.PriceUnitId)
            .NotEmpty().WithMessage("واحد قیمت الزامی است.");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("مبلغ یا وزن حواله باید بزرگتر از صفر باشد.");
    }
}
