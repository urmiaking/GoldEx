using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.MoltenGold;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class MoltenGoldOperationDialog
{
    [CascadingParameter] private ResponsiveDialogSession Session { get; set; } = null!;
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = null!;
    [Inject] private ISnackbar Snackbar { get; set; } = null!;
    [Parameter] public MoltenGoldTradeResponse? Trade { get; set; }
    [Parameter] public MoltenGoldOperation Operation { get; set; }
    [Parameter] public InvoiceType Side { get; set; }
    [Parameter] public decimal LimitAmount { get; set; }
    [Parameter] public decimal LimitWeight { get; set; }
    [Parameter] public decimal Fineness { get; set; }
    [Parameter] public MoltenGoldDeskSettings Settings { get; set; } = null!;
    [Parameter] public List<GetFinancialAccountTitleResponse> Accounts { get; set; } = [];
    [Parameter] public PayMoltenGold? InitialPayment { get; set; }
    [Parameter] public MoltenGoldDeliveryDraft? InitialDelivery { get; set; }

    private readonly Guid _operationId = Guid.NewGuid();
    private MudForm _form = null!;
    private bool _saving;
    private decimal _amount;
    private Guid? _account;
    private Guid _deskInvoice;
    private CustomerVm? _other;
    private GetTinyInvoiceResponse? _otherInvoice;
    private MoltenGoldPaymentChoice? _otherPaperTrade;
    private MoltenGoldSettlementChoice? _goldSettlement;
    private MoltenGoldStockChoice? _stock;
    private string? _assay;
    private string? _reference;
    private TransferAllocation _allocation;
    private enum TransferAllocation { Balance, Invoice, PaperTrade }

    private InvoiceType TradeSide => Trade?.Side ?? Side;
    private decimal AvailableWeight => Trade?.RemainingWeight ?? LimitWeight;
    private decimal AvailableAmount => Trade?.RemainingAmount ?? LimitAmount;
    private bool IsPaper => Trade?.IsPaper ?? true;
    private decimal ExpectedFineness => Trade?.Fineness ?? Fineness;
    private bool HasInitialValue => InitialPayment is not null || InitialDelivery is not null;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        if (InitialPayment is not null)
            (_amount, _account, _reference) = (InitialPayment.Amount, InitialPayment.AccountId, InitialPayment.Reference);
        if (InitialDelivery is not null)
            (_amount, _assay, _stock) = (InitialDelivery.Weight, InitialDelivery.AssayNumber, InitialDelivery.Stock);
        else if (Trade is null && Operation == MoltenGoldOperation.Delivery)
            _amount = LimitWeight;
        _deskInvoice = Trade?.Events.FirstOrDefault(x => x.Kind == MoltenGoldTradeEventKind.Delivery && x.InvoiceRemaining > 0)?.InvoiceId ?? Guid.Empty;
    }

    private void Close() => Session.Cancel();

    private void ClearInitial()
    {
        if (Operation == MoltenGoldOperation.Payment)
            Session.Close(new PayMoltenGold(_operationId, Guid.Empty, 0, null));
        else
            Session.Close(new MoltenGoldDeliveryDraft(0, null, null));
    }

    private void UseBalance() => _amount = Operation switch
    {
        MoltenGoldOperation.Delivery => AvailableWeight,
        MoltenGoldOperation.GoldSettlement => Math.Min(Trade!.RemainingGoldWeight, _goldSettlement?.RemainingGoldWeight ?? Trade.RemainingGoldWeight),
        _ => AvailableAmount
    };

    private void SelectOther(CustomerVm? customer)
    {
        _other = customer;
        _otherInvoice = null;
        _otherPaperTrade = null;
    }

    private async Task<IEnumerable<CustomerVm>> SearchCustomersAsync(string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        var customers = await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            (s, _) => s.GetByNameAsync(text.Trim(), null, ct), createScope: true);
        return customers?.Select(CustomerVm.CreateFrom) ?? [];
    }

    private async Task AddOtherAsync()
    {
        var dialog = ResponsiveDialogs.Show<Customers.Components.Editor>("افزودن طرف‌حساب جدید", new()
        {
            [nameof(Customers.Components.Editor.ReturnModel)] = true,
            [nameof(Customers.Components.Editor.ShowFinancialAccounts)] = false
        });
        if (await dialog.Result is { Canceled: false, Data: CustomerVm customer }) SelectOther(customer);
    }

    private async Task<IEnumerable<MoltenGoldStockChoice>> SearchStockAsync(string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text)) return [];
        var items = await SendRequestAsync<IMoltenGoldTradeService, List<MoltenGoldStockChoice>>(
            (s, _) => s.SearchStockAsync(text, ct), createScope: true) ?? [];
        return items.Where(x => x.Fineness == ExpectedFineness);
    }

    private async Task<IEnumerable<MoltenGoldSettlementChoice>> SearchSettlementsAsync(string text, CancellationToken ct)
        => Trade is null ? [] : await SendRequestAsync<IMoltenGoldTradeService, List<MoltenGoldSettlementChoice>>(
            (s, _) => s.SearchSettlementsAsync(Trade.Id, text ?? "", ct), createScope: true) ?? [];

    private async Task<IEnumerable<GetTinyInvoiceResponse>> SearchInvoicesAsync(string text, CancellationToken ct)
    {
        if (_other?.Id is not Guid customer) return [];
        var expectedType = TradeSide == InvoiceType.Sell ? InvoiceType.Purchase : InvoiceType.Sell;
        var items = await SendRequestAsync<IInvoiceService, List<GetTinyInvoiceResponse>>(
            (s, _) => s.GetCustomerInvoicesAsync(customer, new RequestFilter(Take: 100, Search: text), ct), createScope: true);
        return items?.Where(x => x.InvoiceType == expectedType && x.Remaining > 0 && x.PriceUnit.Id == Settings.PriceUnitId) ?? [];
    }

    private async Task<IEnumerable<MoltenGoldPaymentChoice>> SearchPaymentTradesAsync(string text, CancellationToken ct)
    {
        if (Trade is null || _other?.Id is not Guid customer) return [];
        var items = await SendRequestAsync<IMoltenGoldTradeService, List<MoltenGoldPaymentChoice>>(
            (s, _) => s.SearchPaymentTradesAsync(Trade.Id, customer, ct), createScope: true) ?? [];
        return string.IsNullOrWhiteSpace(text) ? items : items.Where(x => PaymentTradeName(x).Contains(text.Trim(), StringComparison.CurrentCultureIgnoreCase));
    }

    private async Task SaveAsync()
    {
        if (_saving) return;
        await _form.ValidateAsync();
        if (!_form.IsValid) return;
        if ((Operation != MoltenGoldOperation.Cancel && _amount <= 0) ||
            (Operation == MoltenGoldOperation.Payment && (_account is null || _amount > AvailableAmount)) ||
            (Operation == MoltenGoldOperation.Delivery && (_amount > AvailableWeight ||
                (IsPaper && TradeSide == InvoiceType.Sell && (_stock is null || _amount > _stock.Available)))) ||
            (Operation == MoltenGoldOperation.GoldSettlement && _goldSettlement is null) ||
            (Operation == MoltenGoldOperation.Transfer && (_other?.Id is null ||
                (_allocation == TransferAllocation.Invoice && _otherInvoice is null) ||
                (_allocation == TransferAllocation.PaperTrade && _otherPaperTrade is null))))
        {
            Snackbar.Add("مقدار و اطلاعات لازم را کامل کنید و ماندهٔ قابل ثبت را در نظر بگیرید.", Severity.Warning);
            return;
        }
        if (Trade is null)
        {
            if (Operation == MoltenGoldOperation.Payment)
                Session.Close(new PayMoltenGold(_operationId, _account!.Value, _amount, _reference));
            else
                Session.Close(new MoltenGoldDeliveryDraft(_amount, _assay, _stock));
            return;
        }
        _saving = true;
        Session.SetBusy(true);
        try
        {
            await SendRequestAsync<IMoltenGoldTradeService>((s, ct) => Operation switch
            {
                MoltenGoldOperation.Delivery => s.DeliverAsync(Trade.Id, new(_operationId, _amount, _assay, _stock?.Id), ct),
                MoltenGoldOperation.Payment => s.PayAsync(Trade.Id, new(_operationId, _account!.Value, _amount, _reference), ct),
                MoltenGoldOperation.Transfer => s.TransferAsync(Trade.Id, new(_operationId, _deskInvoice, _other!.Id!.Value,
                    _allocation == TransferAllocation.Invoice ? _otherInvoice?.Id : null, _amount, _reference,
                    _allocation == TransferAllocation.PaperTrade ? _otherPaperTrade?.Id : null), ct),
                MoltenGoldOperation.GoldSettlement => s.SettleGoldAsync(Trade.Id, new(_operationId, _goldSettlement!.Id, _amount), ct),
                _ => s.CancelAsync(Trade.Id, ct)
            }, afterSend: () => { Session.Close(true); return Task.CompletedTask; }, ignoreCancellation: true);
        }
        finally { _saving = false; Session.SetBusy(false); }
    }

    private string AccountName(Guid? id) => Accounts.FirstOrDefault(x => x.Id == id)?.Title ?? "";
    private static string CustomerName(CustomerVm? customer) => customer?.FullName ?? "";
    private static string StockName(MoltenGoldStockChoice? stock) => stock is null ? "" : $"{stock.Name} · {stock.Barcode}";
    private static string SettlementName(MoltenGoldSettlementChoice? trade) => trade is null ? "" : $"{trade.CustomerName} · {trade.RemainingGoldWeight:0.###} گرم · نرخ {trade.GramRate:#,##0.##}";
    private static string InvoiceName(GetTinyInvoiceResponse? invoice) => invoice is null ? "" : $"فاکتور {invoice.InvoiceNumber} · {invoice.Remaining:#,##0.##} تومان";
    private static string PaymentTradeName(MoltenGoldPaymentChoice? trade) => trade is null ? "" : $"{trade.Date.ToShortDateString()} · {trade.RemainingAmount:#,##0.##} تومان";
}
