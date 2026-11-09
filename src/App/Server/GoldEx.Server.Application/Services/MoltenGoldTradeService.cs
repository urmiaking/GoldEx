using System.Data;
using FluentValidation;
using FluentValidation.Results;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.MoltenGoldTradeAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.Customers;
using GoldEx.Server.Infrastructure.Specifications.FinancialAccounts;
using GoldEx.Server.Infrastructure.Specifications.Invoices;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Server.Infrastructure.Specifications.Prices;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.CustomerTransfers;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.MoltenGold;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class MoltenGoldTradeService(
    IMoltenGoldTradeRepository trades,
    IInvoiceRepository invoices,
    IInvoicePaymentRepository payments,
    IServerInvoiceService invoiceService,
    IServerCustomerTransferVoucherService transfers,
    IServerMoltenGoldPaperService paper,
    IInventoryStockRepository stock,
    IProductRepository products,
    ICustomerRepository customers,
    IPriceUnitRepository priceUnits,
    IPriceRepository prices,
    IFinancialAccountRepository accounts,
    ILedgerAccountRepository ledgers,
    IServerLedgerAccountService customerLedgers,
    ITransactionRepository journal,
    ISettingService settings,
    IStoreContext storeContext) : IMoltenGoldTradeService
{
    public async Task<MoltenGoldDeskSettings> GetSettingsAsync(CancellationToken cancellationToken = default)
    {
        RequireStore();
        var setting = await settings.GetAsync(cancellationToken);
        var unit = await priceUnits.Get(new PriceUnitsByUnitTypeSpecification(UnitType.TMN))
            .Where(x => x.IsActive).OrderByDescending(x => x.IsDefault).ThenBy(x => x.Id)
            .FirstOrDefaultAsync(cancellationToken);
        var goldUnit = await priceUnits.Get(new PriceUnitsByUnitTypeSpecification(UnitType.Gold18K))
            .Where(x => x.IsActive).OrderBy(x => x.Id).FirstOrDefaultAsync(cancellationToken);
        var rate = await prices.Get(new PricesByPriceCatalogSpecification(PriceCatalog.Geram18)).AsNoTracking()
            .Where(x => x.IsActive).FirstOrDefaultAsync(cancellationToken);
        return new(setting?.MoltenGoldCommissionPercent ?? 1.5m, setting?.GramPerMesghal ?? 4.6083m,
            unit?.Id.Value ?? Guid.Empty, goldUnit?.Id.Value ?? Guid.Empty,
            rate?.PriceHistory?.CurrentValue / 10m ?? 0, rate?.PriceHistory?.LastUpdate);
    }

    public async Task<List<MoltenGoldStockChoice>> SearchStockAsync(string query, CancellationToken cancellationToken = default)
    {
        RequireStore();
        query = query.Trim();
        if (query.Length < 2)
            return [];

        var candidates = await products.Get(new ProductsByMoltenGoldSpecification())
            .Where(x => x.GoldUnitType == GoldUnitType.Gram &&
                (x.Barcode.Contains(query) || x.Name.Contains(query) ||
                 (x.MoltenGold != null && x.MoltenGold.AssayNumber != null && x.MoltenGold.AssayNumber.Contains(query))))
            .OrderBy(x => x.Barcode).Take(30).AsNoTracking().ToListAsync(cancellationToken);
        var quantities = await stock.GetQuantitiesAsync(candidates.Select(x => x.Id), cancellationToken);
        var results = new List<MoltenGoldStockChoice>();
        foreach (var product in candidates)
        {
            var available = quantities.GetValueOrDefault(product.Id) - await stock.GetReservedMoltenGoldAsync(product.Id, cancellationToken);
            if (available > 0)
                results.Add(new(product.Id.Value, product.Name, product.Barcode, product.Fineness, available));
        }
        return results;
    }

    public async Task<List<MoltenGoldTradeResponse>> GetListAsync(MoltenGoldTradeFilter filter, int skip, CancellationToken cancellationToken = default)
    {
        RequireStore();
        var today = DateTime.Today;
        var query = trades.Query.AsNoTracking().Where(x => !x.IsCancelled);
        if (filter == MoltenGoldTradeFilter.Today)
            query = query.Where(x => x.CreatedAt >= today && x.CreatedAt < today.AddDays(1));
        if (filter == MoltenGoldTradeFilter.Undelivered)
            query = query.Where(x => x.RemainingWeight > 0);

        var result = new List<MoltenGoldTradeResponse>();
        var offset = filter == MoltenGoldTradeFilter.Unsettled ? 0 : Math.Max(skip, 0);
        var remainingSkip = filter == MoltenGoldTradeFilter.Unsettled ? Math.Max(skip, 0) : 0;
        while (result.Count < 30)
        {
            var batch = await query.Include(x => x.Customer)
                .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id)
                .Skip(offset).Take(30).ToListAsync(cancellationToken);
            if (batch.Count == 0)
                break;
            var ids = batch.Select(x => x.Id).ToArray();
            var events = await trades.Events.AsNoTracking().Where(x => ids.Contains(x.TradeId))
                .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToListAsync(cancellationToken);
            var byTrade = events.ToLookup(x => x.TradeId);
            var posted = await GetDeliveryInvoicesAsync(events, cancellationToken);
            foreach (var trade in batch)
            {
                var state = BuildState(trade, byTrade[trade.Id].ToList(), posted);
                if (filter == MoltenGoldTradeFilter.Unsettled && state.Remaining <= 0)
                    continue;
                if (remainingSkip > 0)
                {
                    remainingSkip--;
                    continue;
                }
                result.Add(new(trade.Id, trade.Side, trade.CustomerId.Value, trade.Customer.FullName, trade.CreatedAt,
                    trade.Weight, trade.Fineness, trade.CommissionPercent, state.Total, trade.RemainingWeight,
                    Math.Max(state.Remaining, 0), state.Events.Select(x =>
                    {
                        var invoice = x.InvoiceId.HasValue ? state.Invoices.GetValueOrDefault(x.InvoiceId.Value) : null;
                        return new MoltenGoldTradeEventResponse(x.Kind, x.CreatedAt, x.Amount, x.Weight,
                            x.InvoiceId?.Value, invoice?.InvoiceNumber, invoice?.TotalUnpaidAmount ?? 0, x.Reference);
                    }).ToList(), trade.IsPaper, trade.RemainingGoldWeight));
                if (result.Count == 30)
                    break;
            }
            if (batch.Count < 30)
                break;
            offset += batch.Count;
        }
        return result;
    }

    public async Task CreateAsync(CreateMoltenGoldTrade request, CancellationToken cancellationToken = default)
    {
        RequireStore();
        await using var transaction = await trades.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await trades.LockAsync(cancellationToken);
        if (await trades.Query.AnyAsync(x => x.Id == request.Id, cancellationToken))
            return;

        if (request.Id == Guid.Empty || request.Side is not (InvoiceType.Sell or InvoiceType.Purchase) ||
            !Enum.IsDefined(request.RateBasis) || !ValidWeight(request.Weight) || request.Fineness is <= 0 or >= 1000 ||
            request.Fineness != Math.Round(request.Fineness, 6) ||
            request.Rate is <= 0 or > 1_000_000_000_000m || request.Rate != Math.Round(request.Rate, 10) || request.AssayNumber?.Length > 100 ||
            request.DeliveryWeight < 0 || request.DeliveryWeight > request.Weight ||
            request.DeliveryWeight != Math.Round(request.DeliveryWeight, 3) || request.PaymentAmount < 0)
            throw Reject("اطلاعات معامله، وزن، عیار یا نرخ معتبر نیست.");

        if (!await customers.ExistsAsync(new CustomersByIdSpecification(new(request.CustomerId)), cancellationToken))
            throw Reject("طرف‌حساب معتبر نیست.");

        var defaults = await GetSettingsAsync(cancellationToken);
        var commission = request.CommissionPercent ?? (request.Side == InvoiceType.Sell ? defaults.CommissionPercent : 0);
        if (commission is < 0 or > 100 || commission != Math.Round(commission, 6) || defaults.GramPerMesghal <= 0)
            throw Reject("درصد کارمزد یا ضریب تبدیل مظنه معتبر نیست.");

        var baseUnit = await priceUnits.Get(new PriceUnitsSetAsDefaultSpecification()).FirstOrDefaultAsync(cancellationToken);
        if (defaults.PriceUnitId == Guid.Empty || baseUnit?.UnitType is not (UnitType.IRR or UnitType.TMN))
            throw Reject("دفتر آب‌شده به واحد تومان و واحد پایهٔ ریال یا تومان نیاز دارد.");
        if (request.IsPaper && defaults.GoldPriceUnitId == Guid.Empty)
            throw Reject("برای حساب طلایی، واحد گرم طلای ۱۸ عیار را فعال کنید.");

        if (!request.IsPaper && request.Side == InvoiceType.Sell)
        {
            if (request.ProductId is not Guid productId)
                throw Reject("برای فروش، قطعهٔ آب‌شده را از موجودی انتخاب کنید.");
            var productKey = new ProductId(productId);
            await stock.LockProductAsync(productKey, cancellationToken);
            var product = await GetProductAsync(productKey, cancellationToken);
            var available = await stock.GetQuantityAsync(productKey, cancellationToken)
                - await stock.GetReservedMoltenGoldAsync(productKey, cancellationToken);
            if (product.Fineness != request.Fineness || request.Weight > available)
                throw Reject("عیار قطعه یا وزن آزاد موجودی برای این فروش کافی نیست.");
        }
        else if (request.Side == InvoiceType.Purchase && request.ProductId.HasValue)
        {
            throw Reject("قطعهٔ خرید هنگام دریافت به انبار اضافه می‌شود.");
        }

        var gramRate = Math.Round(MoltenGoldTradePricing.GramRate(request.Rate, request.RateBasis, defaults.GramPerMesghal), 18);
        if (gramRate <= 0)
            throw Reject("نرخ تبدیل‌شده معتبر نیست.");
        var trade = MoltenGoldTrade.Create(request, new(defaults.PriceUnitId), baseUnit.UnitType == UnitType.IRR ? 10 : 1,
            gramRate, commission, new StoreId(storeContext.StoreId!.Value), request.IsPaper ? new PriceUnitId(defaults.GoldPriceUnitId) : null);
        if (trade.IsPaper && (trade.RemainingGoldWeight <= 0 || trade.RemainingGoldBookAmount <= 0))
            throw Reject("وزن یا ارزش طلای معامله کمتر از دقت قابل ثبت است.");
        await trades.CreateAsync(trade, cancellationToken);
        if (trade.IsPaper)
            await paper.PostAgreementAsync(trade, cancellationToken);
        if (request.DeliveryWeight > 0)
            await PostDeliveryAsync(trade, Guid.NewGuid(), request.DeliveryWeight, null, request.ProductId, cancellationToken);
        if (request.PaymentAmount > 0)
            await PostPaymentAsync(trade, new(Guid.NewGuid(), request.FinancialAccountId ?? Guid.Empty, request.PaymentAmount, request.Reference), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public Task DeliverAsync(Guid id, DeliverMoltenGold request, CancellationToken cancellationToken = default)
        => ExecuteAsync(id, request.OperationId, (trade, ct) => PostDeliveryAsync(trade, request.OperationId, request.Weight, request.AssayNumber, request.ProductId, ct), cancellationToken);

    public Task PayAsync(Guid id, PayMoltenGold request, CancellationToken cancellationToken = default)
        => ExecuteAsync(id, request.OperationId, (trade, ct) => PostPaymentAsync(trade, request, ct), cancellationToken);

    public Task TransferAsync(Guid id, TransferMoltenGold request, CancellationToken cancellationToken = default)
        => ExecuteAsync(id, request.OperationId, (trade, ct) => PostTransferAsync(trade, request, ct), cancellationToken);

    public async Task<List<MoltenGoldSettlementChoice>> SearchSettlementsAsync(Guid id, string query, CancellationToken cancellationToken = default)
    {
        RequireStore();
        var trade = await GetTradeAsync(id, cancellationToken);
        var text = query.Trim();
        return await trades.Query.AsNoTracking().Where(x => x.IsPaper && !x.IsCancelled && x.Id != id &&
                x.Side != trade.Side && x.GoldPriceUnitId == trade.GoldPriceUnitId && x.RemainingGoldWeight > 0 &&
                (text == "" || x.Customer.FullName.Contains(text)))
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Take(30)
            .Select(x => new MoltenGoldSettlementChoice(x.Id, x.Customer.FullName, x.RemainingGoldWeight, x.GramRate, x.CreatedAt)).ToListAsync(cancellationToken);
    }

    public Task SettleGoldAsync(Guid id, SettleMoltenGold request, CancellationToken cancellationToken = default)
        => ExecuteAsync(id, request.OperationId, async (trade, ct) =>
        {
            var other = await GetTradeAsync(request.OtherTradeId, ct);
            if (!trade.IsPaper || !other.IsPaper || other.IsCancelled || other.Id == trade.Id || other.Side == trade.Side ||
                other.GoldPriceUnitId != trade.GoldPriceUnitId || request.Weight <= 0 ||
                request.Weight != Math.Round(request.Weight, 10) || request.Weight > trade.RemainingGoldWeight || request.Weight > other.RemainingGoldWeight)
                throw Reject("معاملهٔ مقابل یا وزن تسویهٔ طلایی معتبر نیست.");
            var group = await paper.SettleGoldAsync(trade, other, request.Weight, ct);
            await trades.AddEventAsync(MoltenGoldTradeEvent.Create(request.OperationId, trade, MoltenGoldTradeEventKind.GoldSettlement,
                weight: request.Weight, reference: $"تسویه با معاملهٔ {other.Id}", journalGroupId: group), ct);
            await trades.AddEventAsync(MoltenGoldTradeEvent.Create(Guid.NewGuid(), other, MoltenGoldTradeEventKind.GoldSettlement,
                weight: request.Weight, reference: $"تسویه با معاملهٔ {trade.Id}", journalGroupId: group), ct);
        }, cancellationToken);

    public async Task<List<MoltenGoldPaymentChoice>> SearchPaymentTradesAsync(Guid id, Guid customerId, CancellationToken cancellationToken = default)
    {
        RequireStore();
        var trade = await GetTradeAsync(id, cancellationToken);
        var candidates = await trades.Query.AsNoTracking().Where(x => x.IsPaper && !x.IsCancelled && x.Side != trade.Side &&
                x.CustomerId == new CustomerId(customerId) && x.PriceUnitId == trade.PriceUnitId)
            .OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id).Take(100).ToListAsync(cancellationToken);
        var ids = candidates.Select(x => x.Id).ToArray();
        var paid = await trades.Events.Where(x => ids.Contains(x.TradeId) &&
                (x.Kind == MoltenGoldTradeEventKind.Payment || x.Kind == MoltenGoldTradeEventKind.Transfer))
            .GroupBy(x => x.TradeId).Select(x => new { Id = x.Key, Amount = x.Sum(e => e.Amount) })
            .ToDictionaryAsync(x => x.Id, x => x.Amount, cancellationToken);
        return candidates.Select(x => new MoltenGoldPaymentChoice(x.Id, x.CreatedAt, x.Value(x.Weight) - paid.GetValueOrDefault(x.Id)))
            .Where(x => x.RemainingAmount > 0).ToList();
    }

    public async Task CancelAsync(Guid id, CancellationToken cancellationToken = default)
    {
        RequireStore();
        await using var transaction = await trades.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        await trades.LockAsync(cancellationToken);
        var trade = await GetTradeAsync(id, cancellationToken);
        if (trade.IsCancelled)
            return;
        if (await trades.Events.AnyAsync(x => x.TradeId == id && x.Kind != MoltenGoldTradeEventKind.Agreement, cancellationToken))
            throw Reject("لغو فقط برای معامله‌ای امکان‌پذیر است که هنوز وجه یا طلا جابه‌جا نشده است.");

        if (trade.ProductId.HasValue)
            await stock.LockProductAsync(trade.ProductId.Value, cancellationToken);
        if (trade.IsPaper)
            await paper.ReverseAgreementAsync(trade, cancellationToken);
        trade.Cancel();
        await trades.SaveAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task ExecuteAsync(Guid id, Guid operationId, Func<MoltenGoldTrade, CancellationToken, Task> action, CancellationToken ct)
    {
        if (operationId == Guid.Empty)
            throw Reject("شناسهٔ ثبت معتبر نیست.");
        RequireStore();
        await using var transaction = await trades.BeginTransactionAsync(IsolationLevel.Serializable, ct);
        await trades.LockAsync(ct);
        if (await trades.Events.AnyAsync(x => x.Id == operationId && x.TradeId == id, ct))
            return;
        var trade = await GetTradeAsync(id, ct);
        if (trade.IsCancelled)
            throw Reject("این معامله لغو شده است.");
        await action(trade, ct);
        await transaction.CommitAsync(ct);
    }

    private async Task PostDeliveryAsync(MoltenGoldTrade trade, Guid operationId, decimal weight, string? assayNumber, Guid? selectedProductId, CancellationToken ct)
    {
        if ((!ValidWeight(weight) && !(trade.IsPaper && weight > 0 && weight == trade.RemainingWeight)) || weight > trade.RemainingWeight)
            throw Reject("وزن این نوبت از ماندهٔ قابل تحویل بیشتر است یا معتبر نیست.");
        if (assayNumber?.Length > 100)
            throw Reject("شمارهٔ انگ معتبر نیست.");
        if (trade.Side == InvoiceType.Purchase && selectedProductId.HasValue)
            throw Reject("قطعهٔ خرید هنگام دریافت به انبار اضافه می‌شود.");

        Product? product = null;
        var stockProduct = trade.IsPaper && selectedProductId.HasValue ? new ProductId(selectedProductId.Value) : trade.ProductId;
        if (trade.IsPaper && trade.Side == InvoiceType.Sell && stockProduct is null)
            throw Reject("قطعهٔ آب‌شده را فقط برای تحویل واقعی از انبار انتخاب کنید.");
        if (stockProduct is ProductId productId)
        {
            await stock.LockProductAsync(productId, ct);
            product = await GetProductAsync(productId, ct);
            if (product.Fineness != trade.Fineness)
                throw Reject("عیار فعلی قطعه با عیار ثبت‌شدهٔ معامله یکسان نیست.");
            if (trade.IsPaper && await stock.GetQuantityAsync(productId, ct) - await stock.GetReservedMoltenGoldAsync(productId, ct) < weight)
                throw Reject("موجودی قطعه برای این تحویل کافی نیست.");
        }
        if (trade.IsPaper && weight < trade.RemainingWeight && Math.Round(weight * trade.Fineness / 750m, 10) <= 0)
            throw Reject("وزن طلای این نوبت کمتر از دقت قابل ثبت است.");
        var position = trade.IsPaper ? trade.TakeGold(weight) : null;
        if (position is { BookAmount: <= 0 })
            throw Reject("ارزش طلای این نوبت کمتر از دقت قابل ثبت است.");
        if (!trade.IsPaper)
            trade.Deliver(weight);
        await trades.SaveAsync(ct);

        var stamp = product is not null ? product.MoltenGold?.AssayNumber
            : string.IsNullOrWhiteSpace(assayNumber) ? trade.AssayNumber : assayNumber.Trim();
        var productRequest = new ProductRequestDto(product?.Id.Value, product?.Name, product?.Barcode,
            product?.Weight ?? weight, trade.IsPaper ? 0 : trade.CommissionPercent, WageType.Percent, ProductType.MoltenGold,
            trade.Fineness, GoldUnitType.Gram, product?.ProductCategoryId?.Value, null, null, null,
            new MoltenGoldDto(stamp, product?.MoltenGold?.AssayerId?.Value, product?.MoltenGold?.AssayDate));
        var invoiceUnit = trade.IsPaper ? trade.GoldPriceUnitId!.Value : trade.PriceUnitId;
        var invoiceRate = position is null ? trade.ExchangeRate : Math.Round(position.BookAmount / position.Weight, 18);
        decimal? purchaseAmount = trade.Side == InvoiceType.Purchase ? position?.Weight ?? trade.Value(weight) : null;
        decimal? purchaseWage = trade.Side == InvoiceType.Purchase ? productRequest.Wage : null;
        var item = new InvoiceProductItemDto(null, trade.IsPaper ? 1 : trade.GramRate, 0, 0,
            purchaseAmount, null, null, null,
            trade.Side == InvoiceType.Purchase ? invoiceUnit.Value : null,
            false, 1, weight, purchaseWage, WageType.Percent, productRequest);
        var number = await invoices.GetLastNumberAsync(trade.Side, ct) + 1;
        var invoice = await invoiceService.PostAsync(new(null, number, DateTime.Today, null, trade.Side, TradeScale.Retail,
            invoiceUnit.Value, null, null, invoiceRate, trade.CustomerId.Value, [item], [], [], [], [], [], []), ct, true);
        await trades.AddEventAsync(MoltenGoldTradeEvent.Create(operationId, trade, MoltenGoldTradeEventKind.Delivery,
            amount: trade.IsPaper ? 0 : invoice.TotalAmountWithDiscountsAndExtraCosts, weight: weight, invoiceId: invoice.Id), ct);
        if (position is not null)
            await paper.SettleDeliveryAsync(trade, invoice, position, ct);
        else
            await AllocateAdvancesAsync(trade, ct);
    }

    private async Task PostPaymentAsync(MoltenGoldTrade trade, PayMoltenGold request, CancellationToken ct)
    {
        var state = await LoadStateAsync(trade, ct);
        ValidateAmount(request.Amount, state.Remaining);
        var account = await accounts.Get(new FinancialAccountsByIdSpecification(new(request.AccountId)))
            .FirstOrDefaultAsync(ct);
        if (account is null || !account.IsSystemAccount || account.PriceUnitId != trade.PriceUnitId ||
            account.AccountType == FinancialAccountType.Gold || account.LedgerAccountId is not LedgerAccountId accountLedger)
            throw Reject("حساب بانکی یا صندوق تومانی فروشگاه را انتخاب کنید.");

        var sell = trade.Side == InvoiceType.Sell;
        var advance = trade.IsPaper
            ? await customerLedgers.GetOrCreateCustomerSubLedgerAsync(trade.CustomerId, trade.PriceUnitId,
                sell ? LedgerAccountRole.Receivable : LedgerAccountRole.Payable, ct)
            : await GetAdvanceLedgerAsync(trade, ct);
        var groupId = await PostJournalAsync(trade, sell ? accountLedger : advance.Id,
            sell ? advance.Id : accountLedger, request.Amount, "دریافت یا پرداخت وجه معامله آب‌شده", ct);
        await trades.AddEventAsync(MoltenGoldTradeEvent.Create(request.OperationId, trade, MoltenGoldTradeEventKind.Payment,
            request.Amount, reference: CleanReference(request.Reference), journalGroupId: groupId), ct);
        if (!trade.IsPaper)
            await AllocateAdvancesAsync(trade, ct);
    }

    private async Task PostTransferAsync(MoltenGoldTrade trade, TransferMoltenGold request, CancellationToken ct)
    {
        var state = await LoadStateAsync(trade, ct);
        ValidateAmount(request.Amount, state.Remaining);
        if (request.OtherCustomerId == trade.CustomerId.Value ||
            !await customers.ExistsAsync(new CustomersByIdSpecification(new(request.OtherCustomerId)), ct))
            throw Reject("طرف دیگر حواله را انتخاب کنید.");

        var sell = trade.Side == InvoiceType.Sell;
        MoltenGoldTrade? otherTrade = null;
        if (request.OtherTradeId is Guid otherId)
        {
            if (request.OtherInvoiceId.HasValue)
                throw Reject("فقط یک فاکتور یا معاملهٔ طرف دیگر را انتخاب کنید.");
            otherTrade = await GetTradeAsync(otherId, ct);
            if (!otherTrade.IsPaper || otherTrade.IsCancelled || otherTrade.Side == trade.Side ||
                otherTrade.CustomerId.Value != request.OtherCustomerId || otherTrade.PriceUnitId != trade.PriceUnitId)
                throw Reject("معاملهٔ طرف دیگر برای این حواله معتبر نیست.");
            ValidateAmount(request.Amount, (await LoadStateAsync(otherTrade, ct)).Remaining);
        }
        if (trade.IsPaper && request.InvoiceId != Guid.Empty)
            throw Reject("وجه این معامله در حساب تومانی ثبت می‌شود.");
        var beforeDelivery = !trade.IsPaper && request.InvoiceId == Guid.Empty;
        if (beforeDelivery)
        {
            if (trade.RemainingWeight == 0)
                throw Reject("ماندهٔ طلای قابل تحویل وجود ندارد.");
            ValidateAmount(request.Amount, trade.Value(trade.RemainingWeight) - state.Unallocated);
        }
        else if (!trade.IsPaper && !state.Invoices.ContainsKey(new InvoiceId(request.InvoiceId)))
        {
            throw Reject("فاکتور انتخابی متعلق به این معامله نیست.");
        }
        else if (!trade.IsPaper)
        {
            ValidateAmount(request.Amount, state.Invoices[new InvoiceId(request.InvoiceId)].TotalUnpaidAmount);
        }

        if (request.OtherInvoiceId is Guid otherInvoiceId)
        {
            var other = await GetInvoiceAsync(new(otherInvoiceId), ct);
            if (other.CustomerId.Value != request.OtherCustomerId || other.PriceUnitId != trade.PriceUnitId ||
                other.InvoiceType != (sell ? InvoiceType.Purchase : InvoiceType.Sell))
                throw Reject("فاکتور طرف دیگر از نظر طرف‌حساب، واحد یا جهت حواله معتبر نیست.");
            ValidateAmount(request.Amount, other.TotalUnpaidAmount);
        }
        else if (otherTrade is null)
        {
            var balances = await journal.GetCustomerRemainingListAsync(new CustomerId(request.OtherCustomerId),
                trade.PriceUnitId, cancellationToken: ct);
            var balance = balances.Values.SingleOrDefault();
            ValidateAmount(request.Amount, sell ? -balance : balance);
        }

        Guid? deskInvoiceId = beforeDelivery || trade.IsPaper ? null : request.InvoiceId;
        var voucher = await transfers.PostAsync(new()
        {
            TransferDate = DateOnly.FromDateTime(DateTime.Now),
            SourceCustomerId = sell ? trade.CustomerId.Value : request.OtherCustomerId,
            DestinationCustomerId = sell ? request.OtherCustomerId : trade.CustomerId.Value,
            SourceInvoiceId = sell ? deskInvoiceId : request.OtherInvoiceId,
            DestinationInvoiceId = sell ? request.OtherInvoiceId : deskInvoiceId,
            PriceUnitId = trade.PriceUnitId.Value, ExchangeRate = trade.ExchangeRate, Amount = request.Amount,
            Description = CleanReference(request.Reference) ?? "حواله دفتر آب‌شده"
        }, ct, isMoltenGoldDesk: true);

        Guid? group = null;
        if (trade.IsPaper && !sell)
        {
            var payable = await customerLedgers.GetOrCreateCustomerSubLedgerAsync(trade.CustomerId, trade.PriceUnitId,
                LedgerAccountRole.Payable, ct);
            var receivable = await customerLedgers.GetOrCreateCustomerSubLedgerAsync(trade.CustomerId, trade.PriceUnitId,
                LedgerAccountRole.Receivable, ct);
            group = await PostJournalAsync(trade, payable.Id, receivable.Id, request.Amount, "تخصیص حواله به حساب پرداختنی آب‌شدهٔ کاغذی", ct);
        }
        else if (beforeDelivery)
        {
            var advance = await GetAdvanceLedgerAsync(trade, ct);
            // Standalone vouchers book both legs against customer subledgers.
            var customer = await customerLedgers.GetOrCreateCustomerSubLedgerAsync(trade.CustomerId, trade.PriceUnitId,
                LedgerAccountRole.Receivable, ct);
            group = await PostJournalAsync(trade, sell ? customer.Id : advance.Id,
                sell ? advance.Id : customer.Id, request.Amount, "انتقال حواله به پیش‌پرداخت یا پیش‌دریافت آب‌شده", ct);
        }

        await trades.AddEventAsync(MoltenGoldTradeEvent.Create(request.OperationId, trade, MoltenGoldTradeEventKind.Transfer,
            request.Amount, invoiceId: deskInvoiceId.HasValue ? new InvoiceId(deskInvoiceId.Value) : null,
            voucherId: voucher.Id, reference: CleanReference(request.Reference), journalGroupId: group), ct);
        if (otherTrade is not null)
        {
            if (otherTrade.Side == InvoiceType.Purchase)
            {
                var payable = await customerLedgers.GetOrCreateCustomerSubLedgerAsync(otherTrade.CustomerId, otherTrade.PriceUnitId,
                    LedgerAccountRole.Payable, ct);
                var receivable = await customerLedgers.GetOrCreateCustomerSubLedgerAsync(otherTrade.CustomerId, otherTrade.PriceUnitId,
                    LedgerAccountRole.Receivable, ct);
                await PostJournalAsync(otherTrade, payable.Id, receivable.Id, request.Amount, "تخصیص حواله به حساب پرداختنی آب‌شدهٔ کاغذی", ct);
            }
            await trades.AddEventAsync(MoltenGoldTradeEvent.Create(Guid.NewGuid(), otherTrade, MoltenGoldTradeEventKind.Transfer,
                request.Amount, voucherId: voucher.Id, reference: CleanReference(request.Reference)), ct);
        }
    }

    private async Task AllocateAdvancesAsync(MoltenGoldTrade trade, CancellationToken ct)
    {
        var state = await LoadStateAsync(trade, ct);
        var unallocated = state.Unallocated;
        if (unallocated <= 0)
            return;

        var sell = trade.Side == InvoiceType.Sell;
        var advance = await GetAdvanceLedgerAsync(trade, ct);
        var customer = await customerLedgers.GetOrCreateCustomerSubLedgerAsync(trade.CustomerId, trade.PriceUnitId,
            sell ? LedgerAccountRole.Receivable : LedgerAccountRole.Payable, ct);
        foreach (var invoice in state.Invoices.Values.OrderBy(x => x.CreatedAt))
        {
            var amount = Math.Min(invoice.TotalUnpaidAmount, unallocated);
            if (amount <= 0)
                continue;

            var payment = InvoicePayment.Create(DateTime.Now, PaymentType.MoltenGoldDesk,
                sell ? PaymentSide.Receive : PaymentSide.Pay, amount, null, null, invoice.Id, trade.PriceUnitId,
                null, advance.Id, null, null, null, note: "تسویه از دفتر آب‌شده", storeId: trade.StoreId);
            await payments.CreateAsync(payment, ct);
            await PostJournalAsync(trade, sell ? advance.Id : customer.Id,
                sell ? customer.Id : advance.Id, amount, "تخصیص وجه دفتر آب‌شده به فاکتور", ct, payment);
            unallocated -= amount;
            if (unallocated <= 0)
                break;
        }
    }

    private async Task<Guid> PostJournalAsync(MoltenGoldTrade trade, LedgerAccountId debit, LedgerAccountId credit,
        decimal amount, string description, CancellationToken ct, InvoicePayment? payment = null)
    {
        var group = Guid.CreateVersion7();
        var date = DateTime.Now;
        Transaction CreateEntry(TransactionType type, LedgerAccountId ledger)
        {
            if (trade.IsPaper)
                return Transaction.CreateForMoltenGoldTrade(trade, description, amount, trade.ExchangeRate,
                    Math.Round(amount * trade.ExchangeRate, 10), group, type, ledger, trade.PriceUnitId);
            return payment is null
                ? Transaction.CreateForManualEntry(description, amount, trade.ExchangeRate, group, type, ledger,
                    trade.PriceUnitId, null, null, date, trade.StoreId)
                : Transaction.CreateForInvoicePayment(description, amount, trade.ExchangeRate, group, type, ledger,
                    trade.PriceUnitId, payment.InvoiceId, payment.Id, date, trade.StoreId);
        }
        await journal.CreateRangeAsync([CreateEntry(TransactionType.Debit, debit), CreateEntry(TransactionType.Credit, credit)], ct);
        return group;
    }

    private async Task<LedgerAccount> GetAdvanceLedgerAsync(MoltenGoldTrade trade, CancellationToken ct)
    {
        var sell = trade.Side == InvoiceType.Sell;
        var title = sell ? SystemLedgerAccounts.MoltenGoldCustomerAdvances : SystemLedgerAccounts.PrepaymentsToSuppliers;
        var ledger = await ledgers.Get(new LedgerAccountsByTitleSpecification(title)).FirstOrDefaultAsync(ct);
        if (ledger is not null)
            return ledger;

        var parent = await ledgers.Get(new LedgerAccountsByTitleSpecification(sell ? SystemLedgerAccounts.CurrentLiabilities
            : SystemLedgerAccounts.CurrentAssets)).FirstOrDefaultAsync(ct) ?? throw new NotFoundException();
        ledger = LedgerAccount.CreateSystemAccount(title, parent.AccountType, parent.Id, storeId: trade.StoreId);
        await ledgers.CreateAsync(ledger, ct);
        return ledger;
    }

    private async Task<Invoice> GetInvoiceAsync(InvoiceId id, CancellationToken ct)
        => await WithBalances(invoices.Get(new InvoicesByIdSpecification(id))).FirstOrDefaultAsync(ct) ?? throw new NotFoundException();

    private static IQueryable<Invoice> WithBalances(IQueryable<Invoice> query) => query
        .Include(x => x.InvoicePayments!).Include(x => x.ProductItems).Include(x => x.CoinItems)
        .Include(x => x.CurrencyItems).Include(x => x.UsedProducts).Include(x => x.Discounts)
        .Include(x => x.ExtraCosts).AsNoTracking().AsSplitQuery();

    private async Task<TradeState> LoadStateAsync(MoltenGoldTrade trade, CancellationToken ct)
    {
        var events = await trades.Events.AsNoTracking().Where(x => x.TradeId == trade.Id)
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToListAsync(ct);
        var posted = await GetDeliveryInvoicesAsync(events, ct);
        return BuildState(trade, events, posted);
    }

    private async Task<Dictionary<InvoiceId, Invoice>> GetDeliveryInvoicesAsync(List<MoltenGoldTradeEvent> events, CancellationToken ct)
    {
        var ids = events.Where(x => x.Kind == MoltenGoldTradeEventKind.Delivery).Select(x => x.InvoiceId!.Value).ToArray();
        return ids.Length == 0 ? [] : await WithBalances(invoices.Get(new InvoicesByIdsSpecification(ids)))
            .ToDictionaryAsync(x => x.Id, ct);
    }

    private static TradeState BuildState(MoltenGoldTrade trade, List<MoltenGoldTradeEvent> events, Dictionary<InvoiceId, Invoice> available)
    {
        var posted = events.Where(x => x.Kind == MoltenGoldTradeEventKind.Delivery)
            .ToDictionary(x => x.InvoiceId!.Value, x => available[x.InvoiceId!.Value]);
        var advances = events.Where(x => x.Kind == MoltenGoldTradeEventKind.Payment ||
            (x.Kind == MoltenGoldTradeEventKind.Transfer && x.InvoiceId == null)).Sum(x => x.Amount);
        if (trade.IsPaper)
            return new(events, posted, trade.Value(trade.Weight), trade.Value(trade.Weight) - advances, 0);
        var allocated = posted.Values.Sum(x => x.InvoicePayments?.Where(p => p.PaymentType == PaymentType.MoltenGoldDesk).Sum(p => p.Amount) ?? 0);
        var unallocated = advances - allocated;
        var pending = trade.Value(trade.RemainingWeight);
        var total = posted.Values.Sum(x => x.TotalAmountWithDiscountsAndExtraCosts) + pending;
        var remaining = posted.Values.Sum(x => x.TotalUnpaidAmount) + pending - unallocated;
        return new(events, posted, total, remaining, unallocated);
    }

    private async Task<Product> GetProductAsync(ProductId id, CancellationToken ct)
    {
        var product = await products.Get(new ProductsByIdSpecification(id)).FirstOrDefaultAsync(ct);
        return product is { ProductType: ProductType.MoltenGold, GoldUnitType: GoldUnitType.Gram }
            ? product : throw Reject("قطعهٔ آب‌شدهٔ گرمی معتبر نیست.");
    }

    private async Task<MoltenGoldTrade> GetTradeAsync(Guid id, CancellationToken ct)
        => await trades.Query.FirstOrDefaultAsync(x => x.Id == id, ct) ?? throw new NotFoundException();

    private void RequireStore()
    {
        if (storeContext.StoreId is null || storeContext.IsCustomDomain)
            throw new ForbiddenException("فروشگاه فعال برای دفتر آب‌شده مشخص نیست.");
    }

    private static ValidationException Reject(string message)
        => new([new ValidationFailure("MoltenGoldDesk", message)]);

    private static bool ValidWeight(decimal weight) => weight is > 0 and <= 1_000_000 && weight == Math.Round(weight, 3);
    private static string? CleanReference(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim()[..Math.Min(text.Trim().Length, 250)];
    private static void ValidateAmount(decimal amount, decimal limit)
    {
        if (amount <= 0 || amount > limit || amount != Math.Round(amount, 10))
            throw Reject("مبلغ از ماندهٔ قابل تسویه بیشتر است یا معتبر نیست.");
    }

    private record TradeState(List<MoltenGoldTradeEvent> Events, Dictionary<InvoiceId, Invoice> Invoices,
        decimal Total, decimal Remaining, decimal Unallocated);
}
