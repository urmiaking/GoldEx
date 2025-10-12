using System.Numerics;
using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CoinAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.InventoryStockAggregate;

public readonly record struct InventoryStockId(Guid Value);
public class InventoryStock : EntityBase<InventoryStockId>
{
    public Product? Product { get; private set; }
    public ProductId? ProductId { get; private set; }

    public Coin? Coin { get; private set; }
    public CoinId? CoinId { get; private set; }

    public PriceUnit? Currency { get; private set; }
    public PriceUnitId? CurrencyId { get; private set; }

    public MeltingBatchId? MeltingBatchId { get; private set; }
    public MeltingBatch? MeltingBatch { get; private set; }

    public decimal ChangeAmount { get; private set; }

    public WarehouseActionType ActionType { get; private set; }

    public Invoice? Invoice { get; private set; }
    public InvoiceId? InvoiceId { get; private set; }

    public MoltenGoldDetail? MoltenGoldDetail { get; private set; }

    // TODO: add warehousingId when implemented

    public static InventoryStock CreateMeltingBatchProduct(
        ProductId productId,
        int changeAmount,
        WarehouseActionType actionType,
        MeltingBatchId batchId)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(changeAmount, 0, nameof(changeAmount));

        return new InventoryStock
        {
            Id = new InventoryStockId(Guid.NewGuid()),
            ProductId = productId,
            ChangeAmount = changeAmount,
            ActionType = actionType,
            MeltingBatchId = batchId
        };
    }

    public static InventoryStock CreateProduct(
        ProductId productId,
        int changeAmount,
        WarehouseActionType actionType,
        InvoiceId? invoiceId = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(changeAmount, 0, nameof(changeAmount));

        return new InventoryStock
        {
            Id = new InventoryStockId(Guid.NewGuid()),
            ProductId = productId,
            ChangeAmount = changeAmount,
            ActionType = actionType,
            InvoiceId = invoiceId
        };
    }

    public static InventoryStock CreateCoin(
        CoinId coinId,
        int changeAmount,
        WarehouseActionType actionType,
        InvoiceId? invoiceId = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(changeAmount, 0, nameof(changeAmount));

        return new InventoryStock
        {
            Id = new InventoryStockId(Guid.NewGuid()),
            CoinId = coinId,
            ChangeAmount = changeAmount,
            ActionType = actionType,
            InvoiceId = invoiceId
        };
    }

    public static InventoryStock CreateCurrency(
        PriceUnitId currencyId,
        decimal changeAmount,
        WarehouseActionType actionType,
        InvoiceId? invoiceId = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(changeAmount, 0, nameof(changeAmount));

        return new InventoryStock
        {
            Id = new InventoryStockId(Guid.NewGuid()),
            CurrencyId = currencyId,
            ChangeAmount = changeAmount,
            ActionType = actionType,
            InvoiceId = invoiceId
        };
    }

    public static InventoryStock CreateMoltenGold(
        ProductId productId,
        MeltingBatchId meltingBatchId,
        MoltenGoldDetail moltenGoldDetail,
        decimal changeAmount,
        WarehouseActionType actionType,
        InvoiceId? invoiceId = null)
    {
        ArgumentNullException.ThrowIfNull(moltenGoldDetail);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(changeAmount, 0, nameof(changeAmount));
        return new InventoryStock
        {
            Id = new InventoryStockId(Guid.NewGuid()),
            ProductId = productId,
            MoltenGoldDetail = moltenGoldDetail,
            MeltingBatchId = meltingBatchId,
            ChangeAmount = changeAmount,
            ActionType = actionType,
            InvoiceId = invoiceId
        };
    }

    private InventoryStock() { }
}