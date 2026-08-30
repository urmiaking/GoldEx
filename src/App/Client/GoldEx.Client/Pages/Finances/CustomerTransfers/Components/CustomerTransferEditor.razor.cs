using GoldEx.Client.Pages.Customers.Components;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Finances.CustomerTransfers.Validators;
using GoldEx.Client.Pages.Finances.CustomerTransfers.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.CustomerTransfers;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.CustomerTransfers.Components;

public partial class CustomerTransferEditor
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;

    [Parameter] public CustomerTransferVoucherVm Model { get; set; } = new();
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];

    [Inject] private ICustomerService CustomerService { get; set; } = null!;
    [Inject] private IInvoiceService InvoiceService { get; set; } = null!;
    [Inject] private IPriceService PriceService { get; set; } = null!;
    [Inject] private ITransactionService TransactionService { get; set; } = null!;
    [Inject] private ICustomerTransferVoucherService VoucherService { get; set; } = null!;
    [Inject] private CustomerTransferVoucherValidator Validator { get; set; } = null!;

    private MudForm _form = null!;
    private List<GetTinyInvoiceResponse> _sourceInvoices = [];
    private List<GetTinyInvoiceResponse> _destInvoices = [];
    private bool _loadingInvoices;
    private bool _processing;

    private decimal? _sourceCurrentBalance;
    private bool _loadingSourceBalance;

    private decimal? _destCurrentBalance;
    private bool _loadingDestBalance;

    public bool IsEditMode => Model.Id.HasValue;
    private decimal _originalAmount;

    public bool IsDestinationPurchaseInvoice =>
        Model.DestinationInvoice != null && Model.DestinationInvoice.InvoiceType == InvoiceType.Purchase;

    public bool IsSourceSellInvoice =>
        Model.SourceInvoice != null && Model.SourceInvoice.InvoiceType == InvoiceType.Sell;

    public decimal? SourceBaseBalance
    {
        get
        {
            if (!_sourceCurrentBalance.HasValue) return null;
            if (!IsEditMode) return _sourceCurrentBalance.Value;

            if (IsDestinationPurchaseInvoice || IsSourceSellInvoice)
                return _sourceCurrentBalance.Value + _originalAmount;
            return _sourceCurrentBalance.Value - _originalAmount;
        }
    }

    public decimal? DestBaseBalance
    {
        get
        {
            if (!_destCurrentBalance.HasValue) return null;
            if (!IsEditMode) return _destCurrentBalance.Value;

            if (IsDestinationPurchaseInvoice || IsSourceSellInvoice)
                return _destCurrentBalance.Value - _originalAmount;
            return _destCurrentBalance.Value + _originalAmount;
        }
    }

    public decimal? SourceProjectedBalance
    {
        get
        {
            if (!SourceBaseBalance.HasValue) return null;
            if (IsDestinationPurchaseInvoice || IsSourceSellInvoice)
                return SourceBaseBalance.Value - Model.Amount;
            return SourceBaseBalance.Value + Model.Amount;
        }
    }

    public decimal? DestProjectedBalance
    {
        get
        {
            if (!DestBaseBalance.HasValue) return null;
            if (IsDestinationPurchaseInvoice || IsSourceSellInvoice)
                return DestBaseBalance.Value + Model.Amount;
            return DestBaseBalance.Value - Model.Amount;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        _originalAmount = Model.Id.HasValue ? Model.Amount : 0;

        if (!Model.PriceUnitId().HasValue && PriceUnits.Count > 0)
        {
            Model.PriceUnit = PriceUnits.FirstOrDefault(x => x.IsDefault) ?? PriceUnits.First();
        }

        if (Model.SourceCustomer?.Id != null)
        {
            await Task.WhenAll(
                LoadSourceInvoicesAsync(Model.SourceCustomer.Id.Value),
                LoadSourceBalanceAsync()
            );

            if (Model.SourceInvoiceId.HasValue && Model.SourceInvoice == null)
            {
                Model.SourceInvoice = _sourceInvoices.FirstOrDefault(x => x.Id == Model.SourceInvoiceId.Value);
            }
        }

        if (Model.DestinationCustomer?.Id != null)
        {
            await Task.WhenAll(
                LoadDestInvoicesAsync(Model.DestinationCustomer.Id.Value),
                LoadDestBalanceAsync()
            );

            if (Model.DestinationInvoiceId.HasValue && Model.DestinationInvoice == null)
            {
                Model.DestinationInvoice = _destInvoices.FirstOrDefault(x => x.Id == Model.DestinationInvoiceId.Value);
            }
        }

        if (Model.ExchangeRate == null && Model.PriceUnit != null && !Model.PriceUnit.IsDefault)
        {
            await LoadExchangeRateAsync();
        }
    }

    private async Task OnSourceCustomerChanged(CustomerVm? customer)
    {
        Model.SourceCustomer = customer;
        Model.SourceInvoice = null;
        _sourceInvoices.Clear();

        if (customer?.Id != null)
        {
            await Task.WhenAll(
                LoadSourceInvoicesAsync(customer.Id.Value),
                LoadSourceBalanceAsync()
            );
        }
        else
        {
            _sourceCurrentBalance = null;
        }
    }

    private async Task OnDestCustomerChanged(CustomerVm? customer)
    {
        Model.DestinationCustomer = customer;
        Model.DestinationInvoice = null;
        _destInvoices.Clear();

        if (customer?.Id != null)
        {
            await Task.WhenAll(
                LoadDestInvoicesAsync(customer.Id.Value),
                LoadDestBalanceAsync()
            );
        }
        else
        {
            _destCurrentBalance = null;
        }
    }

    private async Task LoadSourceBalanceAsync()
    {
        if (Model.SourceCustomer?.Id == null || Model.PriceUnit == null)
        {
            _sourceCurrentBalance = null;
            return;
        }

        try
        {
            _loadingSourceBalance = true;
            StateHasChanged();
            var balances = await TransactionService.GetCustomerRemainingListAsync(
                Model.SourceCustomer.Id.Value, Model.PriceUnit.Id);
            var item = balances.FirstOrDefault(b => b.PriceUnit.Id == Model.PriceUnit.Id);
            _sourceCurrentBalance = item?.Amount ?? 0m;
        }
        catch (Exception)
        {
            _sourceCurrentBalance = null;
        }
        finally
        {
            _loadingSourceBalance = false;
            StateHasChanged();
        }
    }

    private async Task LoadDestBalanceAsync()
    {
        if (Model.DestinationCustomer?.Id == null || Model.PriceUnit == null)
        {
            _destCurrentBalance = null;
            return;
        }

        try
        {
            _loadingDestBalance = true;
            StateHasChanged();
            var balances = await TransactionService.GetCustomerRemainingListAsync(
                Model.DestinationCustomer.Id.Value, Model.PriceUnit.Id);
            var item = balances.FirstOrDefault(b => b.PriceUnit.Id == Model.PriceUnit.Id);
            _destCurrentBalance = item?.Amount ?? 0m;
        }
        catch (Exception)
        {
            _destCurrentBalance = null;
        }
        finally
        {
            _loadingDestBalance = false;
            StateHasChanged();
        }
    }

    private async Task OnPriceUnitChanged(GetPriceUnitTitleResponse? unit)
    {
        Model.PriceUnit = unit;
        await Task.WhenAll(
            LoadExchangeRateAsync(),
            LoadSourceBalanceAsync(),
            LoadDestBalanceAsync()
        );
    }

    private string GetBalanceLabel(decimal? balance)
    {
        if (!balance.HasValue) return "-";
        var abs = Math.Abs(balance.Value);
        var unitTitle = Model.PriceUnit?.Title ?? "";
        var formatted = abs.ToCurrencyFormat(unitTitle);
        if (balance.Value > 0)
            return $"{formatted} (بدهکار)";
        if (balance.Value < 0)
            return $"{formatted} (بستانکار)";
        return $"۰ {unitTitle} (تسویه)";
    }

    private Color GetBalanceColor(decimal? balance)
    {
        if (!balance.HasValue) return Color.Default;
        if (balance.Value > 0) return Color.Error;
        if (balance.Value < 0) return Color.Success;
        return Color.Default;
    }

    private async Task LoadSourceInvoicesAsync(Guid customerId)
    {
        try
        {
            _loadingInvoices = true;
            _sourceInvoices = await InvoiceService.GetCustomerInvoicesAsync(customerId, new RequestFilter());
        }
        catch (Exception ex)
        {
            AddErrorToast($"خطا در دریافت فاکتورهای فرستنده: {ex.Message}");
        }
        finally
        {
            _loadingInvoices = false;
        }
    }

    private async Task LoadDestInvoicesAsync(Guid customerId)
    {
        try
        {
            _loadingInvoices = true;
            _destInvoices = await InvoiceService.GetCustomerInvoicesAsync(customerId, new RequestFilter());
        }
        catch (Exception ex)
        {
            AddErrorToast($"خطا در دریافت فاکتورهای گیرنده: {ex.Message}");
        }
        finally
        {
            _loadingInvoices = false;
        }
    }

    private async Task<IEnumerable<CustomerVm>> SearchCustomers(string value, CancellationToken cancellationToken)
    {
        var filter = new RequestFilter { Search = value, Take = 20 };
        var pagedList = await CustomerService.GetListAsync(filter, new CustomerFilter(null, null, null, null), cancellationToken);
        return pagedList.Data.Select(c => new CustomerVm
        {
            Id = c.Id,
            FullName = c.FullName,
            CustomerType = c.CustomerType,
            PhoneNumber = c.PhoneNumber,
            NationalId = c.NationalId
        });
    }

    private async Task OnAddCustomer(string title)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, new CustomerVm() }
        };

        var dialog = await DialogService.ShowAsync<Editor>($"ثبت {title} جدید", parameters);
        var result = await dialog.Result;

        if (result is { Canceled: false, Data: CustomerVm newCustomer })
        {
            if (title.Contains("فرستنده"))
                await OnSourceCustomerChanged(newCustomer);
            else
                await OnDestCustomerChanged(newCustomer);
        }
    }

    private async Task LoadExchangeRateAsync()
    {
        var defaultPriceUnit = PriceUnits.FirstOrDefault(x => x.IsDefault);
        if (defaultPriceUnit is null || Model.PriceUnit is null)
            return;

        if (Model.PriceUnit.Id == defaultPriceUnit.Id)
        {
            Model.ExchangeRate = null;
            StateHasChanged();
            return;
        }

        await SendRequestAsync<IPriceService, GetExchangeRateResponse>(
            action: (s, ct) => s.GetExchangeRateAsync(Model.PriceUnit.Id, defaultPriceUnit.Id, ct),
            afterSend: response =>
            {
                Model.ExchangeRate = response.ExchangeRate;
                StateHasChanged();
            });
    }

    private async Task SubmitAsync()
    {
        if (_processing)
            return;

        _processing = true;

        await _form.ValidateAsync();

        if (!_form.IsValid)
        {
            _processing = false;
            return;
        }

        if (Model.SourceCustomer?.Id == Model.DestinationCustomer?.Id)
        {
            AddErrorToast("مشتری فرستنده و گیرنده نمی‌توانند یکسان باشند.");
            _processing = false;
            return;
        }

        if (Model.SourceCustomer?.Id == null || Model.DestinationCustomer?.Id == null || Model.PriceUnit == null)
        {
            AddErrorToast("لطفاً اطلاعات الزامی را کامل کنید.");
            _processing = false;
            return;
        }

        if (!Model.Id.HasValue)
        {
            var req = new CreateCustomerTransferVoucherRequest
            {
                TransferDate = DateOnly.FromDateTime(Model.TransferDate ?? DateTime.Now),
                SourceCustomerId = Model.SourceCustomer.Id.Value,
                DestinationCustomerId = Model.DestinationCustomer.Id.Value,
                PriceUnitId = Model.PriceUnit.Id,
                Amount = Model.Amount,
                ExchangeRate = Model.ExchangeRate,
                SourceInvoiceId = Model.SourceInvoice?.Id,
                DestinationInvoiceId = Model.DestinationInvoice?.Id,
                Description = Model.Description
            };

            await SendRequestAsync<ICustomerTransferVoucherService>(
                action: (s, ct) => s.CreateAsync(req, ct),
                afterSend: () =>
                {
                    AddSuccessToast("سند حواله با موفقیت ثبت شد.");
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }
        else
        {
            var req = new UpdateCustomerTransferVoucherRequest
            {
                Id = Model.Id.Value,
                TransferDate = DateOnly.FromDateTime(Model.TransferDate ?? DateTime.Now),
                SourceCustomerId = Model.SourceCustomer.Id.Value,
                DestinationCustomerId = Model.DestinationCustomer.Id.Value,
                PriceUnitId = Model.PriceUnit.Id,
                Amount = Model.Amount,
                ExchangeRate = Model.ExchangeRate,
                SourceInvoiceId = Model.SourceInvoice?.Id,
                DestinationInvoiceId = Model.DestinationInvoice?.Id,
                Description = Model.Description
            };

            await SendRequestAsync<ICustomerTransferVoucherService>(
                action: (s, ct) => s.UpdateAsync(Model.Id.Value, req, ct),
                afterSend: () =>
                {
                    AddSuccessToast("سند حواله با موفقیت به روزرسانی شد.");
                    MudDialog.Close(DialogResult.Ok(true));
                    return Task.CompletedTask;
                });
        }

        _processing = false;
    }

    private void Cancel() => MudDialog.Cancel();
}

public static class CustomerTransferVoucherVmExtensions
{
    public static Guid? PriceUnitId(this CustomerTransferVoucherVm vm) => vm.PriceUnit?.Id;
}
