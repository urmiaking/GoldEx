using GoldEx.Client.Components.Services;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Client.Pages.Customers.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class CustomerFiltersDialog
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }

    [Parameter] public DateRange DateRange { get; set; } = new();
    [Parameter] public CustomerType? CustomerTypeParam { get; set; }
    [Parameter] public TransactionType? TransactionTypeParam { get; set; }
    [Parameter] public string SortLabel { get; set; } = nameof(CustomerVm.CreatedAt);
    [Parameter] public bool SortDescending { get; set; } = true;

    private DateRange? _dateRange;
    private CustomerType? _customerType;
    private TransactionType? _transactionType;
    private string _sortLabel = nameof(CustomerVm.CreatedAt);
    private bool _sortDescending = true;

    protected override void OnParametersSet()
    {
        _dateRange = new DateRange(DateRange.Start, DateRange.End);
        _customerType = CustomerTypeParam;
        _transactionType = TransactionTypeParam;
        _sortLabel = SortLabel;
        _sortDescending = SortDescending;
    }

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

    private void Apply()
    {
        var result = new CustomersList.CustomerMobileFiltersResult(
            new DateRange(_dateRange?.Start, _dateRange?.End),
            _customerType,
            _transactionType,
            _sortLabel,
            _sortDescending
        );

        Session?.Close(result);
    }

    private void Clear()
    {
        _dateRange = new DateRange();
        _customerType = null;
        _transactionType = null;
        // Reset only filters; keep sorting set by the desktop toolbar.
        Apply();
    }

    private void OnSortChanged(string label)
    {
        _sortLabel = label;
        _sortDescending = label == nameof(CustomerVm.CreatedAt);
    }

    private void Cancel() => Session?.Cancel();
}
