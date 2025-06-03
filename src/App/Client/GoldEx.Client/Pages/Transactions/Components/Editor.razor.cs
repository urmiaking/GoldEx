using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.Transactions.Validators;
using GoldEx.Client.Pages.Transactions.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Transactions.Components;

public partial class Editor
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public Guid? Id { get; set; }
    [Parameter] public Guid? CustomerId { get; set; }

    private readonly TransactionEditorVm _model = new();
    private readonly TransactionValidator _transactionValidator = new();
    private MudForm _form = default!;
    private bool _processing;
    private string? _customerCreditLimitHelperText;
    private string? _creditHelperText;
    private string? _creditRateHelperText;
    private string? _debitHelperText;
    private string? _debitRateHelperText;
    private bool _isDebitMenuOpen;
    private bool _isCreditMenuOpen;
    private bool _isCreditLimitMenuOpen;

    protected override async Task OnParametersSetAsync()
    {
        if (Id is not null)
            await LoadTransactionAsync(Id.Value);
        else
            await GetLastNumberAsync();

        if (CustomerId is not null)
            await LoadCustomerAsync(CustomerId.Value);

        await base.OnParametersSetAsync();
    }

    private async Task GetLastNumberAsync()
    {
        await SendRequestAsync<ITransactionService, GetTransactionNumberResponse>(
            action: (s, ct) => s.GetLastNumberAsync(ct),
            afterSend: response =>
            {
                _model.TransactionNumber = response.Number + 1;
            },
            createScope: true);
    }

    private async Task LoadCustomerAsync(Guid customerId)
    {
        await SendRequestAsync<ICustomerService, GetCustomerResponse>(
            action: (s, ct) => s.GetAsync(customerId, ct),
            afterSend: response =>
            {
                _model.SetCustomer(response);
                OnCustomerCreditLimitChanged(response.CreditLimit);
            });
    }

    private async Task LoadTransactionAsync(Guid id)
    {
        await SendRequestAsync<ITransactionService, GetTransactionResponse>(
            action: (s, ct) => s.GetAsync(id, ct),
            afterSend: response =>
            {
                _model.SetTransaction(response);
                LoadHelperTexts(response);
            });
    }

    private void LoadHelperTexts(GetTransactionResponse response)
    {
        OnCustomerCreditLimitChanged(response.Customer.CreditLimit);
        OnCreditChanged(response.Credit);
        OnDebitChanged(response.Debit);
    }

    private async Task OnSubmit()
    {
        if (_processing)
            return;

        await _form.Validate();

        if (!_form.IsValid)
            return;

        _processing = true;

        if (Id.HasValue)
        {
            var request = TransactionEditorVm.ToUpdateTransactionRequest(_model);
            await SendRequestAsync<ITransactionService>((s, ct) => s.UpdateAsync(Id.Value, request, ct));
        }
        else
        {
            var request = TransactionEditorVm.ToCreateTransactionRequest(_model);
            await SendRequestAsync<ITransactionService>((s, ct) => s.CreateAsync(request, ct));
        }

        _processing = false;
        MudDialog.Close(DialogResult.Ok(true));
    }

    private async Task OnCustomerNationalIdChanged(string nationalId)
    {
        _model.CustomerNationalId = nationalId;

        await SendRequestAsync<ICustomerService, GetCustomerResponse?>(
            action: (s, ct) => s.GetAsync(nationalId, ct),
            afterSend: response =>
            {
                if (response is null)
                    return;

                _model.SetCustomer(response);
                OnCustomerCreditLimitChanged(response.CreditLimit);
            });
    }

    private void OnDebitChanged(decimal? debit)
    {
        _model.Debit = debit;
        _debitHelperText = debit.HasValue || _model.DebitUnit is not null
            ? $"{debit.FormatNumber()} {_model.DebitUnit?.GetDisplayName()}".Trim()
            : string.Empty;

        _model.DebitEquivalent = debit is null || _model.DebitRate is null
            ? null
            : debit.Value * _model.DebitRate.Value;
    }

    private async Task OnDebitUnitChanged(UnitType? debitUnit)
    {
        _model.DebitUnit = debitUnit;
        _debitHelperText = _model.Debit.HasValue || debitUnit is not null
            ? $"{_model.Debit.FormatNumber()} {debitUnit?.GetDisplayName()}".Trim()
            : string.Empty;

        if (debitUnit.HasValue)
            await SendRequestAsync<IPriceService, GetPriceResponse?>(
                action: (s, ct) => s.GetAsync(debitUnit.Value, ct),
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
        _model.DebitRate = debitRate;
        _debitRateHelperText = debitRate.HasValue
            ? $"{debitRate.FormatNumber()} ریال"
            : string.Empty;

        _model.DebitEquivalent = debitRate is null || _model.Debit is null
            ? null
            : _model.Debit.Value * debitRate.Value;
    }

    private void OnCreditChanged(decimal? credit)
    {
        _model.Credit = credit;
        _creditHelperText = credit.HasValue || _model.CreditUnit is not null
            ? $"{credit.FormatNumber()} {_model.CreditUnit?.GetDisplayName()}".Trim()
            : string.Empty;

        _model.CreditEquivalent = credit is null || _model.CreditRate is null
            ? null
            : credit.Value * _model.CreditRate.Value;
    }

    private async Task OnCreditUnitChanged(UnitType? creditUnit)
    {
        _model.CreditUnit = creditUnit;
        _creditHelperText = _model.Credit.HasValue || creditUnit is not null
            ? $"{_model.Credit.FormatNumber()} {creditUnit?.GetDisplayName()}".Trim()
            : string.Empty;

        if (creditUnit.HasValue)
            await SendRequestAsync<IPriceService, GetPriceResponse?>(
                action: (s, ct) => s.GetAsync(creditUnit.Value, ct),
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
        _model.CreditRate = creditRate;
        _creditRateHelperText = creditRate.HasValue
            ? $"{creditRate.FormatNumber()} ریال"
            : string.Empty;

        _model.CreditEquivalent = creditRate is null || _model.Credit is null
            ? null
            : _model.Credit.Value * creditRate.Value;
    }

    private void OnCustomerCreditLimitChanged(decimal? creditLimit)
    {
        _model.CustomerCreditLimit = creditLimit;
        _customerCreditLimitHelperText = creditLimit.HasValue
            ? $"{creditLimit.FormatNumber()} {_model.CustomerCreditLimitUnit?.GetDisplayName()}".Trim()
            : "سقف اعتبار مشتری";
    }

    private void OnCustomerCreditLimitUnitChanged(UnitType? creditLimitUnit)
    {
        _model.CustomerCreditLimitUnit = creditLimitUnit;
        _customerCreditLimitHelperText = _model.CustomerCreditLimit.HasValue || creditLimitUnit is not null
            ? $"{_model.CustomerCreditLimit.FormatNumber()} {creditLimitUnit?.GetDisplayName()}".Trim()
            : "سقف اعتبار مشتری";
    }

    private void OnCustomerNationalIdCleared()
    {
        _model.CustomerNationalId = string.Empty;
        _model.CustomerId = null;
        _model.CustomerFullName = string.Empty;
        _model.CustomerPhoneNumber = string.Empty;
        _model.CustomerAddress = string.Empty;
        _model.CustomerCreditLimit = null;
        _model.CustomerCreditLimitUnit = null;
        _model.CustomerCreditRemaining = null;
        _model.CustomerCreditRemainingUnit = null;
    }

    private async Task SelectDebitUnit(UnitType selectedUnit)
    {
        _model.DebitUnit = selectedUnit;
        _isDebitMenuOpen = false;
        await OnDebitUnitChanged(selectedUnit);
    }

    private async Task SelectCreditUnit(UnitType selectedUnit)
    {
        _model.CreditUnit = selectedUnit;
        _isDebitMenuOpen = false;
        await OnCreditUnitChanged(selectedUnit);
    }

    private void SelectCreditLimitUnit(UnitType selectedUnit)
    {
        _model.CustomerCreditLimitUnit = selectedUnit;
        OnCustomerCreditLimitUnitChanged(selectedUnit);
        _isCreditLimitMenuOpen = false;
    }

    private void Close() => MudDialog.Cancel();
}