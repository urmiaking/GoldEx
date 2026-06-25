using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class CustomerFiltersDialog
{
    [CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

    [Parameter] public DateRange DateRange { get; set; } = new();
    [Parameter] public CustomerType? CustomerTypeParam { get; set; }
    [Parameter] public TransactionType? TransactionTypeParam { get; set; }

    private DateRange? _dateRange;
    private CustomerType? _customerType;
    private TransactionType? _transactionType;

    protected override void OnParametersSet()
    {
        _dateRange = new DateRange(DateRange.Start, DateRange.End);
        _customerType = CustomerTypeParam;
        _transactionType = TransactionTypeParam;
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
            _transactionType
        );

        MudDialog.Close(DialogResult.Ok(result));
    }

    private void Clear()
    {
        _dateRange = new DateRange();
        _customerType = null;
        _transactionType = null;
        Apply();
    }

    private void Cancel() => MudDialog.Cancel();
}
