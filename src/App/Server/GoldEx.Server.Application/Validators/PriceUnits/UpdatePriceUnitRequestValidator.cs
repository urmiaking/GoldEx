using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.PriceUnits;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.PriceUnits;

[ScopedService]
internal class UpdatePriceUnitRequestValidator : AbstractValidator<(Guid id, UpdatePriceUnitRequest request)>
{
    private readonly IPriceUnitRepository _repository;
    public UpdatePriceUnitRequestValidator(IPriceUnitRepository repository)
    {
        _repository = repository;
        RuleFor(x => x.request.Title)
            .NotEmpty()
            .WithMessage("عنوان نمی تواند خالی باشد")
            .MaximumLength(100)
            .WithMessage("عنوان نمی تواند بیشتر از 100 کاراکتر باشد")
            .MustAsync(BeUniqueTitle)
            .WithMessage("عنوان نمیتواند تکراری باشد");
    }

    private async Task<bool> BeUniqueTitle((Guid id, UpdatePriceUnitRequest request) request, string title, CancellationToken cancellationToken = default)
    {
        var item = await _repository
            .Get(new PriceUnitsByTitleSpecification(title))
            .FirstOrDefaultAsync(cancellationToken);

        if (item is null)
            return true;

        return item.Id.Value == request.id;
    }
}