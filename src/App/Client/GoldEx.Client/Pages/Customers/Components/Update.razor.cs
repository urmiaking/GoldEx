using GoldEx.Client.Pages.Customers.Validators;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class Update
{
    [Parameter] public CustomerVm Model { get; set; } = default!;

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

            var request = CustomerVm.ToUpdateRequest(Model);

            await CustomerService.UpdateAsync(Model.Id, request, CancellationTokenSource.Token);

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
        Model.CreditLimit = creditLimit;
        _creditLimitHelperText = $"{creditLimit:N0} {Model.CreditLimitUnit?.GetDisplayName()}";
    }

    private void OnCreditLimitUnitChanged(UnitType? unitType)
    {
        Model.CreditLimitUnit = unitType;
        _creditLimitHelperText = $"{Model.CreditLimit:N0} {unitType?.GetDisplayName()}";
    }
}