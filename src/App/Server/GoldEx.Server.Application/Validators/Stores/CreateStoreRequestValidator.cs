using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Stores;
using GoldEx.Shared.DTOs.Stores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Application.Validators.Stores;

[ScopedService]
public class CreateStoreRequestValidator : AbstractValidator<StoreRequest>
{
    private readonly IStoreRepository _storeRepository;
    private readonly IConfiguration _configuration;
    private readonly GoldExDbContext _dbContext;

    public CreateStoreRequestValidator(
        IStoreRepository storeRepository,
        IConfiguration configuration,
        GoldExDbContext dbContext)
    {
        _storeRepository = storeRepository;
        _configuration = configuration;
        _dbContext = dbContext;

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("نام فروشگاه نمی‌تواند خالی باشد.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("شناسه (Slug) فروشگاه نمی‌تواند خالی باشد.")
            .MustAsync(BeUniqueSlug).WithMessage("فروشگاهی با این شناسه (Slug) از قبل وجود دارد.");

        RuleFor(x => x)
            .MustAsync(NotExceedMaxStoresLimit)
            .WithMessage("تعداد فروشگاه‌های فعال بیش از حد مجاز لایسنس شماست.");
    }

    private async Task<bool> BeUniqueSlug(string slug, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return true;

        var slugExists = await _storeRepository.ExistsAsync(new StoreBySlugSpecification(slug), cancellationToken);
        return !slugExists;
    }

    private async Task<bool> NotExceedMaxStoresLimit(StoreRequest request, CancellationToken cancellationToken)
    {
        var licenseMode = _configuration["License:Mode"] ?? "Hybrid";
        if (licenseMode.Equals("InstanceWide", StringComparison.OrdinalIgnoreCase))
        {
            var maxStores = _configuration.GetValue<int>("License:MaxStores");
            if (maxStores <= 0) maxStores = 1;

            var activeStoresCount = await _dbContext.Set<Store>()
                .IgnoreQueryFilters()
                .CountAsync(x => x.IsActive, cancellationToken);

            return activeStoresCount < maxStores;
        }

        return true;
    }
}
