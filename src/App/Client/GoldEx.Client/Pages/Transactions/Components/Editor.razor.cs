using GoldEx.Client.Pages.Transactions.Validators;
using GoldEx.Client.Pages.Transactions.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Customers;
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

    protected override async Task OnParametersSetAsync()
    {
        if (Id is not null)
            await LoadTransactionAsync(Id.Value);

        if (CustomerId is not null)
            await LoadCustomerAsync(CustomerId.Value);

        await base.OnParametersSetAsync();
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
            afterSend: _model.SetTransaction);
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

    private void OnCustomerCreditLimitChanged(decimal? creditLimit)
    {
        _model.CustomerCreditLimit = creditLimit;
        _customerCreditLimitHelperText = creditLimit is null
            ? "سقف اعتبار مشتری"
            : $"{creditLimit:N0} {_model.CustomerCreditLimitUnit?.GetDisplayName()}";
    }

    private void OnCustomerCreditLimitUnitChanged(UnitType? creditLimitUnit)
    {
        _model.CustomerCreditLimitUnit = creditLimitUnit;
        _customerCreditLimitHelperText = creditLimitUnit is null && _model.CustomerCreditLimit is null
            ? "سقف اعتبار مشتری"
            : $"{_model.CustomerCreditLimit:N0} {creditLimitUnit?.GetDisplayName()}";
    }

    private void OnCreditChanged(decimal? credit)
    {
        _model.Credit = credit;
        _creditHelperText = credit is null && _model.CreditUnit is null
            ? string.Empty
            : $"{credit:N0} {_model.CreditUnit?.GetDisplayName()}";
        _model.CreditEquivalent = credit is null || _model.CreditRate is null
            ? null
            : Math.Round(credit.Value * _model.CreditRate.Value, 2);
    }

    private void OnCreditUnitChanged(UnitType? creditUnit)
    {
        _model.CreditUnit = creditUnit;
        _creditHelperText = creditUnit is null && _model.Credit is null
            ? string.Empty
            : $"{_model.Credit:N0} {creditUnit?.GetDisplayName()}";
    }

    private void OnCreditRateChanged(decimal? creditRate)
    {
        _model.CreditRate = creditRate;
        _creditRateHelperText = creditRate is null
            ? string.Empty
            : $"{creditRate:N0} ریال";

        _model.CreditEquivalent = creditRate is null || _model.Credit is null
            ? null
            : Math.Round(_model.Credit.Value * creditRate.Value, 2);
    }

    private void OnDebitChanged(decimal? debit)
    {
        _model.Debit = debit;
        _debitHelperText = debit is null && _model.DebitUnit is null
            ? string.Empty
            : $"{debit:N0} {_model.DebitUnit?.GetDisplayName()}";
        _model.DebitEquivalent = debit is null || _model.DebitRate is null
            ? null
            : Math.Round(debit.Value * _model.DebitRate.Value, 2);
    }

    private void OnDebitUnitChanged(UnitType? debitUnit)
    {
        _model.DebitUnit = debitUnit;
        _debitHelperText = debitUnit is null && _model.Debit is null
            ? string.Empty
            : $"{_model.Debit:N0} {debitUnit?.GetDisplayName()}";
    }

    private void OnDebitRateChanged(decimal? debitRate)
    {
        _model.DebitRate = debitRate;
        _debitRateHelperText = debitRate is null
            ? string.Empty
            : $"{debitRate:N0} ریال";
        _model.DebitEquivalent = debitRate is null || _model.Debit is null
            ? null
            : Math.Round(_model.Debit.Value * debitRate.Value, 2);
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

    private void Close() => MudDialog.Cancel();
}