using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.Customers.Validators;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
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
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private string? _creditLimitAdornmentText;

    protected override void OnParametersSet()
    {
        if (Id is not null)
            OnCreditLimitChanged(Model.CreditLimit);

        base.OnParametersSet();
    }

    protected override async Task OnParametersSetAsync()
    {
        await LoadPriceUnitsAsync();
        await base.OnParametersSetAsync();
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response =>
            {
                _priceUnits = response;

                if (Model.CreditLimitPriceUnit is null)
                {
                    var selectedUnit = _priceUnits.FirstOrDefault(u => u.IsDefault);
                    _creditLimitAdornmentText = selectedUnit?.Title;
                }
                else
                {
                    _creditLimitAdornmentText = Model.CreditLimitPriceUnit?.Title;
                }
            });
    }

    private async Task Submit()
    {
        if (IsBusy)
            return;

        await _form.Validate();

        if (!_form.IsValid)
            return;

        if (Id is null)
        {
            var request = CustomerVm.ToCreateRequest(Model);
            await SendRequestAsync<ICustomerService>(
                action:(s, ct) => s.CreateAsync(request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
        else
        {
            var request = CustomerVm.ToUpdateRequest(Model);
            await SendRequestAsync<ICustomerService>(
                action:(s, ct) => s.UpdateAsync(Model.Id, request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
    }

    private void Close() => MudDialog.Cancel();

    private void OnCreditLimitChanged(decimal? creditLimit)
    {
        Model.CreditLimit = creditLimit;
    }

    private void OnCreditLimitUnitChanged(GetPriceUnitTitleResponse? unitType)
    {
        Model.CreditLimitPriceUnit = unitType;
    }

    private void SelectCreditLimitUnit(GetPriceUnitTitleResponse selectedUnit)
    {
        OnCreditLimitUnitChanged(selectedUnit);

        _creditLimitAdornmentText = selectedUnit.Title;
        Model.CreditLimitMenuOpen = false;
    }
}