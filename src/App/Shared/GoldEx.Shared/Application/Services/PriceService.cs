using FluentValidation;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Application.Validators;
using GoldEx.Shared.Domain.Aggregates.PriceAggregate;
using GoldEx.Shared.Domain.Aggregates.PriceHistoryAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Services;

public class PriceService<TPrice, TPriceHistory>(IPriceRepository<TPrice, TPriceHistory> repository, PriceValidator<TPrice, TPriceHistory> validator) 
    : IPriceService<TPrice, TPriceHistory> 
    where TPrice : PriceBase<TPriceHistory>
    where TPriceHistory : PriceHistoryBase
{
    public async Task CreateAsync(TPrice price, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(price, cancellationToken);
        await repository.CreateAsync(price, cancellationToken);
    }

    public async Task UpdateAsync(TPrice price, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(price, cancellationToken);
        await repository.UpdateAsync(price, cancellationToken);
    }

    public Task<List<TPrice>> GetLatestPricesAsync(CancellationToken cancellationToken = default)
        => repository.GetLatestPricesAsync(cancellationToken);

    public Task<List<TPrice>> GetListAsync(CancellationToken cancellationToken = default)
        => repository.GetListAsync(cancellationToken);
}