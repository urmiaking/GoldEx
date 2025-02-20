using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Shared.DTOs;
using Mapster;

namespace GoldEx.Server.Common;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        #region Price

        config.NewConfig<Price, GetPriceResponse>()
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Value,
                src => src.PriceHistories!.Any()
                    ? $"{src.PriceHistories!.First().CurrentValue:N0} {src.PriceHistories!.First().Unit}"
                    : "0")
            .Map(dest => dest.Change,
                src => src.PriceHistories!.Any() ? src.PriceHistories!.First().DailyChangeRate : string.Empty)
            .Map(dest => dest.LastUpdate,
                src => src.PriceHistories!.Any() ? src.PriceHistories!.First().LastUpdate : string.Empty)
            .Map(dest => dest.Type, src => src.MarketType)
            .Map(dest => dest.IconUrl, src => src.IconUrl);

        #endregion

    }
}
