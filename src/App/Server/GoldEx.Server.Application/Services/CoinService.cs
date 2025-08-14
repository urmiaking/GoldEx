using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Validators.Coins;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Coins;
using GoldEx.Shared.DTOs.Coins;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class CoinService(ICoinRepository repository,
    IMapper mapper,
    CoinRequestDtoValidator validator) : ICoinService
{
    public async Task<List<GetCoinResponse>> GetListAsync(bool? isActive, CancellationToken cancellationToken = default)
    {
        var items = await repository.Get(new CoinsByStatusSpecification(isActive)).ToListAsync(cancellationToken);

        return mapper.Map<List<GetCoinResponse>>(items);
    }

    public async Task<GetCoinResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new CoinsByIdSpecification(new CoinId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();
        
        return mapper.Map<GetCoinResponse>(item);
    }

    public async Task CreateAsync(CoinRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var coin = Coin.Create(request.Title, request.PriceId.HasValue ? new PriceId(request.PriceId.Value) : null);

        await repository.CreateAsync(coin, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, CoinRequestDto request, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(request, cancellationToken);

        var coin = await repository
            .Get(new CoinsByIdSpecification(new CoinId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        coin.SetTitle(request.Title);
        coin.SetPriceId(request.PriceId.HasValue ? new PriceId(request.PriceId.Value) : null);

        await repository.UpdateAsync(coin, cancellationToken);
    }

    public async Task SetStatusAsync(Guid id, bool isActive, CancellationToken cancellationToken = default)
    {
        var coin = await repository
            .Get(new CoinsByIdSpecification(new CoinId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        coin.SetStatus(isActive);

        await repository.UpdateAsync(coin, cancellationToken);
    }
}