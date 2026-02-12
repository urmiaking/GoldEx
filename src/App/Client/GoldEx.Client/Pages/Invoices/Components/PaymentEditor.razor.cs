using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static MudBlazor.Colors;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class PaymentEditor
{
    [Parameter, EditorRequired] public InvoicePaymentVm Model { get; set; }
    [Parameter, EditorRequired] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; }
    [Parameter, EditorRequired] public decimal TotalRemaining { get; set; }
    [Parameter, EditorRequired] public GetPriceUnitTitleResponse BasePriceUnit { get; set; }
    [Parameter, EditorRequired] public InvoiceType InvoiceType { get; set; }
    [CascadingParameter] private IMudDialogInstance Dialog { get; set; } = default!;

    private List<GetCustomerResponse>? _customers;
    private List<GetTinyInvoiceResponse>? _invoices;
    private MudForm _form = default!;

    private readonly InvoicePaymentValidator _paymentValidator = new();
    private readonly DialogOptions _dialogOptions = new()
    {
        CloseButton = true,
        FullWidth = true,
        FullScreen = false,
        MaxWidth = MaxWidth.Small
    };

    private decimal TotalRemainingCalculated =>
        TotalRemaining - GetBalanceEffect(InvoiceType, Model);

    private const decimal Epsilon = 0.0001m;

    private bool IsZeroRemaining => Math.Abs(TotalRemainingCalculated) < Epsilon;

    private Color RemainingColor =>
        IsZeroRemaining
            ? Color.Info
            : InvoiceType switch
            {
                InvoiceType.Sell => TotalRemainingCalculated < 0
                    ? Color.Success   // بستانکاری مشتری (ما به او بدهکاریم)
                    : Color.Error,    // بدهکاری مشتری (او به ما بدهکار است)

                InvoiceType.Purchase => TotalRemainingCalculated > 0
                    ? Color.Success   // بستانکاری فروشنده (ما به او بدهکاریم)
                    : Color.Error,    // بدهکاری فروشنده (او به ما بدهکار است)

                _ => Color.Info
            };

    private string RemainingLabel =>
        IsZeroRemaining
            ? "فاکتور تسویه شده"
            : InvoiceType switch
            {
                InvoiceType.Sell => TotalRemainingCalculated > 0
                    ? "بدهی مشتری:"
                    : "بستانکاری مشتری:",

                InvoiceType.Purchase => TotalRemainingCalculated > 0
                    ? "بدهی ما به فروشنده:"
                    : "بستانکاری ما از فروشنده:",

                _ => "مانده:"
            };

    private string FinancialAccountLabelText =>
        Model.PaymentSide switch
        {
            PaymentSide.Receive => "واریز به حساب",
            PaymentSide.Pay => "پرداخت از حساب",
            _ => throw new ArgumentOutOfRangeException()
        };

    private static decimal GetBalanceEffect(InvoiceType invoiceType, InvoicePaymentVm model)
    {
        var baseAmount = model.FinalAmount * (model.ExchangeRate ?? 1);

        if (invoiceType == InvoiceType.Purchase)
        {
            return model.PaymentSide == PaymentSide.Pay ? baseAmount : -baseAmount;
        }

        return model.PaymentSide == PaymentSide.Receive ? baseAmount : -baseAmount;
    }

    private void Close() => Dialog.Cancel();

    protected override async Task OnParametersSetAsync()
    {
        if (Model.PaymentType is PaymentType.InternalCash && Model.FinancialAccounts == null)
            await LoadFinancialAccountsAsync();

        if (Model.PaymentType is PaymentType.MoltenGoldInventory or PaymentType.UsedGoldInventory && Model.PriceUnit?.Id != BasePriceUnit.Id && !Model.Id.HasValue)
            await LoadExchangeRateAsync();

        await base.OnParametersSetAsync();
    }

    private async Task Submit()
    {
        await _form.Validate();

        if (!_form.IsValid)
        {
            return;
        }

        Dialog.Close(DialogResult.Ok(Model));
    }

    #region Amounts

    private async Task SelectPriceUnit(GetPriceUnitTitleResponse priceUnit)
    {
        Model.PriceUnit = priceUnit;
        Model.AmountAdornmentText = priceUnit.Title;

        await LoadExchangeRateAsync();

        if (Model.PaymentType is PaymentType.InternalCash)
            await LoadFinancialAccountsAsync();

        if (Model.TargetInvoice != null) 
            Model.TargetInvoice = null;
    }

    private void ApplyTotalRemaining()
    {
        var remaining = TotalRemainingCalculated;

        if (Math.Abs(remaining) < Epsilon)
            return;

        var baseEffectNeeded = TotalRemaining;
        var exchangeRate = Model.ExchangeRate ?? 1m;

        var amount = Math.Abs(baseEffectNeeded / exchangeRate);
        if (amount <= 0)
            return;

        Model.Amount = amount;
        Model.AmountAdornmentText = BasePriceUnit.Title;
        Model.PriceUnit = BasePriceUnit;
    }

    private async Task LoadExchangeRateAsync()
    {
        if (Model.PriceUnit is null)
            return;

        if (Model.PriceUnit.Id == BasePriceUnit.Id)
        {
            Model.ExchangeRate = 1;
            StateHasChanged();
            return;
        }

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(
                Model.PriceUnit.Id,
                BasePriceUnit.Id,
                ct),
            afterSend: response =>
            {
                Model.ExchangeRate = response.ExchangeRate;
                StateHasChanged();
            });
    }

    #endregion

    #region FinancialAccounts

    private async Task<IEnumerable<GetFinancialAccountTitleResponse?>> SearchFinancialAccountsAsync(string value, CancellationToken token)
    {
        // Ensure accounts are loaded  
        if (Model.FinancialAccounts == null)
        {
            await LoadFinancialAccountsAsync();
        }

        // Return all accounts if search is empty, otherwise filter  
        return (string.IsNullOrEmpty(value)
            ? Model.FinancialAccounts
            : Model.FinancialAccounts?.Where(acc => acc.Title.Contains(value,
                StringComparison.InvariantCultureIgnoreCase))) ?? [];
    }

    private async Task LoadFinancialAccountsAsync()
    {
        await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(null, Model.PriceUnit?.Id ?? BasePriceUnit.Id, ct),
            afterSend: response => Model.FinancialAccounts = response);
    }

    private async Task OnAddFinancialAccount()
    {
        var parameters = new DialogParameters<FinancialAccountEditor>
        {
            { x => x.PriceUnits, PriceUnits },
            { x => x.IsSystemAccount, true },
            { x => x.SubmitIndependently, true }
        };

        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>("افزودن حساب مالی جدید",
            parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: FinancialAccountVm financialAccount })
        {
            await LoadFinancialAccountsAsync();
            Model.FinancialAccount = Model.FinancialAccounts?.FirstOrDefault(x => x.Id == financialAccount.Id);
            StateHasChanged();
        }
    }

    #endregion

    #region Customer

    private async Task<IEnumerable<CustomerVm>?> SearchCustomers(string? customerName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(customerName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            action: (s, ct) => s.GetByNameAsync(customerName, null, ct),
            afterSend: response => _customers = response,
            cancelPrevious: true);

        return _customers?.Select(CustomerVm.CreateFrom);
    }

    private async Task OnAddCustomer()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var parameters = new DialogParameters<Customers.Components.Editor>
        {
            { x => x.ReturnModel, true }
        };

        var dialog = await DialogService.ShowAsync<Customers.Components.Editor>("افزودن طرف حساب جدید", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            Model.Endorser = customerVm;
            StateHasChanged();
        }
    }

    private async Task<IEnumerable<GetTinyInvoiceResponse>?> SearchCustomerInvoicesAsync(string? value, CancellationToken cancellationToken = default)
    {
        if (_invoices is null) 
            await LoadCustomerInvoicesAsync();

        if (string.IsNullOrEmpty(value))
            return _invoices;

        return _invoices?.Where(inv => inv.InvoiceNumber.ToString().StartsWith(value));
    }

    private async Task LoadCustomerInvoicesAsync()
    {
        var request = new RequestFilter();

        if (Model.Endorser?.Id is null)
            return;

        await SendRequestAsync<IInvoiceService, List<GetTinyInvoiceResponse>>(
            action: (s, ct) => s.GetCustomerInvoicesAsync(Model.Endorser.Id.Value, request, ct),
            afterSend: response => _invoices = response,
            cancelPrevious: true);
    }

    private void OnEndorserChanged(CustomerVm endorser)
    {
        Model.Endorser = endorser;
        Model.TargetInvoice = null;

        _invoices = null;
    }

    #endregion

    private string? InvoiceToStringFunc(GetTinyInvoiceResponse? inv)
    {
        return inv is not null
            ? $"فاکتور {inv.InvoiceType.GetDisplayName()} " +
              $"شماره {inv.InvoiceNumber.ToString()} " +
              $"- مانده : {inv.Remaining.ToCurrencyFormat(inv.PriceUnit.Title)}"
            : null;
    }
}