using GoldEx.Client.Pages.Invoices.ViewModels;
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
    /// <summary>
    /// لیست پرداخت‌های فعلی فاکتور (ویرایش روی همین لیست انجام می‌شود).
    /// </summary>
    [Parameter] public List<InvoicePaymentVm> Items { get; set; } = new();

    [Parameter] public GetPriceUnitTitleResponse PriceUnit { get; set; } = default!;
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = new();
    [Parameter] public InvoiceType InvoiceType { get; set; }

    /// <summary>
    /// مانده فعلی فاکتور قبل از این‌که کاربر در این دیالوگ تغییری بدهد.
    /// این مقدار از InvoiceVm.TotalUnpaidAmount پر می‌شود.
    /// </summary>
    [Parameter] public decimal TotalRemaining { get; set; }

    [Parameter] public Guid? CustomerId { get; set; }

    [CascadingParameter] IMudDialogInstance Dialog { get; set; } = default!;

    [Inject] public ISettingService SettingService { get; set; } = default!;
    [Inject] public IDialogService DialogService { get; set; } = default!;

    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        FullWidth = true,
        FullScreen = false,
        MaxWidth = MaxWidth.Small
    };

    private GetSettingResponse? _settings;

    private decimal TotalReceiptsAmount =>
        Items.Where(p => p.PaymentSide == PaymentSide.Receive)
            .Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1));

    private decimal TotalPaymentsAmount =>
        Items.Where(p => p.PaymentSide == PaymentSide.Pay)
            .Sum(p => p.FinalAmount * (p.ExchangeRate ?? 1));

    private decimal GetSignedAmount(InvoicePaymentVm p)
    {
        var baseAmount = p.FinalAmount * (p.ExchangeRate ?? 1);
        return p.PaymentSide == PaymentSide.Receive ? baseAmount : -baseAmount;
    }

    /// <summary>
    /// خالص پرداخت‌ها (فقط برای محاسبات کمکی در همین دیالوگ).
    /// </summary>
    private decimal TotalNetPayments => Items.Sum(GetSignedAmount);

    protected override async Task OnInitializedAsync()
    {
        await LoadSettingsAsync();
        await base.OnInitializedAsync();
    }


    private async Task LoadSettingsAsync()
    {
        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response => _settings = response);
    }

    private void Close() => Dialog.Close();

    private void Save() => Dialog.Close(Items);

    /// <summary>
    /// مانده قبل از اعمال پرداختی که در حال ویرایش است.
    /// فرض: TotalRemaining = مانده فعلی فاکتور (بعد از همه پرداخت‌های ثبت‌شده).
    /// برای رسیدن به مانده قبل از این پرداخت، مقدار امضادار خودش را برمی‌گردانیم.
    /// </summary>
    private decimal GetRemainingBefore(InvoicePaymentVm payment)
    {
        var signed = GetSignedAmount(payment);
        return TotalRemaining + signed;
    }

    private async Task OnEdit(InvoicePaymentVm payment)
    {
        var parameters = new DialogParameters<PaymentEditor>
        {
            { x => x.Model, payment },
            { x => x.BasePriceUnit, PriceUnit },
            { x => x.PriceUnits, PriceUnits },
            { x => x.InvoiceType, InvoiceType },
            // مانده قبل از این پرداخت
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
        var result = await DialogService.ShowMessageBox("حذف پرداخت",
            $"آیا مطمئن هستید که می‌خواهید پرداخت با مبلغ {model.Amount.ToCurrencyFormat(model.PriceUnit?.Title)} را حذف کنید؟",
            yesText: "بله", noText: "خیر");

        if (result is true)
        {
            Items.Remove(model);
        }
    }

    private async Task OnAddPayment(PaymentType paymentType)
    {
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
            // برای پرداخت جدید، مانده فعلی فاکتور را به‌عنوان ورودی می‌دهیم
            { x => x.TotalRemaining, TotalRemaining }
        };

        var dialog = await DialogService.ShowAsync<PaymentEditor>(paymentType.GetDisplayTitle(), parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: InvoicePaymentVm addedPayment })
        {
            Items.Add(addedPayment);
        }
    }
}