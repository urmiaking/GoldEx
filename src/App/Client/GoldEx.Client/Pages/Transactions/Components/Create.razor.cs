using GoldEx.Client.Pages.Transactions.Validators;
using GoldEx.Client.Pages.Transactions.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Transactions.Components;

public partial class Create
{
    private CreateTransactionVm _model = new();
    private readonly CreateTransactionValidator _createTransactionValidator = new();
    private MudForm _form = default!;
    private bool _processing;
    private string? _customerCreditLimitHelperText;
    private string? _creditHelperText;
    private string? _creditRateHelperText;
    private string? _debitHelperText;
    private string? _debitRateHelperText;

    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public Guid? CustomerId { get; set; }

    private ICustomerClientService CustomerService => GetRequiredService<ICustomerClientService>();
    private ITransactionClientService TransactionService => GetRequiredService<ITransactionClientService>();

    protected override async Task OnParametersSetAsync()
    {
        if (CustomerId.HasValue)
            await LoadCustomerAsync(CustomerId.Value);
        
        await base.OnParametersSetAsync();
    }

    private async Task LoadCustomerAsync(Guid customerId)
    {
        try
        {
            SetBusy();
            CancelToken();

            var response = await CustomerService.GetAsync(customerId, CancellationTokenSource.Token);

            if (response is not null)
                _model.SetCustomer(response);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            AddExceptionToast(e);
        }
        finally
        {
            SetIdeal();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await GetTransactionNumberAsync();
        await base.OnInitializedAsync();
    }

    private async Task GetTransactionNumberAsync()
    {
        try
        {
            SetBusy();
            CancelToken();

            var response = await TransactionService.GetLatestTransactionNumberAsync(CancellationTokenSource.Token);

            _model.TransactionNumber = response.Number + 1;
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            AddExceptionToast(e);
        }
        finally
        {
            SetIdeal();
        }
    }

    private async Task OnCustomerNationalIdChanged(string nationalId)
    {
        _model.CustomerNationalId = nationalId;

        try
        {
            SetBusy();
            CancelToken();

            var customer = await CustomerService.GetAsync(nationalId, CancellationTokenSource.Token);

            if (customer is not null)
            {
                _model.SetCustomer(customer);
                OnCustomerCreditLimitChanged(customer.CreditLimit);
            }
        }
        catch (Exception e)
        {
            AddExceptionToast(e);
        }
        finally
        {
            SetIdeal();
        }
    }

    private void OnCustomerCreditLimitChanged(double? creditLimit)
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

    private void OnCreditChanged(double? credit)
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

    private void OnCreditRateChanged(double? creditRate)
    {
        _model.CreditRate = creditRate;
        _creditRateHelperText = creditRate is null
            ? string.Empty
            : $"{creditRate:N0} ریال";

        _model.CreditEquivalent = creditRate is null || _model.Credit is null
            ? null
            : Math.Round(_model.Credit.Value * creditRate.Value, 2);
    }

    private void OnDebitChanged(double? debit)
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

    private void OnDebitRateChanged(double? debitRate)
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

    private async Task OnSubmit()
    {
        await _form.Validate();

        if (!_form.IsValid)
            return;

        try
        {
            if (_processing)
                return;

            SetBusy();
            CancelToken();
            _processing = true;

            if (_model.CustomerId is null)
            {
                var customerRequest = CreateTransactionVm.ToCreateCustomerRequest(_model);
                await CustomerService.CreateAsync(customerRequest, CancellationTokenSource.Token);
            }
            else
            {
                var customerRequest = CreateTransactionVm.ToUpdateCustomerRequest(_model);
                await CustomerService.UpdateAsync(_model.CustomerId.Value, customerRequest, CancellationTokenSource.Token);
            }

            var transactionRequest = CreateTransactionVm.ToCreateTransactionRequest(_model);

            await TransactionService.CreateAsync(transactionRequest, CancellationTokenSource.Token);

            MudDialog.Close(DialogResult.Ok(true));
        }
        catch (Exception e)
        {
            AddExceptionToast(e);
        }
        finally
        {
            SetIdeal();
            _processing = false;
        }
    }

    private void Close() => MudDialog.Cancel();
}