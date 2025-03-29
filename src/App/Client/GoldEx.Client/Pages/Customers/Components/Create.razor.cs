using GoldEx.Client.Pages.Customers.Validators;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class Create
{
    private readonly CustomerVm _model = CustomerVm.CreateDefaultInstance();

    private readonly CustomerValidator _customerValidator = new();
    private MudForm _form = default!;
    private bool _processing;
    private string? _creditLimitHelperText;
    private ICustomerClientService CustomerService => GetRequiredService<ICustomerClientService>();

    [CascadingParameter]
    private IMudDialogInstance MudDialog { get; set; } = default!;

    private async Task Submit()
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

            var request = CustomerVm.ToCreateRequest(_model);

            await CustomerService.CreateAsync(request);

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

    private void OnCreditLimitChanged(double? creditLimit)
    {
        _model.CreditLimit = creditLimit;
        _creditLimitHelperText = $"{creditLimit:N0} {_model.CreditLimitUnit?.GetDisplayName()}";
    }

    private void OnCreditLimitUnitChanged(UnitType? unitType)
    {
        _model.CreditLimitUnit = unitType;
        _creditLimitHelperText = $"{_model.CreditLimit:N0} {unitType?.GetDisplayName()}";
    }
}