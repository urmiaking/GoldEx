using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

public class InventoryStocksMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InventoryStock, GetInventoryStockResponse>()
            .Map(dest => dest.CurrentAmount, src => src.ChangeAmount);
            
        config.NewConfig<InventorySummaryData, GetInventoryStockResponse>()
            .Map(dest => dest.CurrentAmount, src => src.CurrentAmount)
            .Map(dest => dest.SoldAmount, src => src.SoldAmount)
            .Map(dest => dest.DateTime, src => src.DateTime);

        config.NewConfig<InventoryWeightChartData, GetInventoryWeightChartResponse>();

        config.NewConfig<InventoryStock, GetInventoryStockItemResponse>()
            .Map(dest => dest.Amount, src => src.ChangeAmount)
            .Map(dest => dest.DateTime, src => src.PostingDate)
            .Map(dest => dest.ActionType, src => src.ActionType)
            .Map(dest => dest.GoldUnitType, src => 
                src.Product != null ? src.Product.GoldUnitType : (GoldUnitType?)null)
            .Map(dest => dest.PriceUnit, src => src.Currency != null ? src.Currency.Title : null)
            .Map(dest => dest.Description, src => GenerateDescription(src));

    }

    private static string GenerateDescription(InventoryStock src)
    {
        var prefix = src.ReverseInventoryStockId.HasValue ? "برگشت: " : string.Empty;
        var action = src.ActionType == WarehouseActionType.In ? "ورود" : "خروج";

        if (src.Product != null)
        {
            return $"{prefix}{action} {src.ChangeAmount.ToWeightFormat(src.Product.GoldUnitType)} {src.Product.Name}";
        }

        if (src.Coin != null)
        {
            return $"{prefix}{action} {src.ChangeAmount:G29} عدد {src.Coin.Title}";
        }

        if (src.Currency != null)
        {
            return $"{prefix}{action} {src.ChangeAmount.ToCurrencyFormat(src.Currency.Title)}";
        }

        return $"{prefix}{action} {src.ChangeAmount} واحد نامشخص";
    }
}