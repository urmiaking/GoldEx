using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.InventoryEntries;

namespace GoldEx.Server.Application.Validators.InventoryEntries;

[ScopedService]
internal class CreateInventoryEntryRequestValidator : AbstractValidator<CreateInventoryEntryRequest>
{
    public CreateInventoryEntryRequestValidator()
    {
        RuleForEach(x => x.Stocks)
            .NotEmpty()
            .WithMessage("لیست اجناس نمی تواند خالی باشد");

        // TODO: Add more validation rules as needed
    }
}