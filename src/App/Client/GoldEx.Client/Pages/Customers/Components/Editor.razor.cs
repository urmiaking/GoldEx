using GoldEx.Client.Pages.Customers.Validators;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class Editor
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;
    [Parameter] public CustomerVm Model { get; set; } = new();

    private readonly CustomerValidator _customerValidator = new();
    private MudForm _form = default!;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private string? _creditLimitAdornmentText;
    private bool _processing;

    protected override void OnParametersSet()
    {
        if (Model.Id.HasValue)
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
        if (_processing)
            return;

        _processing = true;

        await _form.Validate();

        if (!_form.IsValid)
            return;

        var request = CustomerVm.ToRequest(Model);

        if (!Model.Id.HasValue)
        {
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
            await SendRequestAsync<ICustomerService>(
                action:(s, ct) => s.UpdateAsync(Model.Id.Value, request, ct),
                afterSend: () =>
                {
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }

        _processing = false;
    }

    private void Close() => MudDialog.Cancel();

    private void OnCreditLimitChanged(decimal? creditLimit)
    {
        Model.CreditLimit = creditLimit;

        if (!string.IsNullOrEmpty(_creditLimitAdornmentText) && creditLimit.HasValue)
        {
            Model.CreditLimitPriceUnit = _priceUnits.FirstOrDefault(u => u.Title == _creditLimitAdornmentText);
        }
        else
        {
            Model.CreditLimitPriceUnit = null;
        }
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