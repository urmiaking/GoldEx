using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Stores;
using GoldEx.Shared.DTOs.Stores;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Validators.Stores;

[ScopedService]
public class UpdateStoreRequestValidator : AbstractValidator<(Guid Id, StoreRequest Request)>
{
    private readonly IStoreRepository _storeRepository;

    public UpdateStoreRequestValidator(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;

        RuleFor(x => x.Request.Name)
            .NotEmpty().WithMessage("نام فروشگاه نمی‌تواند خالی باشد.");

        RuleFor(x => x.Request.Slug)
            .NotEmpty().WithMessage("شناسه (Slug) فروشگاه نمی‌تواند خالی باشد.")
            .MustAsync(BeUniqueSlug).WithMessage("فروشگاهی با این شناسه (Slug) از قبل وجود دارد.");
    }

    private async Task<bool> BeUniqueSlug((Guid Id, StoreRequest Request) tuple, string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return true;

        var otherStoreWithSlug = await _storeRepository.Get(new StoreBySlugSpecification(slug))
            .FirstOrDefaultAsync(cancellationToken);

        if (otherStoreWithSlug == null)
            return true;

        return otherStoreWithSlug.Id.Value == tuple.Id;
    }
}
