using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.InventoryEntries;

namespace GoldEx.Server.Application.Validators.InventoryEntries;

[ScopedService]
internal class CreateInventoryEntryRequestValidator : AbstractValidator<CreateInventoryEntryRequest>
{
    public CreateInventoryEntryRequestValidator()
    {
        RuleFor(x => x)
            .Must(AtLeastContainOneItem)
            .WithMessage("باید حداقل یک قلم کالا، سکه یا ارز وجود داشته باشد.");

        // TODO: Add more validation rules as needed
    }

    private bool AtLeastContainOneItem(CreateInventoryEntryRequest request)
    {
        return request.Products.Count > 0 ||
               request.Coins.Count > 0 ||
               request.Currencies.Count > 0;
    }
}