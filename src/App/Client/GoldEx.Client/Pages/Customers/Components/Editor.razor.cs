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
    private string? _creditLimitHelperText;
    private bool _isCreditLimitMenuOpen;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];

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
            });
    }

    private async Task Submit()
    {
        if (IsBusy)
            return;

        await _form.Validate();

        if (!_form.IsValid)
            return;

        bool result;

        if (Id is null)
        {
            var request = CustomerVm.ToCreateRequest(Model);
            result = await SendRequestAsync<ICustomerService>((s, ct) => s.CreateAsync(request, ct));
        }
        else
        {
            var request = CustomerVm.ToUpdateRequest(Model);
            result = await SendRequestAsync<ICustomerService>((s, ct) => s.UpdateAsync(Model.Id, request, ct));
        }

        if (result == false)
            return;

        MudDialog.Close(DialogResult.Ok(result));
    }

    private void Close() => MudDialog.Cancel();

    private void OnCreditLimitChanged(decimal? creditLimit)
    {
        Model.CreditLimit = creditLimit;
        _creditLimitHelperText = $"{creditLimit.FormatNumber()} {Model.CreditLimitPriceUnit?.Title}".Trim();
    }

    private void OnCreditLimitUnitChanged(GetPriceUnitTitleResponse? unitType)
    {
        Model.CreditLimitPriceUnit = unitType;
        _creditLimitHelperText = $"{Model.CreditLimit.FormatNumber()} {unitType?.Title}".Trim();
    }

    private void SelectCreditLimitUnit(GetPriceUnitTitleResponse selectedUnit)
    {
        OnCreditLimitUnitChanged(selectedUnit);
        _isCreditLimitMenuOpen = false;
    }
}