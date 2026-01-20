using GoldEx.Server.Domain.InventoryExitAggregate;
using GoldEx.Shared.DTOs.InventoryExits;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class InventoryExitMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<InventoryExit, InventoryExitResponse>()
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