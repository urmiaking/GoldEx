using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Invoices.Validators;
using GoldEx.Client.Pages.Invoices.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Invoices.Components;

public partial class EditorForm
{
    [Parameter] public InvoiceVm Model { get; set; } = InvoiceVm.CreateDefaultInstance();

    private readonly InvoiceValidator _invoiceValidator = new();
    private MudForm _form = default!;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private bool _isCustomerCreditLimitMenuOpen;

    protected override async Task OnParametersSetAsync()
    {
        await LoadPriceUnitsAsync();
        await base.OnParametersSetAsync();
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (s, ct) => s.GetTitlesAsync(ct),
            afterSend: response => _priceUnits = response);
    }

    private void OnCustomerCreditLimitChanged(decimal? creditLimit)
    {
        Model.Customer.CreditLimit = creditLimit;
        Model.Customer.CreditLimitHelperText = $"{creditLimit.FormatNumber()} {Model.Customer.CreditLimitPriceUnit?.Title}".Trim();
    }

    private void OnCreditLimitUnitChanged(GetPriceUnitTitleResponse? unitType)
    {
        Model.Customer.CreditLimitPriceUnit = unitType;
        Model.Customer.CreditLimitHelperText = $"{Model.Customer.CreditLimit.FormatNumber()} {unitType?.Title}".Trim();
    }

    private void SelectCustomerCreditLimitUnit(GetPriceUnitTitleResponse selectedUnit)
    {
        OnCreditLimitUnitChanged(selectedUnit);
        Model.Customer.CreditLimitMenuOpen = false;
    }

    private async Task Submit()
    {
        return;
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
}