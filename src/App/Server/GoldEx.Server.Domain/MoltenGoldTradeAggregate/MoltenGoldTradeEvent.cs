using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.CustomerTransferVoucherAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Shared.DTOs.MoltenGold;

namespace GoldEx.Server.Domain.MoltenGoldTradeAggregate;

public class MoltenGoldTradeEvent : EntityBase<Guid>, IStoreFiltered
{
    private MoltenGoldTradeEvent() { }

    public StoreId StoreId { get; private set; }
    public Guid TradeId { get; private set; }
    public MoltenGoldTradeEventKind Kind { get; private set; }
    public decimal Amount { get; private set; }
    public decimal Weight { get; private set; }
    public InvoiceId? InvoiceId { get; private set; }
    public CustomerTransferVoucherId? VoucherId { get; private set; }
    public string? Reference { get; private set; }
    public Guid? JournalGroupId { get; private set; }

    public static MoltenGoldTradeEvent Create(Guid id, MoltenGoldTrade trade, MoltenGoldTradeEventKind kind,
        decimal amount = 0, decimal weight = 0, InvoiceId? invoiceId = null,
        CustomerTransferVoucherId? voucherId = null, string? reference = null, Guid? journalGroupId = null) => new()
        {
            Id = id, StoreId = trade.StoreId, TradeId = trade.Id, Kind = kind, Amount = amount,
            Weight = weight, InvoiceId = invoiceId, VoucherId = voucherId, Reference = reference, JournalGroupId = journalGroupId
        };
}
