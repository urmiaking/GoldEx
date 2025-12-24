using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Shared.DTOs.InventoryEntries;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class InventoryEntryMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InventoryEntry, InventoryEntryResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.OperationDate, src => src.CreatedAt)
            .Map(dest => dest.ProductsAmount,
                src => src.InventoryStocks != null
                    ? src.InventoryStocks
                        .Where(x => x.ProductId != null)
                        .Sum(x => x.ChangeAmount)
                    : 0)
            .Map(dest => dest.CoinsAmount, src =>
                src.InventoryStocks != null
                    ? src.InventoryStocks.Where(x => x.CoinInstanceId != null)
                        .Sum(x => x.ChangeAmount)
                    : 0)
            .Map(dest => dest.CurrenciesAmount, src =>
                src.InventoryStocks != null
                    ? src.InventoryStocks.Where(x => x.CurrencyId != null)
                        .Sum(x => x.ChangeAmount)
                    : 0);
    }
}