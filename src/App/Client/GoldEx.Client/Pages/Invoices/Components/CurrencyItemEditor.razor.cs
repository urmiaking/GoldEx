using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class CurrencyItemEditor
{
    [Parameter] public CurrencyItemVm Model { get; set; } = new();
    [Parameter] public GetPriceUnitTitleResponse? PriceUnit { get; set; }
    [Parameter] public InvoiceType InvoiceType { get; set; }
    [CascadingParameter] public IMudDialogInstance MudDialog { get; set; } = default!;
    public string FinancialAccountLabel => InvoiceType is InvoiceType.Purchase ? "واریز به حساب" : "واریز از حساب";

    private List<GetPriceUnitTitleResponse> _currencies = [];
    private MudForm _form = default!;
    private bool _isProcessing;
    private readonly CurrencyItemValidator _currencyItemValidator = new();
    private List<GetFinancialAccountTitleResponse> _financialAccounts = [];
    private decimal? _maxAvailableAmount;
    private GetFinancialAccountBalanceResponse? _financialAccountBalance;

    protected override async Task OnParametersSetAsync()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Model.Currency != null)
        {
            await LoadMaxAmountAsync(Model.Currency);
        }

        await LoadFinancialAccountBalanceAsync();

        await LoadCurrenciesAsync();
        await LoadFinancialAccountsAsync();
        await base.OnParametersSetAsync();
    }

    private async Task LoadCurrenciesAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response =>
            {
                _currencies = response;
                StateHasChanged();
            });
    }

    private async Task LoadFinancialAccountsAsync()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (Model.Currency == null)
        {
            return;
        }

        await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(null, Model.Currency.Id, ct),
            afterSend: async response =>
            {
                _financialAccounts = response;
                Model.FinancialAccount = _financialAccounts.FirstOrDefault() ?? null;
                await LoadFinancialAccountBalanceAsync();
                StateHasChanged();
            });
    }

    public void Close() => MudDialog.Close();

    private async Task Submit()
    {
        _isProcessing = true;
        await _form.ValidateAsync();

        if (!_form.IsValid)
        {
            _isProcessing = false;
            return;
        }

        _isProcessing = false;
        MudDialog.Close(DialogResult.Ok(Model));
    }

    private async Task OnCurrencyChanged(GetPriceUnitTitleResponse? currency)
    {
        if (currency is null)
            return;

        Model.Currency = currency;

        if (PriceUnit is null)
            return;

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(currency.Id, PriceUnit.Id, ct),
            afterSend: response =>
            {
                Model.UnitPrice = response.ExchangeRate ?? 0;

                StateHasChanged();
            });

        await LoadMaxAmountAsync(currency);

        await LoadFinancialAccountsAsync();
    }

    private async Task LoadMaxAmountAsync(GetPriceUnitTitleResponse currency)
    {
        await SendRequestAsync<IInventoryStockService, GetInventoryStockAmountResponse>(
            action: (s, ct) => s.GetAvailableItemAmountAsync(currency.Id, ItemType.Currency, ct),
            afterSend: response => _maxAvailableAmount = response.Amount);
    }

    private async Task LoadFinancialAccountBalanceAsync()
    {
        if (Model.FinancialAccount is null)
        {
            _financialAccountBalance = null;
            return;
        }

        await SendRequestAsync<ITransactionService, GetFinancialAccountBalanceResponse>(
            action: (s, ct) => s.GetFinancialAccountBalanceAsync(Model.FinancialAccount.Id, ct),
            afterSend: response =>
            {
                _financialAccountBalance = response;
            });
    }

    private async Task OnAddFinancialAccount()
    {
        DialogOptions dialogOptions = new()
        {
            CloseButton = true,
            FullWidth = true,
            FullScreen = false,
            MaxWidth = MaxWidth.Small
        };

        var parameters = new DialogParameters<FinancialAccountEditor>
        {
            { x => x.PriceUnits, _currencies },
            { x => x.PriceUnit, Model.Currency },
            { x => x.AccountType, FinancialAccountType.InternationalBankAccount },
            { x => x.IsSystemAccount, true },
            { x => x.SubmitIndependently, true }
        };

        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>("افزودن حساب مالی جدید",
            parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: FinancialAccountVm })
        {
            await LoadFinancialAccountsAsync();
            StateHasChanged();
        }
    }

    private decimal GetMaxAmount()
    {
        if (InvoiceType is InvoiceType.Purchase)
            return decimal.MaxValue;

        return _maxAvailableAmount ?? 0m;
    }

    private string GetAvailableFinancialAccountAmountHelperText()
    {
        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (_financialAccountBalance is not null && Model.Currency is not null)
        {
            return $"موجودی: {_financialAccountBalance.Amount.ToCurrencyFormat(Model.Currency.Title)}";
        }

        return "لطفا حساب مالی را انتخاب کنید";
    }

    private async Task OnFinancialAccountChanged(GetFinancialAccountTitleResponse? financialAccount)
    {
        Model.FinancialAccount = financialAccount;
        await LoadFinancialAccountBalanceAsync();
    }
}