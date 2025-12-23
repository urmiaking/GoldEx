using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.CoinInstances;

namespace GoldEx.Server.Application.Validators.Coins;

[ScopedService]
internal class DeleteCoinValidator : AbstractValidator<Guid>
{
    private readonly ICoinInstanceRepository _coinInstanceRepository;
    public DeleteCoinValidator(ICoinInstanceRepository coinInstanceRepository)
    {
        _coinInstanceRepository = coinInstanceRepository;

        RuleFor(x => x)
            .MustAsync(NotUsedInCoinInstances)
            .WithMessage("امکان حذف این سکه به دلیل مورد استفاده قرار گرفتن وجود ندارد.");
    }

    private async Task<bool> NotUsedInCoinInstances(Guid coinId, CancellationToken cancellationToken)
    {
        var exists = await _coinInstanceRepository.ExistsAsync(new CoinInstancesByCoinIdSpecification(new CoinId(coinId)), cancellationToken);
        return !exists;
    }
}