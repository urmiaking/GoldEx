using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class PaymentDialog
{
    [Parameter] public List<InvoicePaymentVm> Items { get; set; } = [];
    [Parameter] public GetPriceUnitTitleResponse PriceUnit { get; set; } = default!;
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];
    [Parameter] public InvoiceType InvoiceType { get; set; }
    [Parameter] public decimal TotalRemaining { get; set; }
    [Parameter] public bool StartWithReview { get; set; }
    [CascadingParameter] private IMudDialogInstance Dialog { get; set; } = default!;
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = default!;

    [Inject] public ISettingService SettingService { get; set; } = default!;

    internal enum SettlementStep { Direction, Method, Review }
    private SettlementStep _step = SettlementStep.Direction;
    private PaymentSide? _selectedSide;
    private decimal _initialEffect;

    private static string GetSettlementTypeTitle(PaymentType type) =>
        type == PaymentType.InternalCash ? "تسویه نقدی" : type.GetDisplayTitle();

    private string ReceiveTitle => InvoiceType == InvoiceType.Purchase
        ? "از فروشنده دریافت می‌کنم" : "از مشتری دریافت می‌کنم";
    private string PayTitle => InvoiceType == InvoiceType.Purchase
        ? "به فروشنده پرداخت می‌کنم" : "به مشتری پرداخت می‌کنم";
    private string ReceiveDescription => InvoiceType == InvoiceType.Purchase
        ? "وجه، چک یا جنس را از فروشنده می‌گیرم؛ مانده حساب متناسب با دریافت اصلاح می‌شود."
        : "وجه، چک یا جنس را از مشتری می‌گیرم؛ بدهی مشتری کمتر می‌شود.";
    private string PayDescription => InvoiceType == InvoiceType.Purchase
        ? "بهای خرید را به فروشنده می‌دهم؛ مانده پرداختنی ما کمتر می‌شود."
        : "مبلغی را به مشتری برمی‌گردانم یا جنس تحویل می‌دهم؛ مانده حساب متناسب تغییر می‌کند.";

    private bool _isLeaving;
    private bool _changingStep;

    private async Task ChangeStepAsync(SettlementStep next)
    {
        if (_step == next || _changingStep) return;

        _changingStep = true;
        try
        {
            _isLeaving = true;
            StateHasChanged();
            await Task.Delay(140);
            _step = next;
            _isLeaving = false;
        }
        finally
        {
            _changingStep = false;
        }
    }

    private async Task SelectSide(PaymentSide side)
    {
        if (_changingStep) return;
        _selectedSide = side;
        await ChangeStepAsync(SettlementStep.Method);
    }

    private Task GoBackToDirection() => ChangeStepAsync(SettlementStep.Direction);
    private Task ShowReview() => ChangeStepAsync(SettlementStep.Review);

    private static decimal GetBalanceEffect(InvoiceType invoiceType, InvoicePaymentVm payment)
    {
        var amount = payment.FinalAmount * (payment.ExchangeRate ?? 1);
        return invoiceType == InvoiceType.Purchase
            ? (payment.PaymentSide == PaymentSide.Pay ? amount : -amount)
            : (payment.PaymentSide == PaymentSide.Receive ? amount : -amount);
    }

    private GetSettingResponse? _settings;
    private GetLicenseResponse? _license;

    private decimal TotalReceiptsAmount =>
        Items.Where(p => p.PaymentSide == PaymentSide.Receive)
            .Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1));

    private decimal TotalPaymentsAmount =>
        Items.Where(p => p.PaymentSide == PaymentSide.Pay)
            .Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1));

    // Current invoice balance is derived from the original effect and the draft
    // list, so successive entries in one wizard never use a stale amount.
    private decimal CurrentRemaining => TotalRemaining + _initialEffect
        - Items.Sum(p => GetBalanceEffect(InvoiceType, p));

    private string CurrentRemainingLabel => Math.Abs(CurrentRemaining) < 0.0001m
        ? "فاکتور تسویه شده"
        : InvoiceType == InvoiceType.Purchase
            ? CurrentRemaining > 0 ? "مانده بدهی ما به فروشنده" : "طلب ما از فروشنده"
            : CurrentRemaining > 0 ? "مانده بدهی مشتری" : "بستانکاری مشتری";

    protected override async Task OnInitializedAsync()
    {
        _initialEffect = Items.Sum(p => GetBalanceEffect(InvoiceType, p));
        _step = StartWithReview && Items.Count > 0 ? SettlementStep.Review : SettlementStep.Direction;
        await LoadSettingsAsync();
        await LoadLicenseAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadSettingsAsync()
    {
        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response => _settings = response);
    }

    private async Task LoadLicenseAsync()
    {
        await SendRequestAsync<ILicenseService, GetLicenseResponse>(
            action: (s, ct) => s.GetLicenseAsync(ct),
            afterSend: response => _license = response);
    }

    private void Close() { if (Session is not null) Session.Cancel(); else Dialog.Close(); }

    private void Save() { if (Session is not null) Session.Close(Items); else Dialog.Close(Items); }

    /// <summary>
    /// مانده قبل از اعمال پرداختی که در حال ویرایش است.
    /// CurrentRemaining = مانده بعد از اعمال همهٔ پرداخت‌های فعلی (از جمله همین payment).
    /// برای رسیدن به مانده قبل از این پرداخت، اثر خودش را برمی‌گردانیم.
    /// </summary>
    private decimal GetRemainingBefore(InvoicePaymentVm payment) =>
        CurrentRemaining + GetBalanceEffect(InvoiceType, payment);

    private async Task OnEdit(InvoicePaymentVm payment)
    {
        var dialog = ResponsiveDialogs.Show<PaymentEditor>("ویرایش پرداخت",
            new Dictionary<string, object>
            {
                [nameof(PaymentEditor.Model)] = payment,
                [nameof(PaymentEditor.BasePriceUnit)] = PriceUnit,
                [nameof(PaymentEditor.PriceUnits)] = PriceUnits,
                [nameof(PaymentEditor.InvoiceType)] = InvoiceType,
                [nameof(PaymentEditor.TotalRemaining)] = GetRemainingBefore(payment)
            }, wide: true);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: InvoicePaymentVm editedPayment })
        {
            var index = Items.IndexOf(payment);
            if (index != -1)
            {
                Items[index] = editedPayment;
            }
        }
    }

    private async Task OnDelete(InvoicePaymentVm model)
    {
        var dialog = ResponsiveDialogs.Show<InvoicePaymentDeleteDialog>("حذف پرداخت",
            new Dictionary<string, object>
            {
                [nameof(InvoicePaymentDeleteDialog.AmountText)] = model.Amount.ToCurrencyFormat(model.PriceUnit?.Title)
            });
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: true })
        {
            Items.Remove(model);
        }
    }

    private async Task OnAddPayment(PaymentType paymentType)
    {
        if (_selectedSide is null || _license is null)
            return;

        if (paymentType is PaymentType.CustomerTransfer && _license.IsExpired)
        {
            AddErrorToast("قابلیت پرداخت حواله ای در نسخه Premium قابل استفاده است");
            return;
        }

        var priceUnit = paymentType is PaymentType.MoltenGoldInventory or PaymentType.UsedGoldInventory
            ? PriceUnits.FirstOrDefault(pu => pu.IsGoldBased)
            : PriceUnit;

        var adornmentText = priceUnit?.Title ?? string.Empty;

        var fineness = paymentType switch
        {
            PaymentType.MoltenGoldInventory => 750m,
            PaymentType.UsedGoldInventory => 750m - _settings?.UsedGoldFinenessDeductionRate,
            _ => null
        };

        var defaultSide = _selectedSide.Value;

        var newPayment = new InvoicePaymentVm
        {
            PaymentType = paymentType,
            PriceUnit = priceUnit,
            AmountAdornmentText = adornmentText,
            GoldFineness = fineness,
            PaymentSide = defaultSide,
            CoinInstance = paymentType is PaymentType.Coin ? new CoinInstanceVm() : null,
            CoinQuantity = paymentType is PaymentType.Coin ? 1 : null
        };

        var dialog = ResponsiveDialogs.Show<PaymentEditor>(paymentType.GetDisplayTitle(),
            new Dictionary<string, object>
            {
                [nameof(PaymentEditor.Model)] = newPayment,
                [nameof(PaymentEditor.BasePriceUnit)] = PriceUnit,
                [nameof(PaymentEditor.PriceUnits)] = PriceUnits,
                [nameof(PaymentEditor.InvoiceType)] = InvoiceType,
                [nameof(PaymentEditor.TotalRemaining)] = CurrentRemaining,
                [nameof(PaymentEditor.LockPaymentSide)] = true
            }, wide: true);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: InvoicePaymentVm addedPayment })
        {
            Items.Add(addedPayment);
            await ShowReview();
        }
    }
}
