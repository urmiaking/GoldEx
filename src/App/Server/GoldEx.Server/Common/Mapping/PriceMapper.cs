using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Shared.DTOs.Prices;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class PriceMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Price, GetPriceResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.UnitType, src => src.PriceUnit != null ? src.PriceUnit!.UnitType : null)
            .Map(dest => dest.Value,
                src => src.PriceHistory != null ? src.PriceHistory.CurrentValue.ToString("N0") : "-")
            .Map(dest => dest.Change, src => src.PriceHistory != null ? src.PriceHistory.DailyChangeRate : "-")
            .Map(dest => dest.LastUpdate, src => src.PriceHistory != null ? src.PriceHistory.LastUpdate : null)
            .Map(dest => dest.Unit, src => src.PriceHistory != null ? src.PriceHistory.Unit : "-")
            .Map(dest => dest.Type, src => src.MarketType)
            .Map(dest => dest.PriceCatalog, src => src.PriceCatalog)
            .Map(dest => dest.HasIcon,
                src => MapContext.Current.GetService<IWebHostEnvironment>().PriceHistoryIconExists(src.Id.Value));

        config.NewConfig<Price, GetPriceTitleResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src => src.Title);
    }
}