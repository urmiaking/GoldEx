using GoldEx.Client.Offline.Domain.CheckpointAggregate;
using GoldEx.Client.Offline.Domain.ProductAggregate;
using GoldEx.Client.Shared.DTOs;
using GoldEx.Shared.DTOs.Products;
using Mapster;

namespace GoldEx.Client.Common;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        #region Product

        config.NewConfig<Product, GetProductResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);

        config.NewConfig<Product, GetPendingProductResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Status, src => src.Status);

        #endregion

        #region Checkpoint

        config.NewConfig<Checkpoint, GetCheckPointResponse>()
            .Map(dest => dest.SyncDate, src => src.DateTime);

        #endregion
    }
}