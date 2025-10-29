using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Shared.DTOs.InventoryStocks;
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
    }
}