using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.InventoryEntryAggregate;
using GoldEx.Server.Domain.InventoryExitAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.InventoryStockAggregate;

public readonly record struct InventoryStockId(Guid Value);
public class InventoryStock : EntityBase<InventoryStockId>
{
    public Product? Product { get; private set; }
    public ProductId? ProductId { get; private set; }

    public CoinInstance? CoinInstance { get; private set; }
    public CoinInstanceId? CoinInstanceId { get; private set; }

    public PriceUnit? Currency { get; private set; }
    public PriceUnitId? CurrencyId { get; private set; }

    public MeltingBatchId? MeltingBatchId { get; private set; }
    public MeltingBatch? MeltingBatch { get; private set; }

    public decimal ChangeAmount { get; private set; }

    public WarehouseActionType ActionType { get; private set; }

    public Invoice? Invoice { get; private set; }
    public InvoiceId? InvoiceId { get; private set; }

    public MoltenGoldDetail? MoltenGoldDetail { get; private set; }

    public DateTime PostingDate { get; private set; }

    public InventoryStockId? ReverseInventoryStockId { get; private set; }
    public InventoryStock? ReverseInventoryStock { get; private set; }

    public InventoryEntryId? InventoryEntryId { get; private set; }
    public InventoryEntry? InventoryEntry { get; private set; }

    public InventoryExitId? InventoryExitId { get; private set; }
    public InventoryExit? InventoryExit { get; private set; }

    public IReadOnlyList<Transaction>? Transactions { get; private set; }

    public static InventoryStock CreateMeltingBatchProduct(
        ProductId productId,
        decimal changeAmount,
        WarehouseActionType actionType,
        MeltingBatchId batchId,
        DateTime? postingDate = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(changeAmount, 0);

        return new InventoryStock
        {
            Id = new InventoryStockId(Guid.CreateVersion7()),
            ProductId = productId,
            ChangeAmount = changeAmount,
            ActionType = actionType,
            MeltingBatchId = batchId,
            PostingDate = postingDate ?? DateTime.Now
        };
    }

    public static InventoryStock CreateProduct(
        ProductId productId,
        decimal changeAmount,
        WarehouseActionType actionType,
        InvoiceId? invoiceId = null,
        DateTime? postingDate = null,
        InventoryEntryId? inventoryEntryId = null,
        InventoryExitId? inventoryExitId = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(changeAmount, 0);

        return new InventoryStock
        {
            Id = new InventoryStockId(Guid.CreateVersion7()),
            ProductId = productId,
            ChangeAmount = changeAmount,
            ActionType = actionType,
            InvoiceId = invoiceId,
            PostingDate = postingDate ?? DateTime.Now,
            InventoryEntryId = inventoryEntryId,
            InventoryExitId = inventoryExitId
        };
    }

    public static InventoryStock CreateCoin(
        CoinInstanceId coinInstanceId,
        int changeAmount,
        WarehouseActionType actionType,
        InvoiceId? invoiceId = null,
        DateTime? postingDate = null,
        InventoryEntryId? inventoryEntryId = null,
        InventoryExitId? inventoryExitId = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(changeAmount, 0);

        return new InventoryStock
        {
            Id = new InventoryStockId(Guid.CreateVersion7()),
            CoinInstanceId = coinInstanceId,
            ChangeAmount = changeAmount,
            ActionType = actionType,
            InvoiceId = invoiceId,
            PostingDate = postingDate ?? DateTime.Now,
            InventoryEntryId = inventoryEntryId,
            InventoryExitId = inventoryExitId
        };
    }

    public static InventoryStock CreateCurrency(
        PriceUnitId currencyId,
        decimal changeAmount,
        WarehouseActionType actionType,
        InvoiceId? invoiceId = null,
        DateTime? postingDate = null,
        InventoryEntryId? inventoryEntryId = null,
        InventoryExitId? inventoryExitId = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(changeAmount, 0);

        return new InventoryStock
        {
            Id = new InventoryStockId(Guid.CreateVersion7()),
            CurrencyId = currencyId,
            ChangeAmount = changeAmount,
            ActionType = actionType,
            InvoiceId = invoiceId,
            PostingDate = postingDate ?? DateTime.Now,
            InventoryEntryId = inventoryEntryId,
            InventoryExitId = inventoryExitId
        };
    }

    public static InventoryStock CreateMoltenGold(
        ProductId productId,
        MeltingBatchId meltingBatchId,
        MoltenGoldDetail moltenGoldDetail,
        decimal changeAmount,
        WarehouseActionType actionType,
        InvoiceId? invoiceId = null,
        DateTime? postingDate = null,
        InventoryEntryId? inventoryEntryId = null)
    {
        ArgumentNullException.ThrowIfNull(moltenGoldDetail);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(changeAmount, 0);

        return new InventoryStock
        {
            Id = new InventoryStockId(Guid.CreateVersion7()),
            ProductId = productId,
            MoltenGoldDetail = moltenGoldDetail,
            MeltingBatchId = meltingBatchId,
            ChangeAmount = changeAmount,
            ActionType = actionType,
            InvoiceId = invoiceId,
            PostingDate = postingDate ?? DateTime.Now,
            InventoryEntryId = inventoryEntryId
        };
    }

    public static InventoryStock CreateProductExit(
        ProductId productId,
        decimal changeAmount,
        InventoryExitId inventoryExitId,
        DateTime? postingDate = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(changeAmount, 0);

        return new InventoryStock
        {
            Id = new InventoryStockId(Guid.CreateVersion7()),
            ProductId = productId,
            ChangeAmount = changeAmount,
            ActionType = WarehouseActionType.Out,
            InventoryExitId = inventoryExitId,
            PostingDate = postingDate ?? DateTime.Now
        };
    }

    public static InventoryStock CreateCoinExit(
        CoinInstanceId coinInstanceId,
        int changeAmount,
        InventoryExitId inventoryExitId,
        DateTime? postingDate = null)
    {
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(changeAmount, 0);

        return new InventoryStock
        {
            Id = new InventoryStockId(Guid.CreateVersion7()),
            CoinInstanceId = coinInstanceId,
            ChangeAmount = changeAmount,
            ActionType = WarehouseActionType.Out,
            InventoryExitId = inventoryExitId,
            PostingDate = postingDate ?? DateTime.Now
        };
    }

    public InventoryStock MarkAsReversalOf(InventoryStockId originalId)
    {
        ReverseInventoryStockId = originalId;
        return this;
    }

    private InventoryStock() { }
}