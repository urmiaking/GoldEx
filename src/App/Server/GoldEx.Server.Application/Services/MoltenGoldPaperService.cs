using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.MoltenGoldTradeAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.Transactions;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.MoltenGold;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class MoltenGoldPaperService(ITransactionRepository journal, ILedgerAccountRepository ledgers,
    IServerLedgerAccountService customerLedgers, IInvoicePaymentRepository payments,
    IMoltenGoldTradeRepository trades) : IServerMoltenGoldPaperService
{
    public async Task PostAgreementAsync(MoltenGoldTrade trade, CancellationToken ct)
    {
        var sell = trade.Side == InvoiceType.Sell;
        var money = await customerLedgers.GetOrCreateCustomerSubLedgerAsync(trade.CustomerId, trade.PriceUnitId,
            sell ? LedgerAccountRole.Receivable : LedgerAccountRole.Payable, ct);
        var gold = await GoldLedgerAsync(trade, ct);
        var group = Guid.CreateVersion7();
        var total = trade.Value(trade.Weight);
        var totalBase = Round(total * trade.ExchangeRate);
        var entries = new List<Transaction>
        {
            Entry(trade, sell ? TransactionType.Debit : TransactionType.Credit, money.Id, trade.PriceUnitId,
                total, totalBase, group, "حساب وجه معاملهٔ آب‌شدهٔ کاغذی"),
            Entry(trade, sell ? TransactionType.Credit : TransactionType.Debit, gold.Id, trade.GoldPriceUnitId!.Value,
                trade.RemainingGoldWeight, trade.RemainingGoldBookAmount, group, "حساب طلای معاملهٔ آب‌شدهٔ کاغذی")
        };
        var fee = totalBase - trade.RemainingGoldBookAmount;
        if (fee > 0)
        {
            var ledger = await SystemLedgerAsync(sell ? SystemLedgerAccounts.AdditionalChargesRevenue
                : SystemLedgerAccounts.ServiceExpenses, ct);
            entries.Add(Entry(trade, sell ? TransactionType.Credit : TransactionType.Debit, ledger.Id,
                trade.PriceUnitId, Round(fee / trade.ExchangeRate), fee, group, "کارمزد معاملهٔ آب‌شدهٔ کاغذی"));
        }
        await journal.CreateRangeAsync(entries, ct);
        await trades.AddEventAsync(MoltenGoldTradeEvent.Create(Guid.NewGuid(), trade, MoltenGoldTradeEventKind.Agreement,
            amount: total, journalGroupId: group), ct);
    }

    public async Task ReverseAgreementAsync(MoltenGoldTrade trade, CancellationToken ct)
    {
        var entries = await journal.Get(new TransactionsByMoltenGoldTradeSpecification(trade.Id)).AsNoTracking().ToListAsync(ct);
        var group = Guid.CreateVersion7();
        var reversals = entries.Select(x => Entry(trade, x.TransactionType == TransactionType.Debit
            ? TransactionType.Credit : TransactionType.Debit, x.LedgerAccountId, x.PriceUnitId,
            x.Amount, x.BaseCurrencyAmount, group, "برگشت معاملهٔ لغوشدهٔ آب‌شدهٔ کاغذی").MarkAsReversalOf(x.Id)).ToList();
        await journal.CreateRangeAsync(reversals, ct);
    }

    public async Task SettleDeliveryAsync(MoltenGoldTrade trade, Invoice invoice, MoltenGoldPosition position, CancellationToken ct)
    {
        var sell = trade.Side == InvoiceType.Sell;
        var contract = await GoldLedgerAsync(trade, ct);
        var invoiceLedger = await customerLedgers.GetOrCreateCustomerSubLedgerAsync(trade.CustomerId,
            invoice.PriceUnitId, sell ? LedgerAccountRole.Receivable : LedgerAccountRole.Payable, ct);
        var amount = Round(invoice.TotalAmountWithDiscountsAndExtraCosts);
        var payment = InvoicePayment.Create(DateTime.Now, PaymentType.MoltenGoldDesk,
            sell ? PaymentSide.Receive : PaymentSide.Pay, amount, null, null, invoice.Id, invoice.PriceUnitId,
            null, contract.Id, null, null, null, note: "تسویه از حساب طلای آب‌شدهٔ کاغذی", storeId: trade.StoreId);
        await payments.CreateAsync(payment, ct);
        var group = Guid.CreateVersion7();
        var entries = new List<Transaction>
        {
            Entry(trade, sell ? TransactionType.Debit : TransactionType.Credit, contract.Id, invoice.PriceUnitId,
                position.Weight, position.BookAmount, group, "تحویل فیزیکی از حساب طلای آب‌شدهٔ کاغذی"),
            Transaction.CreateForInvoicePayment("تسویهٔ طلایی فاکتور تحویل آب‌شده", amount, invoice.ExchangeRate,
                group, sell ? TransactionType.Credit : TransactionType.Debit, invoiceLedger.Id,
                invoice.PriceUnitId, invoice.Id, payment.Id, DateTime.Now, trade.StoreId)
        };
        await AddDifferenceAsync(trade, entries, group, ct);
        await journal.CreateRangeAsync(entries, ct);
    }

    public async Task<Guid> SettleGoldAsync(MoltenGoldTrade first, MoltenGoldTrade second, decimal goldWeight, CancellationToken ct)
    {
        var sale = first.Side == InvoiceType.Sell ? first : second;
        var purchase = first.Side == InvoiceType.Purchase ? first : second;
        var salePosition = sale.SettleGold(goldWeight);
        var purchasePosition = purchase.SettleGold(goldWeight);
        var payable = await GoldLedgerAsync(sale, ct);
        var receivable = await GoldLedgerAsync(purchase, ct);
        var group = Guid.CreateVersion7();
        var entries = new List<Transaction>
        {
            Entry(sale, TransactionType.Debit, payable.Id, sale.GoldPriceUnitId!.Value,
                goldWeight, salePosition.BookAmount, group, $"تسویهٔ طلایی با معاملهٔ {purchase.Id}"),
            Entry(purchase, TransactionType.Credit, receivable.Id, purchase.GoldPriceUnitId!.Value,
                goldWeight, purchasePosition.BookAmount, group, $"تسویهٔ طلایی با معاملهٔ {sale.Id}")
        };
        await AddDifferenceAsync(sale, entries, group, ct);
        await journal.CreateRangeAsync(entries, ct);
        return group;
    }

    private async Task AddDifferenceAsync(MoltenGoldTrade trade, List<Transaction> entries, Guid group, CancellationToken ct)
    {
        var difference = entries.Sum(x => x.TransactionType == TransactionType.Debit
            ? Round(x.BaseCurrencyAmount) : -Round(x.BaseCurrencyAmount));
        if (difference == 0)
            return;
        var ledger = await SystemLedgerAsync(SystemLedgerAccounts.ExchangeGainLoss, ct);
        entries.Add(Entry(trade, difference > 0 ? TransactionType.Credit : TransactionType.Debit,
            ledger.Id, trade.PriceUnitId, Round(Math.Abs(difference) / trade.ExchangeRate),
            Math.Abs(difference), group, "اختلاف بهای تسویهٔ طلایی آب‌شدهٔ کاغذی"));
    }

    private Task<LedgerAccount> GoldLedgerAsync(MoltenGoldTrade trade, CancellationToken ct)
        => customerLedgers.GetOrCreateCustomerSubLedgerAsync(trade.CustomerId, trade.GoldPriceUnitId!.Value,
            trade.Side == InvoiceType.Sell ? LedgerAccountRole.Payable : LedgerAccountRole.Receivable, ct);

    private async Task<LedgerAccount> SystemLedgerAsync(string title, CancellationToken ct)
        => await ledgers.Get(new LedgerAccountsByTitleSpecification(title)).FirstOrDefaultAsync(ct) ?? throw new NotFoundException();

    private static Transaction Entry(MoltenGoldTrade trade, TransactionType type, LedgerAccountId ledger,
        PriceUnitId unit, decimal amount, decimal baseAmount, Guid group, string description)
        => Transaction.CreateForMoltenGoldTrade(trade, description, amount,
            amount == 0 || baseAmount == 0 ? 1 : Math.Round(baseAmount / amount, 18, MidpointRounding.AwayFromZero),
            baseAmount, group, type, ledger, unit);

    private static decimal Round(decimal value) => Math.Round(value, 10, MidpointRounding.AwayFromZero);
}
