using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Transactions.Validators;
using GoldEx.Client.Pages.Transactions.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Settings;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Transactions.Components;

public partial class Editor
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public TransactionVm Model { get; set; } = new();
    [Parameter] public Guid? CustomerId { get; set; }
    private string CreditRateLabel => $"نرخ تبدیل {Model.CreditUnit?.Title} به {Model.PriceUnit?.Title}";
    private string DebitRateLabel => $"نرخ تبدیل {Model.DebitUnit?.Title} به {Model.PriceUnit?.Title}";
    private bool CreditRateShow => Model.PriceUnit?.Id != Model.CreditUnit?.Id;
    private bool DebitRateShow => Model.PriceUnit?.Id != Model.DebitUnit?.Id;

    private readonly TransactionValidator _transactionValidator = new();
    private MudForm _form = default!;
    private bool _isDebitMenuOpen;
    private bool _isCreditMenuOpen;
    private bool _isCreditLimitMenuOpen;
    private string? _creditLimitAdornmentText;
    private string? _creditAdornmentText;
    private string? _debitAdornmentText;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private GetSettingResponse? _setting;

    protected override async Task OnParametersSetAsync()
    {
        if (Model.TransactionId.HasValue)
        {
            SetCreditLimitAdornmentText();
            SetCreditAdornmentText();
            SetDebitAdornmentText();
            OnDebitChanged(Model.Debit);
            OnCreditChanged(Model.Credit);
        }
        else
            await GetLastNumberAsync();

        if (CustomerId.HasValue)
            await LoadCustomerAsync();

        await LoadPriceUnitsAsync();
        await LoadSettingsAsync();

        await base.OnParametersSetAsync();
    }

    private async Task LoadCustomerAsync()
    {
        if (!CustomerId.HasValue)
            return;

        await SendRequestAsync<ICustomerService, GetCustomerResponse>(
            action: (s, ct) => s.GetAsync(CustomerId.Value, ct),
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
            afterSend: async response =>
            {
                _priceUnits = response;

                Model.PriceUnit ??= _priceUnits.FirstOrDefault(u => u.IsDefault);

                if (Model.Customer.CreditLimitPriceUnit is null)
                    OnCustomerCreditLimitUnitChanged(_priceUnits.FirstOrDefault(u => u.IsDefault));

                if (Model.DebitUnit is null)
                    await OnDebitUnitChanged(_priceUnits.FirstOrDefault(u => u.IsDefault));

                if (Model.CreditUnit is null)
                    await OnCreditUnitChanged(_priceUnits.FirstOrDefault(u => u.IsDefault));
            });
    }

    private async Task LoadSettingsAsync()
    {
        await SendRequestAsync<ISettingService, GetSettingResponse?>(
            action: (s, ct) => s.GetAsync(ct),
            afterSend: response => _setting = response);
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
    
    private async Task OnCustomerPhoneNumberChanged(string phoneNumber)
    {
        Model.Customer.PhoneNumber = phoneNumber;

        await SendRequestAsync<ICustomerService, GetCustomerResponse?>(
            action: (s, ct) => s.GetByPhoneNumberAsync(phoneNumber, ct),
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
        SetDebitAdornmentText();

        if (Model.PriceUnit is null || debitUnit is null)
        {
            return;
        }

        if (Model.PriceUnit.Id == debitUnit.Id)
        {
            OnDebitChanged(null);
            return;
        }

        await GetExchangeRateAsync(debitUnit.Id, TransactionType.Debit);
    }

    private async Task GetExchangeRateAsync(Guid priceUnitId, TransactionType transactionType)
    {
        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(priceUnitId, Model.PriceUnit!.Id, ct),
            afterSend: response =>
            {
                switch (transactionType)
                {
                    case TransactionType.Credit:
                        OnCreditRateChanged(response.ExchangeRate);
                        break;
                    case TransactionType.Debit:
                        OnDebitRateChanged(response.ExchangeRate);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(transactionType), transactionType, null);
                }

                StateHasChanged();
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
        SetCreditAdornmentText();

        if (Model.PriceUnit is null || creditUnit is null)
            return;

        if (Model.PriceUnit.Id == creditUnit.Id)
        {
            OnCreditChanged(null);
            return;
        }

        await GetExchangeRateAsync(creditUnit.Id, TransactionType.Credit);
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

    private void OnCustomerCleared()
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

    private void SetCreditAdornmentText()
    {
        _creditAdornmentText = Model.CreditUnit?.Title;
        StateHasChanged();
    }

    private void SetDebitAdornmentText()
    {
        _debitAdornmentText = Model.DebitUnit?.Title;
        StateHasChanged();
    }


    private void Close() => MudDialog.Cancel();

    private async Task OnPriceUnitChanged(GetPriceUnitTitleResponse? priceUnit)
    {
        if (priceUnit is null)
            return;

        Model.PriceUnit = priceUnit;

        if (!Model.Customer.CreditLimit.HasValue)
            OnCustomerCreditLimitUnitChanged(priceUnit);

        if (!Model.Debit.HasValue)
            await OnDebitUnitChanged(priceUnit);

        if (!Model.Credit.HasValue)
            await OnCreditUnitChanged(priceUnit);

        if (Model.CreditUnit is not null && Model.PriceUnit.Id != Model.CreditUnit.Id)
        {
            await GetExchangeRateAsync(Model.CreditUnit.Id, TransactionType.Credit);
        }
        else
        {
            OnCreditRateChanged(null);
        }

        if (Model.DebitUnit is not null && Model.PriceUnit.Id != Model.DebitUnit.Id)
        {
            await GetExchangeRateAsync(Model.DebitUnit.Id, TransactionType.Debit);
        }
        else
        {
            OnDebitRateChanged(null);
        }
    }
}