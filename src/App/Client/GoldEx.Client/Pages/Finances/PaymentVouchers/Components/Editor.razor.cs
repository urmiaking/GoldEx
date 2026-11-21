using GoldEx.Client.Components.Services;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Finances.PaymentVouchers.Validators;
using GoldEx.Client.Pages.Finances.PaymentVouchers.ViewModels;
using GoldEx.Client.Pages.FinancialAccounts.Components;
using GoldEx.Client.Pages.FinancialAccounts.ViewModels;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.FinancialAccounts;
using GoldEx.Shared.DTOs.PaymentVouchers;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.PaymentVouchers.Components;

public partial class Editor
{
    [Parameter] public Guid? Id { get; set; }
    [Inject] private ResponsiveDialogService ResponsiveDialogs { get; set; } = default!;
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    private readonly PaymentVoucherValidator _validator = new();
    private PaymentVoucherVm _model = new();
    private MudForm _form = default!;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private List<GetFinancialAccountTitleResponse> _sourceFinancialAccounts = [];
    private List<GetFinancialAccountTitleResponse> _destinationFinancialAccounts = [];
    private bool _loading = true;
    private bool _refreshingAccounts;
    private bool _processing;

    private GetPriceUnitTitleResponse? DefaultPriceUnit => _priceUnits.FirstOrDefault(x => x.IsDefault);
    private bool CanSubmit => !_loading && !_refreshingAccounts && !_processing && (!Id.HasValue || _model.Id == Id);

    private string? SourceAccountHint => _refreshingAccounts ? "در حال دریافت حساب‌ها"
        : _model.PriceUnit is null ? "ابتدا واحد پرداخت را انتخاب کنید."
        : _sourceFinancialAccounts.Count == 0 ? "برای این واحد، حساب پرداختی تعریف نشده است."
        : null;

    private string? DestinationAccountHint => _refreshingAccounts || _model.PriceUnit is null ? null
        : _model.Customer is null ? "ابتدا طرف حساب را انتخاب کنید."
        : _destinationFinancialAccounts.Count == 0 ? "برای این طرف حساب، حسابی با این واحد ثبت نشده است."
        : null;

    protected override async Task OnParametersSetAsync()
    {
        _loading = true;
        try
        {
            if (Id.HasValue)
                await LoadPaymentVoucherAsync();
            else
                await LoadVoucherNumberAsync();

            await LoadPriceUnitsAsync();
            await LoadSourceFinancialAccountsAsync();
            await LoadDestinationFinancialAccountsAsync();

            await base.OnParametersSetAsync();
        }
        finally
        {
            _loading = false;
        }
    }

    private async Task LoadVoucherNumberAsync()
    {
        await SendRequestAsync<IPaymentVoucherService, GetVoucherNumberResponse>(
            action: (s, ct) => s.GetLastNumberAsync(ct),
            afterSend: response =>
            {
                _model.VoucherNumber = response.VoucherNumber + 1;
            },
            createScope: true);
    }

    private async Task LoadPriceUnitsAsync()
    {
        await SendRequestAsync<IPriceUnitService, List<GetPriceUnitTitleResponse>>(
            action: (service, token) => service.GetTitlesAsync(token),
            afterSend: response =>
            {
                _priceUnits = response;
                _model.PriceUnit ??= _priceUnits.FirstOrDefault(u => u.IsDefault);
            });
    }

    private async Task LoadPaymentVoucherAsync()
    {
        await SendRequestAsync<IPaymentVoucherService, GetPaymentVoucherResponse>(
            action: (service, token) => service.GetAsync(Id!.Value, token),
            afterSend: response => _model = PaymentVoucherVm.CreateFrom(response));
    }

    private void Close()
    {
        if (_processing)
            return;

        if (Session is not null)
            Session.Cancel();
        else
            MudDialog.Cancel();
    }

    public async Task Submit()
    {
        if (!CanSubmit)
            return;

        _processing = true;
        Session?.SetBusy(true);
        try
        {
            await _form.ValidateAsync();

            if (!_form.IsValid)
            {
                AddErrorToast("موارد مشخص‌شده در فرم را بررسی کنید.");
                return;
            }

            await SendRequestAsync<IPaymentVoucherService>(
                action: (service, token) => Id.HasValue
                    ? service.UpdateAsync(Id.Value, PaymentVoucherVm.ToRequest(_model), token)
                    : service.CreateAsync(PaymentVoucherVm.ToRequest(_model), token),
                afterSend: () =>
                {
                    if (Session is not null)
                        Session.Close(true);
                    else
                        MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
        finally
        {
            _processing = false;
            Session?.SetBusy(false);
        }
    }

    private async Task OnAddSupplier()
    {
        var dialog = ResponsiveDialogs.Show<Customers.Components.Editor>("افزودن طرف حساب جدید", new Dictionary<string, object> {
            [nameof(Customers.Components.Editor.ReturnModel)] = true,
            [nameof(Customers.Components.Editor.ShowFinancialAccounts)] = false
        }, wide: true);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm customerVm })
            await OnSupplierChanged(customerVm);
    }

    private async Task OnAddFinancialAccount(bool isSourceAccount)
    {
        var nativeArgs = new Dictionary<string, object> {
            [nameof(FinancialAccountEditor.SubmitIndependently)] = true,
            [nameof(FinancialAccountEditor.PriceUnits)] = _priceUnits
        };
        if (_model.PriceUnit is { IsGoldBased: false } unit)
            nativeArgs[nameof(FinancialAccountEditor.PriceUnit)] = unit;

        if (!isSourceAccount)
        {
            if (_model.Customer?.Id is not Guid customerId)
                return;

            nativeArgs[nameof(FinancialAccountEditor.CustomerId)] = customerId;
            nativeArgs[nameof(FinancialAccountEditor.AccountHolderName)] = _model.Customer.FullName;
        }
        var dialog = ResponsiveDialogs.Show<FinancialAccountEditor>("افزودن حساب مالی جدید", nativeArgs, wide: true);

        var result = await dialog.Result;

        if (result is { Canceled: false, Data: FinancialAccountVm })
        {
            if (isSourceAccount)
                await LoadSourceFinancialAccountsAsync();
            else
                await LoadDestinationFinancialAccountsAsync();
        }
    }

    private async Task OnPriceUnitChanged(GetPriceUnitTitleResponse? priceUnit)
    {
        if (_processing || _refreshingAccounts || _model.PriceUnit == priceUnit)
            return;

        _model.PriceUnit = priceUnit;
        _model.ExchangeRate = null;
        _model.SourceFinancialAccount = null;
        _model.DestinationFinancialAccount = null;
        _sourceFinancialAccounts = [];
        _destinationFinancialAccounts = [];

        _refreshingAccounts = true;
        try
        {
            await LoadSourceFinancialAccountsAsync();
            await LoadDestinationFinancialAccountsAsync();
            await LoadExchangeRateAsync();
        }
        finally
        {
            _refreshingAccounts = false;
        }
    }

    private async Task LoadExchangeRateAsync()
    {
        if (DefaultPriceUnit is not { } defaultUnit)
        {
            AddErrorToast("ارز پیش فرض تعریف نشده است");
            return;
        }

        if (_model.PriceUnit is not { } unit || unit.Id == defaultUnit.Id)
            return;
        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(unit.Id, defaultUnit.Id, ct),
            afterSend: response => _model.ExchangeRate = response.ExchangeRate);
    }

    private async Task LoadSourceFinancialAccountsAsync()
    {
        if (_model.PriceUnit is { } unit)
            await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountTitleResponse>>(
                action: (service, token) => service.GetTitlesAsync(null, unit.Id, token),
                afterSend: response => _sourceFinancialAccounts = response);
    }

    private async Task LoadDestinationFinancialAccountsAsync()
    {
        if (_model.PriceUnit is { } unit && _model.Customer?.Id is Guid customerId)
            await SendRequestAsync<IFinancialAccountService, List<GetFinancialAccountTitleResponse>>(
                action: (service, token) => service.GetTitlesAsync(customerId, unit.Id, token),
                afterSend: response => _destinationFinancialAccounts = response);
    }

    private async Task<IEnumerable<CustomerVm>> SearchSuppliers(string? supplierName, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(supplierName))
            return [];

        var customers = await SendRequestAsync<ICustomerService, List<GetCustomerResponse>>(
            action: async (service, token) =>
            {
                using var search = CancellationTokenSource.CreateLinkedTokenSource(token, cancellationToken);
                return await service.GetByNameAsync(supplierName.Trim(), null, search.Token);
            },
            createScope: true);
        return customers?.Select(CustomerVm.CreateFrom) ?? [];
    }

    private async Task OnSupplierChanged(CustomerVm? supplier)
    {
        if (_processing || _refreshingAccounts || _model.Customer?.Id == supplier?.Id)
            return;

        _model.Customer = supplier;
        _model.DestinationFinancialAccount = null;
        _destinationFinancialAccounts = [];

        _refreshingAccounts = true;
        try
        {
            await LoadDestinationFinancialAccountsAsync();
        }
        finally
        {
            _refreshingAccounts = false;
        }
    }
}
