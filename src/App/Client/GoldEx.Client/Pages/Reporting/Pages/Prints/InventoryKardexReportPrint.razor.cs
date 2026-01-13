using GoldEx.Client.Pages.Reporting.ViewModels;
using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Reporting.Pages.Prints;

public partial class InventoryKardexReportPrint
{
    [Parameter, SupplyParameterFromQuery]
    public Guid? ProductId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public Guid? CoinInstanceId { get; set; }

    [Parameter, SupplyParameterFromQuery]
    public Guid? CurrencyId { get; set; }

    public string TitleFilter => $" - {_itemTitle?.Title}";

    private readonly int _version = new Random().Next(0, 1000);
    private InventoryKardexFilterVm _filter = default!;
    private List<InventoryKardexRpResponse>? _items;
    private GetInventoryItemTitleResponse? _itemTitle;
    private ReportSummaryVm? _summary;

    protected override void OnInitialized()
    {
        _filter = new InventoryKardexFilterVm
        {
            ProductId = ProductId,
            CoinInstanceId = CoinInstanceId,
            CurrencyId = CurrencyId,
            DateRange = FromDate.HasValue || ToDate.HasValue
                ? new DateRange(FromDate, ToDate)
                : null
        };
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadReportAsync();
        await LoadItemTitleAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadItemTitleAsync()
    {
        var itemType = _filter.GetItemType();
        var id = _filter.GetItemId();

        _itemTitle = await SendRequestAsync<IInventoryStockService, GetInventoryItemTitleResponse>(
            action: (s, ct) => s.GetTitleAsync(itemType, id, ct),
            createScope: true);
    }

    private async Task LoadReportAsync()
    {
        var request = _filter.ToRequest();

        _items = await SendRequestAsync<IReportingService, List<InventoryKardexRpResponse>>(
            action: (s, ct) => s.GetInventoryKardexAsync(request, ct),
            createScope: true);

        CalculateSummary();
    }

    private void CalculateSummary()
    {
        if (_items == null || !_items.Any())
        {
            _summary = null;
            return;
        }

        var totalIn = _items
            .Where(x => x.ActionType == WarehouseActionType.In)
            .Sum(x => x.Amount);

        var totalOut = _items
            .Where(x => x.ActionType == WarehouseActionType.Out)
            .Sum(x => x.Amount);

        var remaining = totalIn - totalOut;

        var unit = _items.First().GoldUnitType.HasValue
            ? _items.First().GoldUnitType?.GetDisplayName()
            : _items.First().PriceUnit ?? "عدد";

        _summary = new ReportSummaryVm
        {
            Sections =
            [
                new SummarySection
                {
                    Items =
                    [
                        new SummaryItem
                        {
                            Label = "مجموع ورود",
                            Value = totalIn.ToCurrencyFormat(unit),
                            Type = "credit",
                            ShowIcon = true,
                            IconType = "credit-icon"
                        },
                        new SummaryItem
                        {
                            Label = "مجموع خروج",
                            Value = totalOut.ToCurrencyFormat(unit),
                            Type = "debit",
                            ShowIcon = true,
                            IconType = "debit-icon"
                        },
                        new SummaryItem
                        {
                            Label = "مانده",
                            Value = Math.Abs(remaining).ToCurrencyFormat(unit),
                            Type = remaining switch
                            {
                                0 => "net",
                                > 0 => "credit",
                                < 0 => "debit"
                            },
                            ShowIcon = true,
                            IconType = remaining switch
                            {
                                0 => "",
                                > 0 => "credit-icon",
                                < 0 => "debit-icon"
                            }
                        }
                    ]
                }
            ]
        };
    }
}