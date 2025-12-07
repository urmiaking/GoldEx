using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

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
        TotalRemaining - GetSignedAmount(Model);

    private const decimal Epsilon = 0.0001m;

    private bool IsZeroRemaining => Math.Abs(TotalRemainingCalculated) < Epsilon;
    private bool IsCreditorRemaining => TotalRemainingCalculated < -Epsilon;
    private bool IsDebtorRemaining => TotalRemainingCalculated > Epsilon;

    private Color RemainingColor =>
        IsZeroRemaining ? Color.Info :
        IsCreditorRemaining ? Color.Success :
        Color.Error;

    private string RemainingLabel =>
        IsZeroRemaining
            ? "فاکتور تسویه شده"
            : IsCreditorRemaining
                ? "بستانکاری مشتری:"
                : "بدهی به مشتری:";
    private string FinancialAccountLabelText =>
        Model.PaymentSide switch
        {
            PaymentSide.Receive => "دریافت از حساب",
            PaymentSide.Pay => "پرداخت از حساب",
            _ => throw new ArgumentOutOfRangeException()
        };

    private static decimal GetSignedAmount(InvoicePaymentVm model)
    {
        var baseAmount = model.FinalAmount * (model.ExchangeRate ?? 1);
        return model.PaymentSide == PaymentSide.Receive ? baseAmount : -baseAmount;
    }

    private void Close() => Dialog.Cancel();

    protected override async Task OnParametersSetAsync()
    {
        if (Model.PaymentType is PaymentType.InternalCash && !Model.FinancialAccounts.Any())
            await LoadFinancialAccountsAsync();

        if (Model.PaymentType is PaymentType.MoltenGoldInventory or PaymentType.UsedGoldInventory && Model.PriceUnit?.Id != BasePriceUnit.Id)
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
    }

    private void ApplyTotalRemaining()
    {
        var remaining = TotalRemainingCalculated;

        if (Math.Abs(remaining) < Epsilon)
            return;

        Model.Amount = Math.Abs(remaining);
        Model.AmountAdornmentText = BasePriceUnit.Title;
        Model.PriceUnit = BasePriceUnit;
    }

    private async Task LoadExchangeRateAsync()
    {
        if (Model.PriceUnit is null)
            return;

        Model.ExchangeRateLabel = $"نرخ تبدیل هر {Model.PriceUnit.Title}";

        if (Model.PriceUnit.Id == BasePriceUnit.Id)
        {
            Model.ExchangeRate = 1;
            Model.ExchangeRateLabel = null;
            StateHasChanged();
            return;
        }

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(Model.PriceUnit.Id, BasePriceUnit.Id, ct),
            afterSend: response =>
            {
                if (response.ExchangeRate.HasValue)
                {
                    Model.ExchangeRate = response.ExchangeRate.Value;
                }
                StateHasChanged();
            });
    }

    #endregion


    #region FinancialAccounts

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
            Model.FinancialAccount = Model.FinancialAccounts.FirstOrDefault(x => x.Id == financialAccount.Id);
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
            afterSend: response =>
            {
                _customers = response;
            },
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

    #endregion
}