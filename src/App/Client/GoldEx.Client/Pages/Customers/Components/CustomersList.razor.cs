using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Client.Pages.Invoices.Components;
using GoldEx.Client.Pages.Transactions.Components;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class CustomersList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string ContainerClass { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public bool ShowTitle { get; set; }

    private string? _searchString;
    private DateRange _filterDateRange = new();
    private MudTable<CustomerVm> _table = new();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };
    private CustomerType? _customerType;

    private async Task<TableData<CustomerVm>> LoadCustomersAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<CustomerVm>();

        var filter = new RequestFilter(state.Page * state.PageSize, state.PageSize, _searchString, state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            });

        var customerFilter = new CustomerFilter(_customerType, _filterDateRange.Start, _filterDateRange.End);

        await SendRequestAsync<ICustomerService, PagedList<GetCustomerResponse>>(
            action: (s, token) => s.GetListAsync(filter, customerFilter, token),
            afterSend: response =>
            {
                var items = response.Data.Select(CustomerVm.CreateFrom).ToList();

                result = new TableData<CustomerVm>
                {
                    TotalItems = response.Total,
                    Items = items
                };
            },
            createScope: true,
            cancelPrevious: true
        );

        return result;
    }

    private async Task OnSearch(string text)
    {
        _searchString = text;
        await RefreshAsync();
    }
        
    private void PageChanged(int i)
    {
        _table.NavigateTo(i - 1);   
    }

    public async Task OnCreate()
    {
        var dialog = await DialogService.ShowAsync<Editor>("افزودن مشتری جدید", _dialogOptions);

        var result = await dialog.Result;

        if (result is {Canceled: false})
        {
            AddSuccessToast("مشتری جدید با موفقیت افزوده شد.");
            await RefreshAsync();
        }
    }

    private async Task OnRemove(CustomerVm model)
    {
        if (!model.Id.HasValue)
            throw new InvalidOperationException();

        var parameters = new DialogParameters<Remove>
        {
            { x => x.Id, model.Id.Value },
            { x => x.CustomerName, model.FullName }
        };

        var dialog = await DialogService.ShowAsync<Remove>("حذف مشتری", parameters, _dialogOptions with { MaxWidth = MaxWidth.Small });

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("مشتری با موفقیت حذف شد.");
            await RefreshAsync();
        }
    }

    private async Task OnEdit(CustomerVm model)
    {
        var parameters = new DialogParameters<Editor>
        {
            { x => x.Model, model }
        };

        var dialog = await DialogService.ShowAsync<Editor>("ویرایش اطلاعات مشتری", parameters, _dialogOptions);

        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            AddSuccessToast("اطلاعات مشتری با موفقیت ویرایش شد.");
            await RefreshAsync();
        }
    }

    private async Task OnViewTransaction(CustomerVm customerVm)
    {
        //var parameters = new DialogParameters
        //{
        //    { nameof(TransactionsList.CustomerId), customerVm.Id }
        //};
        var dialog = await DialogService.ShowAsync<TransactionsList>($"تراکنش های {customerVm.FullName}", _dialogOptions with { MaxWidth = MaxWidth.Large });
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            await RefreshAsync();
        }
    }

    private async Task OnViewInvoices(CustomerVm customerVm)
    {
        var parameters = new DialogParameters
        {
            { nameof(InvoicesList.CustomerId), customerVm.Id }
        };
        var dialog = await DialogService.ShowAsync<InvoicesList>($"فاکتورهای های {customerVm.FullName}", parameters, _dialogOptions with { MaxWidth = MaxWidth.Large });
        var result = await dialog.Result;
        if (result is { Canceled: false })
        {
            await RefreshAsync();
        }
    }

    private async Task OnDateRangeChanged(DateRange dateRange)
    {
        _filterDateRange = dateRange;
        await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        await _table.ReloadServerData();
        StateHasChanged();
    }

    public string? CustomerTypeIcon => _customerType switch
    {
        CustomerType.RetailCustomer => Icons.Material.Filled.ShoppingCart,
        CustomerType.Wholesaler => Icons.Material.Filled.Warehouse,
        CustomerType.Workshop => Icons.Material.Filled.Build,
        CustomerType.Retailer => Icons.Material.Filled.Store,
        CustomerType.AssayingLab => Icons.Material.Filled.Science,
        CustomerType.MeltedGoldDealer => Icons.Material.Filled.LocalFireDepartment,
        null => Icons.Material.Filled.List,
        _ => null
    };

    public Color CustomerTypeColor => _customerType switch
    {
        CustomerType.RetailCustomer => Color.Info,
        CustomerType.Wholesaler => Color.Primary,
        CustomerType.Workshop => Color.Tertiary,
        CustomerType.Retailer => Color.Error,
        CustomerType.AssayingLab => Color.Info,
        CustomerType.MeltedGoldDealer => Color.Success,
        null => Color.Info,
        _ => Color.Default
    };

    private string? GetCustomerTypeIcon(CustomerType? type) => type switch
    {
        CustomerType.RetailCustomer => Icons.Material.Filled.ShoppingCart,
        CustomerType.Wholesaler => Icons.Material.Filled.Warehouse,
        CustomerType.Workshop => Icons.Material.Filled.Build,
        CustomerType.Retailer => Icons.Material.Filled.Store,
        CustomerType.AssayingLab => Icons.Material.Filled.Science,
        CustomerType.MeltedGoldDealer => Icons.Material.Filled.LocalFireDepartment,
        null => Icons.Material.Filled.List,
        _ => null
    };

    private Color GetCustomerTypeIconColor(CustomerType? type) => type switch
    {
        CustomerType.RetailCustomer => Color.Info,
        CustomerType.Wholesaler => Color.Primary,
        CustomerType.Workshop => Color.Tertiary,
        CustomerType.Retailer => Color.Error,
        CustomerType.AssayingLab => Color.Info,
        CustomerType.MeltedGoldDealer => Color.Success,
        null => Color.Info,
        _ => Color.Info
    };

    private async Task SetCustomerTypeFilterText(CustomerType? customerType)
    {
        _customerType = customerType;

        await RefreshAsync();
    }
}