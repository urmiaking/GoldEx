using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
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

    protected override async Task OnParametersSetAsync()
    {
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
            afterSend: response =>
            {
                _financialAccounts = response;
                Model.FinancialAccount ??= _financialAccounts.FirstOrDefault() ?? null;
                StateHasChanged();
            });
    }

    public void Close() => MudDialog.Close();

    private async Task Submit()
    {
        _isProcessing = true;
        await _form.Validate();

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

        await LoadFinancialAccountsAsync();
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
}