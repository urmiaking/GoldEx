using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class PriceService(IPriceRepository repository, IValidator<Price> validator) : IPriceService
{
    public async Task CreateAsync(Price price, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(price, cancellationToken);
        await repository.CreateAsync(price, cancellationToken);
    }

    public async Task UpdateAsync(Price price, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(price, cancellationToken);
        await repository.UpdateAsync(price, cancellationToken);
    }

    public Task<List<Price>> GetLatestPricesAsync(CancellationToken cancellationToken = default)
        => repository.GetLatestPricesAsync(cancellationToken);

    public Task<List<Price>> GetListAsync(CancellationToken cancellationToken = default)
        => repository.GetListAsync(cancellationToken);
}