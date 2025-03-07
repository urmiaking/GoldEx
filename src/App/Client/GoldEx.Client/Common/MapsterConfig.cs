﻿using GoldEx.Client.Offline.Domain.CheckpointAggregate;
using GoldEx.Client.Offline.Domain.PriceAggregate;
using GoldEx.Client.Offline.Domain.ProductAggregate;
using GoldEx.Client.Shared.DTOs;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.Products;
using Mapster;

namespace GoldEx.Client.Common;

public class MapsterConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        #region Price

        config.NewConfig<Price, GetPriceResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Value, src => src.PriceHistory.CurrentValue.ToString("N0"))
            .Map(dest => dest.Change, src => src.PriceHistory.DailyChangeRate)
            .Map(dest => dest.LastUpdate, src => src.PriceHistory.LastUpdate)
            .Map(dest => dest.Unit, src => src.PriceHistory.Unit)
            .Map(dest => dest.Type, src => src.MarketType)
            .Map(dest => dest.IconFileBase64, src => src.IconFile);

        #endregion

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