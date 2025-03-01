using GoldEx.Client.Offline.Domain.ProductAggregate;
using GoldEx.Shared.DTOs.Products;
using Mapster;

namespace GoldEx.Client.Offline.Common;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Product, GetProductResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);
    }
}