using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using Mapster;
using MapsterMapper;

namespace GoldEx.Server.Common.Mapping;

public class MeltingBatchMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MeltingBatch, GetMeltingBatchResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.TotalWeight, src => src.TotalWeight)
            .Map(dest => dest.WeightUnitType, src => src.WeightUnitType)
            .Map(dest => dest.CurrentStatus, src => src.CurrentStatus)
            .Map(dest => dest.CurrentDateTime, src => src.CurrentDateTime)
            .Map(dest => dest.Assayer, src => src.Assayer)
            .Map(dest => dest.Transactions,
                src => src.Transactions != null
                    ? src.Transactions.OrderByDescending(x => x.CreatedAt)
                        .ToList()
                    : null)
            .Map(dest => dest.Products, src =>
                src.InventoryStocks != null
                    ? src.InventoryStocks.Where(x =>
                        x.Product != null && x.Product.ProductType == ProductType.UsedGold).Select(x => x.Product)
                    : new List<Product>())
            .Map(dest => dest.FinalProductDetail, src =>
                src.InventoryStocks != null &&
                src.InventoryStocks.Any(x => x.Product != null && x.Product.ProductType == ProductType.MoltenGold) &&
                src.InventoryStocks.First(x => x.Product!.ProductType == ProductType.MoltenGold).MoltenGoldDetail !=
                null
                    ? new GetMeltingBatchDetailResponse(
                        MapContext.Current.GetService<IMapper>().Map<GetProductResponse>(
                            src.InventoryStocks.First(x => x.Product!.ProductType == ProductType.MoltenGold).Product!),
                        src.InventoryStocks.First(x => x.Product!.ProductType == ProductType.MoltenGold).ChangeAmount,
                        src.InventoryStocks.First(x => x.Product!.ProductType == ProductType.MoltenGold)
                            .MoltenGoldDetail!.WeightUnitType,
                        src.InventoryStocks.First(x => x.Product!.ProductType == ProductType.MoltenGold)
                            .MoltenGoldDetail!.Fineness,
                        src.InventoryStocks.First(x => x.Product!.ProductType == ProductType.MoltenGold)
                            .MoltenGoldDetail!.AssayNumber)
                    : null
            );

        config.NewConfig<Transaction, GetMeltingBatchTransactionSummary>()
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Amount, src => src.Amount)
            .Map(dest => dest.TransactionType, src => src.TransactionType)
            .Map(dest => dest.LedgerAccount, src => src.LedgerAccount != null ? src.LedgerAccount.Title : string.Empty)
            .Map(dest => dest.PriceUnit, src => src.PriceUnit != null ? src.PriceUnit.Title : string.Empty)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);
    }
}