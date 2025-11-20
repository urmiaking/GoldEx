using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class PriceUnitMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PriceUnit, GetPriceUnitResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.PriceId, src => src.PriceId.HasValue ? src.PriceId.Value.Value : (Guid?)null)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.PriceTitle, src => src.Price != null ? src.Price.Title : null)
            .Map(dest => dest.HasIcon,
                src => MapContext.Current.GetService<IWebHostEnvironment>().PriceUnitIconExists(src.Id.Value));

        config.NewConfig<PriceUnit, GetPriceUnitTitleResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.IsGoldBased,
                src => src.UnitType == UnitType.Gold18K || src.UnitType == UnitType.Mesghal)
            .Map(dest => dest.HasIcon,
                src => MapContext.Current.GetService<IWebHostEnvironment>().PriceUnitIconExists(src.Id.Value));
    }
}