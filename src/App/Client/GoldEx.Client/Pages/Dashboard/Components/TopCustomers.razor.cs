using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MudBlazor;

namespace GoldEx.Client.Pages.Dashboard.Components;

public partial class TopCustomers
{
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private GetPriceUnitTitleResponse? _selectedPriceUnit;
    private TransactionType _transactionType = TransactionType.Debit;
    private MudTable<GetTopCustomerResponse>? _table;

    private string AmountIcon => _transactionType is TransactionType.Debit
        ? Icons.Material.Outlined.ArrowDropDown
        : Icons.Material.Outlined.ArrowDropUp;

    private Color AmountColor => _transactionType is TransactionType.Debit
        ? Color.Error
        : Color.Success;

    public string CardTitle => _transactionType is TransactionType.Debit
        ? "بدهکار ترین مشتریان"
        : "بستانکار ترین مشتریان";

    protected override async Task OnInitializedAsync()
    {
        await LoadPriceUnitsAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadPriceUnitsAsync()
    {
        var request = new TransactionFilter(
            null,
            null,
            null,
            null,
            null,
            null,
            false,
            false);

        await SendRequestAsync<ITransactionService, List<GetPriceUnitTitleResponse>>(
            (s, ct) => s.GetAvailablePriceUnitsAsync(request, ct),
            response => _priceUnits = response);
    }

    private async Task LoadTopCustomersAsync()
    {
        if (_table is not null) 
            await _table.ReloadServerData();
    }

    private async Task SelectPriceUnit(GetPriceUnitTitleResponse? item)
    {
        _selectedPriceUnit = _priceUnits.FirstOrDefault(x => x.Id == item?.Id);
        await LoadTopCustomersAsync();
    }

    private async Task ToggleTransactionType()
    {
        _transactionType = _transactionType is TransactionType.Debit ? TransactionType.Credit : TransactionType.Debit;
        await LoadTopCustomersAsync();
    }

    private async Task<TableData<GetTopCustomerResponse>> TopCustomerListProvider(TableState state, CancellationToken cancellationToken)
    {
        var result = new TableData<GetTopCustomerResponse>();

        await SendRequestAsync<ITransactionService, List<GetTopCustomerResponse>>(
            action: (s, ct) => s.GetTopCustomersAsync(_transactionType, _selectedPriceUnit?.Id, ct),
            afterSend: response =>
            {
                result = new TableData<GetTopCustomerResponse>
                {
                    TotalItems = response.Count,
                    Items = response
                };
            }
        );

        return result;
    }
}