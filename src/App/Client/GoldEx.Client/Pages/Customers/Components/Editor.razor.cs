using GoldEx.Client.Pages.Customers.Validators;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class Editor
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public CustomerVm Model { get; set; } = CustomerVm.CreateDefaultInstance();
    [Parameter] public Guid? Id { get; set; }

    private readonly CustomerValidator _customerValidator = new();
    private MudForm _form = default!;
    private bool _processing;
    private string? _creditLimitHelperText;

    private async Task Submit()
    {
        if (_processing)
            return;

        await _form.Validate();

        if (!_form.IsValid)
            return;

        _processing = true;

        if (Id is null)
        {
            var request = CustomerVm.ToCreateRequest(Model);
            await SendRequestAsync<ICustomerService>((s, ct) => s.CreateAsync(request, ct));
        }
        else
        {
            var request = CustomerVm.ToUpdateRequest(Model);
            await SendRequestAsync<ICustomerService>((s, ct) => s.UpdateAsync(Model.Id, request, ct));
        }

        _processing = false;

        MudDialog.Close(DialogResult.Ok(true));
    }

    private void Close() => MudDialog.Cancel();

    private void OnCreditLimitChanged(decimal? creditLimit)
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