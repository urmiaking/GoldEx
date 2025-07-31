using GoldEx.Client.Pages.Customers.Components;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.PaymentVouchers.Validators;
using GoldEx.Client.Pages.PaymentVouchers.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Xml.Linq;

namespace GoldEx.Client.Pages.PaymentVouchers.Components;

public partial class Editor
{
    [Parameter] public Guid? Id { get; set; }
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private readonly PaymentVoucherValidator _validator = new();
    private PaymentVoucherVm _model = new();
    private MudForm _form = default!;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private List<GetFinancialAccountTitleResponse> _sourceFinancialAccounts = [];
    private List<GetFinancialAccountTitleResponse> _destinationFinancialAccounts = [];
    private List<GetCustomerResponse> _suppliers = [];
    private bool _processing;
    private string? _searchString;

    protected override async Task OnParametersSetAsync()
    {
        if (Id.HasValue)
            await LoadPaymentVoucherAsync();
        else
            await LoadVoucherNumberAsync();

        await LoadPriceUnitsAsync();
        await LoadSourceFinancialAccountsAsync();

        await base.OnParametersSetAsync();
    }

    private async Task LoadVoucherNumberAsync()
    {
        await SendRequestAsync<IPaymentVoucherService, GetVoucherNumberResponse>(
            action: (s, ct) => s.GetLastNumberAsync(ct),
            afterSend: response => _model.VoucherNumber = response.VoucherNumber + 1);
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (service, token) => service.GetTitlesAsync(token),
            afterSend: response =>
            {
                _priceUnits = response;
                _model.PriceUnit ??= _priceUnits.FirstOrDefault(u => u.IsDefault);
                StateHasChanged();
            });
    }

    private async Task LoadPaymentVoucherAsync()
    {
        await SendRequestAsync<IPaymentVoucherService, GetPaymentVoucherResponse>(
            action: (service, token) => service.GetAsync(Id!.Value, token),
            afterSend: async response =>
            {
                _model = PaymentVoucherVm.CreateFrom(response);
                await LoadDestinationFinancialAccountsAsync();
                await LoadSourceFinancialAccountsAsync();
                StateHasChanged();
            });
    }

    private void Close() => MudDialog.Cancel();

    public async Task Submit()
    {
        if (_processing)
            return;

        await _form.Validate();

        if (!_form.IsValid)
            return;

        _processing = true;

        await SendRequestAsync<IPaymentVoucherService>(
            action: (service, token) => Id.HasValue
                ? service.UpdateAsync(Id.Value, PaymentVoucherVm.ToRequest(_model), token)
                : service.CreateAsync(PaymentVoucherVm.ToRequest(_model), token),
            afterSend: () =>
            {
                _processing = false;
                MudDialog.Close(DialogResult.Ok(true));
                return Task.CompletedTask;
            });
    }

    private async Task OnAddSupplier()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var parameters = new DialogParameters<Customers.Components.Editor>
        {
            { x => x.ReturnModel, true }
        };

        var dialog = await DialogService.ShowAsync<Customers.Components.Editor>("افزودن تامین کننده جدید", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
        {
            _model.Customer = customerVm;
            await LoadDestinationFinancialAccountsAsync();
            StateHasChanged();
        }
    }

    private async Task OnAddSourceFinancialAccount()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        var parameters = new DialogParameters<FinancialAccountEditor>
        {
            { x => x.SubmitIndependently, true },
            { x => x.PriceUnits, _priceUnits },
        };

        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>("افزودن حساب مالی جدید", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: FinancialAccountVm })
        {
            await LoadSourceFinancialAccountsAsync();
        }
    }

    private async Task OnPriceUnitChanged(GetPriceUnitTitleResponse? priceUnit)
    {
        _model.PriceUnit = priceUnit;

        await LoadSourceFinancialAccountsAsync();

        if (_model.Customer is not null)
            await LoadDestinationFinancialAccountsAsync();
    }

    private async Task LoadFinancialAccounts(Guid? customerId, Guid? priceUnitId, bool isSourceFinancialAccount)
    {
        await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountTitleResponse>>(
            action: (service, token) => service.GetTitlesAsync(customerId, priceUnitId, token),
            afterSend: response =>
            {
                if (isSourceFinancialAccount)
                {
                    _sourceFinancialAccounts = response;

                    if (!Id.HasValue) 
                        _model.SourceFinancialAccount = null;
                }
                else
                {
                    _destinationFinancialAccounts = response;

                    if (!Id.HasValue)
                        _model.DestinationFinancialAccount = null;
                }
                StateHasChanged();
            });
    }

    private async Task LoadSourceFinancialAccountsAsync() => await LoadFinancialAccounts(null, _model.PriceUnit?.Id, true);
    private async Task LoadDestinationFinancialAccountsAsync() => await LoadFinancialAccounts(_model.Customer?.Id, _model.PriceUnit?.Id, false);

    private async Task OnAddDestinationFinancialAccount()
    {
        DialogOptions dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Small };

        if (_model.Customer is null)
            return;

        var parameters = new DialogParameters
        {
            { nameof(FinancialAccountEditor.SubmitIndependently), true },
            { nameof(FinancialAccountEditor.PriceUnits), _priceUnits },
            { nameof(FinancialAccountEditor.CustomerId), _model.Customer.Id },
            { nameof(FinancialAccountEditor.AccountHolderName), _model.Customer.FullName }
        };

        var dialog = await DialogService.ShowAsync<FinancialAccountEditor>("افزودن حساب مالی جدید", parameters, dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: FinancialAccountVm })
        {
            await LoadDestinationFinancialAccountsAsync();
        }
    }

    private async Task<IEnumerable<CustomerVm>?> SearchSuppliers(string? supplierName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(supplierName))
            return null;

        await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            action: (s, ct) => s.GetByNameAsync(supplierName, ct),
            afterSend: response =>
            {
                _suppliers = response;
            });

        return _suppliers.Select(CustomerVm.CreateFrom);
    }

    private async Task OnSupplierChanged(CustomerVm supplier)
    {
        _model.Customer = supplier;

        await LoadDestinationFinancialAccountsAsync();
        _model.DestinationFinancialAccount = null;
    }
}
