using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Client.Pages.Finances.PaymentVouchers.Components;
using GoldEx.Shared.DTOs.Licenses;
using GoldEx.Shared.DTOs.PaymentVouchers;
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
    [Parameter] public Guid? CustomerId { get; set; }
    [CascadingParameter] private IMudDialogInstance Dialog { get; set; } = default!;

    [Inject] public ISettingService SettingService { get; set; } = default!;

    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        FullWidth = true,
        FullScreen = false,
        MaxWidth = MaxWidth.Medium
    };

    private GetSettingResponse? _settings;
    private GetLicenseResponse? _license;

    private decimal TotalReceiptsAmount =>
        Items.Where(p => p.PaymentSide == PaymentSide.Receive)
            .Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1));

    private decimal TotalPaymentsAmount =>
        Items.Where(p => p.PaymentSide == PaymentSide.Pay)
            .Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1));

    /// <summary>
    /// خالص پرداخت‌ها (Receive - Pay) بر اساس لیست فعلی Items.
    /// فقط برای فاکتور فروش استفاده می‌شود.
    /// </summary>
    private decimal TotalNetPayments => Items.Sum(GetSignedAmount);

    /// <summary>
    /// مجموع ساده پرداخت‌ها بر اساس لیست فعلی Items.
    /// برای فاکتور خرید استفاده می‌شود.
    /// </summary>
    private decimal TotalAbsolutePayments =>
        Items.Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1));

    /// <summary>
    /// مانده فعلی فاکتور داخل همین دیالوگ، بر اساس مانده اولیه و تغییرات روی Items.
    /// TotalRemaining = مانده‌ای که هنگام باز شدن دیالوگ از InvoiceVm آمده.
    /// </summary>
    private decimal CurrentRemaining => TotalRemaining;

    private decimal GetSignedAmount(InvoicePaymentVm p)
    {
        var baseAmount = p.FinalAmount * (p.ExchangeRate ?? 1);
        return p.PaymentSide == PaymentSide.Receive ? baseAmount : -baseAmount;
    }

    protected override async Task OnInitializedAsync()
    {
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

    private void Close() => Dialog.Close();

    private void Save() => Dialog.Close(Items);

    /// <summary>
    /// مانده قبل از اعمال پرداختی که در حال ویرایش است.
    /// CurrentRemaining = مانده بعد از اعمال همهٔ پرداخت‌های فعلی (از جمله همین payment).
    /// برای رسیدن به مانده قبل از این پرداخت، اثر خودش را برمی‌گردانیم.
    /// </summary>
    private decimal GetRemainingBefore(InvoicePaymentVm payment)
    {
        var baseAmount = payment.FinalAmount * (payment.ExchangeRate ?? 1);

        if (InvoiceType == InvoiceType.Sell)
        {
            var signed = GetSignedAmount(payment);
            // در فروش: InvoiceVm از NetPayments استفاده می‌کند.
            // TotalRemaining = مانده بعد از این پرداخت.
            // برای قبل از این پرداخت، باید اثر امضادار خودش را حذف کنیم.
            return TotalRemaining + signed;
        }

        // در خرید: InvoiceVm از مجموع ساده (TotalPaidAmount) استفاده می‌کند.
        // TotalRemaining = Amount - SumPaymentsIncludingThis
        // برای قبل از این پرداخت، باید مبلغ مثبتش را حذف کنیم.
        return TotalRemaining + baseAmount;
    }

    private async Task OnEdit(InvoicePaymentVm payment)
    {
        var parameters = new DialogParameters<PaymentEditor>
        {
            { x => x.Model, payment },
            { x => x.BasePriceUnit, PriceUnit },
            { x => x.PriceUnits, PriceUnits },
            { x => x.InvoiceType, InvoiceType },
            { x => x.TotalRemaining, GetRemainingBefore(payment) }
        };

        var dialog = await DialogService.ShowAsync<PaymentEditor>("ویرایش پرداخت", parameters, _dialogOptions);

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
        var result = await DialogService.ShowMessageBoxAsync("حذف پرداخت",
            $"آیا مطمئن هستید که می‌خواهید پرداخت با مبلغ {model.Amount.ToCurrencyFormat(model.PriceUnit?.Title)} را حذف کنید؟",
            yesText: "بله", noText: "خیر");

        if (result is true)
        {
            Items.Remove(model);
        }
    }

    private async Task OnAddPayment(PaymentType paymentType)
    {
        if (_license is null)
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

        var defaultSide = InvoiceType is InvoiceType.Sell
            ? PaymentSide.Receive
            : PaymentSide.Pay;

        var newPayment = new InvoicePaymentVm
        {
            PaymentType = paymentType,
            PriceUnit = priceUnit,
            AmountAdornmentText = adornmentText,
            GoldFineness = fineness,
            PaymentSide = defaultSide
        };

        var parameters = new DialogParameters<PaymentEditor>
        {
            { x => x.Model, newPayment },
            { x => x.BasePriceUnit, PriceUnit },
            { x => x.PriceUnits, PriceUnits },
            { x => x.InvoiceType, InvoiceType },
            { x => x.TotalRemaining, TotalRemaining }
        };

        var dialog = await DialogService.ShowAsync<PaymentEditor>(paymentType.GetDisplayTitle(), parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: InvoicePaymentVm addedPayment })
        {
            Items.Add(addedPayment);
        }
    }

    private async Task OnApplyPrepaymentVouchers()
    {
        if (!CustomerId.HasValue)
            return;

        var selectedVoucherIds = Items
            .Where(p => p.VoucherId.HasValue)
            .Select(p => p.VoucherId!.Value)
            .ToList();

        var parameters = new DialogParameters<PaymentVouchersSelectorList>
        {
            { x => x.CustomerId, CustomerId.Value },
            { x => x.SelectedPaymentVouchers, selectedVoucherIds }
        };

        var dialog = await DialogService.ShowAsync<PaymentVouchersSelectorList>(
            "انتخاب اسناد پیش‌پرداخت",
            parameters,
            _dialogOptions with { MaxWidth = MaxWidth.Small });

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: HashSet<GetPaymentVoucherResponse> selectedVouchers })
        {
            // 1. Remove any existing payment items that are linked to a voucher
            var existingVoucherPayments = Items.Where(p => p.VoucherId.HasValue).ToList();
            foreach (var payment in existingVoucherPayments)
            {
                Items.Remove(payment);
            }

            // 2. Add the newly selected vouchers as payments
            foreach (var voucher in selectedVouchers)
            {
                var voucherPriceUnit = PriceUnits.FirstOrDefault(pu => pu.Id == voucher.PriceUnit.Id);
                
                var paymentVm = new InvoicePaymentVm
                {
                    Amount = voucher.Amount,
                    PriceUnit = voucherPriceUnit,
                    PaymentType = PaymentType.InternalCash, // Prepayment acts as cash settled in advance
                    PaymentSide = PaymentSide.Pay,
                    PaymentDate = voucher.PaymentDate,
                    VoucherId = voucher.Id,
                    Note = $"اعمال سند پیش‌پرداخت شماره {voucher.VoucherNumber} - {voucher.Description}",
                    ExchangeRate = voucher.ExchangeRate,
                    AmountAdornmentText = voucherPriceUnit?.Title ?? string.Empty,
                    Disabled = true // Disable editing since it is linked to a voucher
                };

                Items.Add(paymentVm);
            }
            StateHasChanged();
        }
    }
}