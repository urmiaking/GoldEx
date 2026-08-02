using GoldEx.Client.Pages.Customers.Components;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Finances.CustomerTransfers.Validators;
using GoldEx.Client.Pages.Finances.CustomerTransfers.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.PriceUnits;
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
    [Inject] private ICustomerTransferVoucherService VoucherService { get; set; } = null!;
    [Inject] private CustomerTransferVoucherValidator Validator { get; set; } = null!;

    private MudForm _form = null!;
    private List<GetTinyInvoiceResponse> _sourceInvoices = [];
    private List<GetTinyInvoiceResponse> _destInvoices = [];
    private bool _loadingInvoices;
    private bool _processing;

    protected override async Task OnInitializedAsync()
    {
        if (!Model.PriceUnitId().HasValue && PriceUnits.Count > 0)
        {
            Model.PriceUnit = PriceUnits.FirstOrDefault(x => x.IsDefault) ?? PriceUnits.First();
        }

        if (Model.SourceCustomer?.Id != null)
        {
            await LoadSourceInvoicesAsync(Model.SourceCustomer.Id.Value);
        }

        if (Model.DestinationCustomer?.Id != null)
        {
            await LoadDestInvoicesAsync(Model.DestinationCustomer.Id.Value);
        }
    }

    private async Task OnSourceCustomerChanged(CustomerVm? customer)
    {
        Model.SourceCustomer = customer;
        Model.SourceInvoice = null;
        _sourceInvoices.Clear();

        if (customer?.Id != null)
        {
            await LoadSourceInvoicesAsync(customer.Id.Value);
        }
    }

    private async Task OnDestCustomerChanged(CustomerVm? customer)
    {
        Model.DestinationCustomer = customer;
        Model.DestinationInvoice = null;
        _destInvoices.Clear();

        if (customer?.Id != null)
        {
            await LoadDestInvoicesAsync(customer.Id.Value);
        }
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
        var pagedList = await CustomerService.GetListAsync(filter, null, cancellationToken);
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

    private void OnPriceUnitChanged(GetPriceUnitTitleResponse? unit)
    {
        Model.PriceUnit = unit;
        if (unit != null && unit.IsDefault)
        {
            Model.ExchangeRate = null;
        }
    }

    private async Task SubmitAsync()
    {
        await _form.ValidateAsync();

        if (!_form.IsValid)
            return;

        if (Model.SourceCustomer?.Id == Model.DestinationCustomer?.Id)
        {
            AddErrorToast("مشتری فرستنده و گیرنده نمی‌توانند یکسان باشند.");
            return;
        }

        _processing = true;
        try
        {
            MudDialog.Close(DialogResult.Ok(Model));
        }
        finally
        {
            _processing = false;
        }
    }

    private void Cancel() => MudDialog.Cancel();
}

public static class CustomerTransferVoucherVmExtensions
{
    public static Guid? PriceUnitId(this CustomerTransferVoucherVm vm) => vm.PriceUnit?.Id;
}
