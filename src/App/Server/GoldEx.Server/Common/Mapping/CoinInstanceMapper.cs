using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Shared.DTOs.CoinInstances;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class CoinInstanceMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CoinInstance, GetCoinInstanceResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.CoinPackage, dest => dest.CoinInstancePackage);

        config.NewConfig<CoinInstancePackage, CoinPackageResponse>();
    }
}