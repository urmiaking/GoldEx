using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class InventoryStockMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InventoryStock, GetInventoryStockResponse>()
            .Map(dest => dest.CurrentAmount, src => src.ChangeAmount);

        config.NewConfig<InventorySummaryData, GetInventoryStockResponse>()
            .Map(dest => dest.CurrentAmount, src => src.CurrentAmount)
            .Map(dest => dest.SoldAmount, src => src.SoldAmount)
            .Map(dest => dest.Coin, src => src.CoinInstance)
            .Map(dest => dest.DateTime, src => src.DateTime);

        config.NewConfig<InventoryWeightChartData, GetInventoryWeightChartResponse>();

        config.NewConfig<InventoryStock, GetInventoryStockItemResponse>()
            .Map(dest => dest.Amount, src => src.ChangeAmount)
            .Map(dest => dest.DateTime, src => src.PostingDate)
            .Map(dest => dest.ActionType, src => src.ActionType)
            .Map(dest => dest.GoldUnitType, src =>
                src.Product != null ? src.Product.GoldUnitType : (GoldUnitType?)null)
            .Map(dest => dest.PriceUnit, src => src.Currency != null ? src.Currency.Title : null)
            .Map(dest => dest.Description, src => InventoryStockDescriptionBuilder.Build(src, false));

        config.NewConfig<InventoryStock, GetInventoryStockTraceResponse>()
            .Map(dest => dest.Amount, src => src.ChangeAmount)
            .Map(dest => dest.DateTime, src => src.PostingDate)
            .Map(dest => dest.ActionType, src => src.ActionType)
            .Map(dest => dest.GoldUnitType, src =>
                src.Product != null ? src.Product.GoldUnitType : (GoldUnitType?)null)
            .Map(dest => dest.PriceUnit, src => src.Currency != null ? src.Currency.Title : null)
            .Map(dest => dest.Description, src => InventoryStockDescriptionBuilder.Build(src, true))
            .Map(dest => dest.SourceUrl, src => InventoryStockDescriptionBuilder.BuildUrl(src));
    }
}