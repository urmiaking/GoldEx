using GoldEx.Client.Components.Services;
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
    [Inject] private HelpContext HelpContext { get; set; } = default!;

    private string? _searchString;
    private DateRange _filterDateRange = new();
    private MudTable<CustomerVm> _table = new();
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };
    private CustomerType? _customerType;
    private TransactionType? _transactionType;

    protected override void OnInitialized()
    {
        HelpContext.Slug = "customer-management-video";
        base.OnInitialized();
    }

    public override ValueTask DisposeAsync()
    {
        HelpContext.Slug = null;
        return base.DisposeAsync();
    }

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

        var customerFilter = new CustomerFilter(_customerType, _transactionType, _filterDateRange.Start, _filterDateRange.End);

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

        if (_table.CurrentPage != 0)
            _table.NavigateTo(0);

        else
            await _table.ReloadServerData();
    }

    private void PageChanged(int i)
    {
        if (i <= 0)
            return;

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
        var parameters = new DialogParameters<TransactionList>
        {
            { x => x.CustomerId, customerVm.Id },
            { x => x.ShowReversed, false },
            { x => x.Descending, true },
            { x => x.ContainerClass, "responsive-table-toolbar" }
        };
        var dialog = await DialogService.ShowAsync<TransactionList>($"تراکنش های {customerVm.FullName}",
            parameters,
            _dialogOptions with
            {
                MaxWidth = MaxWidth.ExtraLarge
            });
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
        var dialog = await DialogService.ShowAsync<InvoicesList>($"فاکتورهای {customerVm.FullName}", parameters, _dialogOptions with { MaxWidth = MaxWidth.ExtraLarge });
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
        _ => Color.Info
    };

    private async Task SetCustomerTypeFilterText(CustomerType? customerType)
    {
        _customerType = customerType;

        await RefreshAsync();
    }

    public string? TransactionTypeIcon => _transactionType switch
    {
        TransactionType.Debit => Icons.Material.Filled.AccountBalanceWallet,
        TransactionType.Credit => Icons.Material.Filled.Payments,
        null => Icons.Material.Filled.CompareArrows,
        _ => null
    };

    public Color TransactionTypeColor => _transactionType switch
    {
        TransactionType.Debit => Color.Error,
        TransactionType.Credit => Color.Success,
        null => Color.Info,
        _ => Color.Default
    };

    private string? GetTransactionTypeIcon(TransactionType? type) => type switch
    {
        TransactionType.Debit => Icons.Material.Filled.AccountBalanceWallet,
        TransactionType.Credit => Icons.Material.Filled.Payments,
        null => Icons.Material.Filled.CompareArrows,
        _ => null
    };

    private Color GetTransactionTypeIconColor(TransactionType? type) => type switch
    {
        TransactionType.Debit => Color.Error,
        TransactionType.Credit => Color.Success,
        _ => Color.Info
    };

    private async Task SetTransactionTypeFilterText(TransactionType? transactionType)
    {
        _transactionType = transactionType;

        await RefreshAsync();
    }

    #region Mobile filters

    public record CustomerMobileFiltersResult(
        DateRange DateRange,
        CustomerType? CustomerType,
        TransactionType? TransactionType
    );

    private async Task OpenMobileFilters()
    {
        var parameters = new DialogParameters<CustomerFiltersDialog>
        {
            { x => x.DateRange, _filterDateRange },
            { x => x.CustomerTypeParam, _customerType },
            { x => x.TransactionTypeParam, _transactionType }
        };

        var options = new DialogOptions
        {
            CloseButton = true,
            FullWidth = true,
            MaxWidth = MaxWidth.Small
        };

        var dialog = await DialogService.ShowAsync<CustomerFiltersDialog>("فیلترها", parameters, options);
        var result = await dialog.Result;

        if (result is { Canceled: false } && result.Data is CustomerMobileFiltersResult filterResult)
        {
            await ApplyMobileFilters(filterResult);
        }
    }

    private async Task ApplyMobileFilters(CustomerMobileFiltersResult result)
    {
        _filterDateRange = new DateRange(result.DateRange.Start, result.DateRange.End);
        _customerType = result.CustomerType;
        _transactionType = result.TransactionType;

        await RefreshAsync();
    }

    #endregion
}