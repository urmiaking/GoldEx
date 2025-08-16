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
            .Map(dest => dest.Amount, src => src.ChangeAmount);

        config.NewConfig<InventorySummaryData, GetInventoryStockResponse>()
            .Map(dest => dest.Amount, src => src.CurrentQuantity);
    }
}