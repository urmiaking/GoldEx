using GoldEx.Server.Domain.StoneTypeAggregate;
using GoldEx.Shared.DTOs.StoneTypes;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class StoneTypeMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<StoneType, GetStoneTypeResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);
    }
}
