using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Transactions.Validators;
using GoldEx.Client.Pages.Transactions.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Transactions.Components;

public partial class Editor
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public TransactionVm Model { get; set; } = new();
    [Parameter] public Guid? CustomerId { get; set; }

    private readonly TransactionValidator _transactionValidator = new();
    private MudForm _form = default!;
    private bool _isDebitMenuOpen;
    private bool _isCreditMenuOpen;
    private bool _isCreditLimitMenuOpen;
    private string? _creditLimitAdornmentText;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];

    protected override async Task OnParametersSetAsync()
    {
        if (Model.TransactionId.HasValue)
        {
            SetCreditLimitAdornmentText();
            OnDebitChanged(Model.Debit);
            OnCreditChanged(Model.Credit);
        }
        else
            await GetLastNumberAsync();

        if (CustomerId.HasValue)
            await LoadCustomerAsync();

        await LoadPriceUnitsAsync();

        await base.OnParametersSetAsync();
    }

    private async Task LoadCustomerAsync()
    {
        await SendRequestAsync<ICustomerService, GetCustomerResponse>(
            action: (s, ct) => s.GetAsync(Model.Customer.Id, ct),
            afterSend: response =>
            {
                Model.Customer = CustomerVm.CreateFrom(response);
                OnCustomerCreditLimitChanged(response.CreditLimit);
            });
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response =>
            {
                _priceUnits = response;

                if (Model.Customer.CreditLimitPriceUnit is null)
                {
                    var selectedUnit = _priceUnits.FirstOrDefault(u => u.IsDefault);
                    
                    OnCustomerCreditLimitUnitChanged(selectedUnit);
                }
            });
    }

    private async Task GetLastNumberAsync()
    {
        await SendRequestAsync<ITransactionService, GetTransactionNumberResponse>(
            action: (s, ct) => s.GetLastNumberAsync(ct),
            afterSend: response =>
            {
                Model.TransactionNumber = response.Number + 1;
            },
            createScope: true);
    }

    private async Task OnSubmit()
    {
        if (IsBusy)
            return;

        await _form.Validate();

        if (!_form.IsValid)
            return;

        if (Model.TransactionId.HasValue)
        {
            var request = TransactionVm.ToUpdateRequest(Model);
            await SendRequestAsync<ITransactionService>(
                action: (s, ct) => s.UpdateAsync(Model.TransactionId.Value, request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
        else
        {
            var request = TransactionVm.ToCreateRequest(Model);
            await SendRequestAsync<ITransactionService>(
                action: (s, ct) => s.CreateAsync(request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
    }

    private async Task OnCustomerNationalIdChanged(string nationalId)
    {
        Model.Customer.NationalId = nationalId;

        await SendRequestAsync<ICustomerService, GetCustomerResponse?>(
            action: (s, ct) => s.GetAsync(nationalId, ct),
            afterSend: response =>
            {
                if (response is null)
                    return;

                Model.Customer = CustomerVm.CreateFrom(response);
                OnCustomerCreditLimitChanged(response.CreditLimit);
            });
    }

    private void OnDebitChanged(decimal? debit)
    {
        Model.Debit = debit;

        Model.DebitEquivalent = debit is null || Model.DebitRate is null
            ? null
            : debit.Value * Model.DebitRate.Value;
    }

    private async Task OnDebitUnitChanged(GetPriceUnitTitleResponse? debitUnit)
    {
        Model.DebitUnit = debitUnit;

        if (debitUnit != null)
            await SendRequestAsync<IPriceService, GetPriceResponse?>(
                action: (s, ct) => s.GetAsync(debitUnit.Id, ct),
                afterSend: response =>
                {
                    if (response is not null)
                        OnDebitRateChanged(decimal.Parse(response.Value));
                    else
                        OnDebitRateChanged(null);
                });
    }

    private void OnDebitRateChanged(decimal? debitRate)
    {
        Model.DebitRate = debitRate;

        Model.DebitEquivalent = debitRate is null || Model.Debit is null
            ? null
            : Model.Debit.Value * debitRate.Value;
    }

    private void OnCreditChanged(decimal? credit)
    {
        Model.Credit = credit;

        Model.CreditEquivalent = credit is null || Model.CreditRate is null
            ? null
            : credit.Value * Model.CreditRate.Value;
    }

    private async Task OnCreditUnitChanged(GetPriceUnitTitleResponse? creditUnit)
    {
        Model.CreditUnit = creditUnit;

        if (creditUnit != null)
            await SendRequestAsync<IPriceService, GetPriceResponse?>(
                action: (s, ct) => s.GetAsync(creditUnit.Id, ct),
                afterSend: response =>
                {
                    if (response is not null)
                        OnCreditRateChanged(decimal.Parse(response.Value));
                    else
                        OnCreditRateChanged(null);
                });
    }

    private void OnCreditRateChanged(decimal? creditRate)
    {
        Model.CreditRate = creditRate;

        Model.CreditEquivalent = creditRate is null || Model.Credit is null
            ? null
            : Model.Credit.Value * creditRate.Value;
    }

    private void OnCustomerCreditLimitChanged(decimal? creditLimit)
    {
        Model.Customer.CreditLimit = creditLimit;
    }

    private void OnCustomerCreditLimitUnitChanged(GetPriceUnitTitleResponse? creditLimitUnit)
    {
        Model.Customer.CreditLimitPriceUnit = creditLimitUnit;
        SetCreditLimitAdornmentText();
    }

    private void OnCustomerNationalIdCleared()
    {
        Model.Customer = new CustomerVm();
    }

    private async Task SelectDebitUnit(GetPriceUnitTitleResponse selectedUnit)
    {
        _isDebitMenuOpen = false;
        await OnDebitUnitChanged(selectedUnit);
    }

    private async Task SelectCreditUnit(GetPriceUnitTitleResponse selectedUnit)
    {
        _isDebitMenuOpen = false;
        await OnCreditUnitChanged(selectedUnit);
    }

    private void SelectCreditLimitUnit(GetPriceUnitTitleResponse selectedUnit)
    {
        OnCustomerCreditLimitUnitChanged(selectedUnit);
        _isCreditLimitMenuOpen = false;
        SetCreditLimitAdornmentText();
    }

    private void SetCreditLimitAdornmentText()
    {
        _creditLimitAdornmentText = Model.Customer.CreditLimitPriceUnit?.Title;
        StateHasChanged();
    }

    private void Close() => MudDialog.Cancel();
}