using GoldEx.Client.Pages.Finances.CustomerTransfers.ViewModels;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.CustomerTransfers;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.CustomerTransfers.Components;

public partial class CustomerTransferList
{
    [Parameter] public string Class { get; set; } = default!;
    [Parameter] public string ContainerClass { get; set; } = default!;
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public bool ShowTitle { get; set; }
    [Parameter] public Guid? CustomerId { get; set; }
    [Parameter] public string? SearchQuery { get; set; }

    [Inject] private ICustomerTransferVoucherService VoucherService { get; set; } = null!;
    [Inject] private IPriceUnitService PriceUnitService { get; set; } = null!;

    private MudTable<GetCustomerTransferVoucherListResponse> _table = new();
    private DateRange _filterDateRange = new();
    private GetPriceUnitTitleResponse? _selectedPriceUnit;
    private List<GetPriceUnitTitleResponse> _priceUnits = [];
    private readonly DialogOptions _dialogOptions = new() { CloseButton = true, FullWidth = true, FullScreen = false, MaxWidth = MaxWidth.Medium };

    protected override async Task OnInitializedAsync()
    {
        try
        {
            _priceUnits = await PriceUnitService.GetTitlesAsync();
        }
        catch (Exception ex)
        {
            AddErrorToast($"خطا در دریافت لیست واحدهای قیمت: {ex.Message}");
        }
    }

    private async Task<TableData<GetCustomerTransferVoucherListResponse>> LoadCustomerTransfersAsync(TableState state, CancellationToken cancellationToken = default)
    {
        var result = new TableData<GetCustomerTransferVoucherListResponse>();

        var voucherFilter = new CustomerTransferVoucherFilter
        {
            FromDate = _filterDateRange.Start.HasValue ? DateOnly.FromDateTime(_filterDateRange.Start.Value) : null,
            ToDate = _filterDateRange.End.HasValue ? DateOnly.FromDateTime(_filterDateRange.End.Value) : null,
            PriceUnitId = _selectedPriceUnit?.Id,
            SourceCustomerId = CustomerId
        };

        var filter = new RequestFilter(
            state.Page * state.PageSize,
            state.PageSize,
            SearchQuery,
            state.SortLabel,
            state.SortDirection switch
            {
                SortDirection.None => Sdk.Common.Definitions.SortDirection.None,
                SortDirection.Ascending => Sdk.Common.Definitions.SortDirection.Ascending,
                SortDirection.Descending => Sdk.Common.Definitions.SortDirection.Descending,
                _ => throw new ArgumentOutOfRangeException()
            }
        );

        await SendRequestAsync<ICustomerTransferVoucherService, PagedList<GetCustomerTransferVoucherListResponse>>(
            action: (service, token) => service.GetListAsync(filter, voucherFilter, token),
            afterSend: response =>
            {
                result = new TableData<GetCustomerTransferVoucherListResponse>
                {
                    TotalItems = response.Total,
                    Items = response.Data
                };
            },
            createScope: true,
            cancelPrevious: true
        );

        return result;
    }

    private async Task OnSearch(string text)
    {
        SearchQuery = text;

        if (_table.CurrentPage != 0)
            _table.NavigateTo(0);
        else
            await _table.ReloadServerData();
    }

    private async Task OnDateRangeChanged(DateRange range)
    {
        _filterDateRange = range;
        await _table.ReloadServerData();
    }

    private async Task SetPriceUnitFilter(GetPriceUnitTitleResponse? unit)
    {
        _selectedPriceUnit = unit;
        await _table.ReloadServerData();
    }

    private void PageChanged(int i)
    {
        if (i <= 0)
            return;

        _table.NavigateTo(i - 1);
    }

    private async Task OnCreate()
    {
        var model = new CustomerTransferVoucherVm
        {
            TransferDate = DateTime.Now
        };

        var parameters = new DialogParameters<CustomerTransferEditor>
        {
            { x => x.Model, model },
            { x => x.PriceUnits, _priceUnits }
        };

        var dialog = await DialogService.ShowAsync<CustomerTransferEditor>("ثبت حواله جدید بین مشتریان", parameters, _dialogOptions);
        var result = await dialog.Result;

        if (result is { Canceled: false })
        {
            await _table.ReloadServerData();
        }
    }

    private async Task OnEdit(GetCustomerTransferVoucherListResponse item)
    {
        try
        {
            var fullItem = await VoucherService.GetAsync(item.Id);

            var model = new CustomerTransferVoucherVm
            {
                Id = fullItem.Id,
                VoucherNumber = fullItem.VoucherNumber,
                TransferDate = fullItem.TransferDate.ToDateTime(TimeOnly.MinValue),
                PriceUnit = _priceUnits.FirstOrDefault(u => u.Id == fullItem.PriceUnitId),
                Amount = fullItem.Amount,
                ExchangeRate = fullItem.ExchangeRate,
                Description = fullItem.Description,
                SourceInvoiceId = fullItem.SourceInvoiceId,
                DestinationInvoiceId = fullItem.DestinationInvoiceId,
                SourceCustomer = new Customers.ViewModels.CustomerVm
                {
                    Id = fullItem.SourceCustomerId,
                    FullName = fullItem.SourceCustomerName
                },
                DestinationCustomer = new Customers.ViewModels.CustomerVm
                {
                    Id = fullItem.DestinationCustomerId,
                    FullName = fullItem.DestinationCustomerName
                }
            };

            var parameters = new DialogParameters<CustomerTransferEditor>
            {
                { x => x.Model, model },
                { x => x.PriceUnits, _priceUnits }
            };

            var dialog = await DialogService.ShowAsync<CustomerTransferEditor>("ویرایش سند حواله", parameters, _dialogOptions);
            var result = await dialog.Result;

            if (result is { Canceled: false })
            {
                await _table.ReloadServerData();
            }
        }
        catch (Exception ex)
        {
            AddErrorToast($"خطا در بارگذاری یا ویرایش حواله: {ex.Message}");
        }
    }

    private async Task OnDelete(GetCustomerTransferVoucherListResponse item)
    {
        var result = await DialogService.ShowMessageBoxAsync("حذف سند حواله",
            $"آیا از حذف سند حواله شماره {item.VoucherNumber} به مبلغ/وزن {item.Amount.ToCurrencyFormat(item.PriceUnitTitle)} مطمئن هستید؟",
            yesText: "حذف", noText: "انصراف");

        if (result is true)
        {
            try
            {
                await VoucherService.DeleteAsync(item.Id);
                AddSuccessToast("سند حواله با موفقیت حذف شد.");
                await _table.ReloadServerData();
            }
            catch (Exception ex)
            {
                AddErrorToast($"خطا در حذف سند حواله: {ex.Message}");
            }
        }
    }
}
