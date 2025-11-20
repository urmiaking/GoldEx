using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Shared.DTOs.Coins;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class CoinMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Coin, GetCoinResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.PriceId, src => src.PriceId.HasValue ? src.PriceId.Value.Value : (Guid?)null);
    }
}