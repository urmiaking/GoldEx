using GoldEx.Client.Extensions;
using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class CurrencyInventoryReportPrint
{
    [Parameter, SupplyParameterFromQuery]
    public string? ItemStatus { get; set; }

    public string TitleFilter
    {
        get
        {
            string? output = null;

            if (_itemStatus.HasValue)
            {
                output += $" ({_itemStatus.Value.GetDisplayName()})";
            }

            return output ?? string.Empty;
        }
    }

    private readonly int _version = new Random().Next(0, 1000);
    private CurrencyInventoryFilterVm _filter = default!;
    private List<CurrencyInventoryRpResponse>? _items;
    private ItemStatus? _itemStatus;

    protected override void OnInitialized()
    {
        if (!string.IsNullOrWhiteSpace(ItemStatus) &&
            Enum.TryParse<ItemStatus>(ItemStatus, ignoreCase: true, out var parsedStatus))
        {
            _itemStatus = parsedStatus;
        }

        _filter = new CurrencyInventoryFilterVm
        {
            ItemStatus = _itemStatus,
            DateRange = DateRangeExtensions.From(FromDate, ToDate),
        };
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadReportAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadReportAsync()
    {
        var request = _filter.ToRequest();

        _items = await SendRequestAsync<IReportingService, List<CurrencyInventoryRpResponse>>(
            action: (s, ct) => s.GetCurrencyInventoryAsync(request, ct),
            createScope: true);
    }
}
