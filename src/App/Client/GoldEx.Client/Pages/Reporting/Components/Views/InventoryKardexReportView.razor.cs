using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using static GoldEx.Client.Pages.Reporting.ViewModels.InventoryKardexFilterVm;

namespace GoldEx.Client.Pages.Reporting.Components.Views;

public partial class InventoryKardexReportView
{
    [Parameter] public List<InventoryKardexRpResponse>? Items { get; set; }
    [Parameter] public EventCallback OnPrintReport { get; set; }
    [Parameter] public bool IsLoading { get; set; }

    private InventoryKardexReportSummary? _summary;

    protected override void OnParametersSet()
    {
        CalculateSummary();
        base.OnParametersSet();
    }

    private void CalculateSummary()
    {
        if (Items == null || Items.Count == 0)
        {
            _summary = null;
            return;
        }

        var totalIn = Items
            .Where(x => x.ActionType == WarehouseActionType.In)
            .Sum(x => x.Amount);

        var totalOut = Items
            .Where(x => x.ActionType == WarehouseActionType.Out)
            .Sum(x => x.Amount);

        var unit = Items.First().GoldUnitType.HasValue
            ? Items.First().GoldUnitType?.GetDisplayName()
            : Items.First().PriceUnit ?? "عدد"; 

        _summary = new InventoryKardexReportSummary
        {
            TotalIn = totalIn,
            TotalOut = totalOut,
            Unit = unit
        };
    }

    private string GetAmount(InventoryKardexRpResponse context)
    {
        return context.GoldUnitType.HasValue
            ? $"{context.Amount.ToWeightFormat(context.GoldUnitType.Value)}"
            : context.Amount.ToCurrencyFormat(context.PriceUnit ?? "عدد");
    }

    private void OnViewSource(string sourceUrl) => Navigation.NavigateTo(sourceUrl);

    private Color GetColor(InventoryKardexRpResponse context)
    {
        return context.ActionType switch
        {
            WarehouseActionType.In => Color.Success,
            WarehouseActionType.Out => Color.Error,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private string GetIcon(InventoryKardexRpResponse context)
    {
        return context.ActionType switch
        {
            WarehouseActionType.In => Icons.Material.Outlined.ArrowDownward,
            WarehouseActionType.Out => Icons.Material.Outlined.ArrowUpward,
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private Color GetActionColor(string sourceUrl)
    {
        if (sourceUrl.StartsWith(ClientRoutes.Invoices.Index))
            return Color.Info;

        if (sourceUrl.StartsWith(ClientRoutes.InventoryStocks.InventoryEntry.Index))
            return Color.Success;

        if (sourceUrl.StartsWith(ClientRoutes.InventoryStocks.MeltingBatches.Index))
            return Color.Primary;

        throw new NotImplementedException();
    }

    private string GetActionText(string sourceUrl)
    {
        if (sourceUrl.StartsWith(ClientRoutes.Invoices.Index))
            return "مشاهده فاکتور";

        if (sourceUrl.StartsWith(ClientRoutes.InventoryStocks.InventoryEntry.Index))
            return "مشاهده ورود انبار";

        if (sourceUrl.StartsWith(ClientRoutes.InventoryStocks.MeltingBatches.Index))
            return "مشاهده درخواست ذوب طلا";

        throw new NotImplementedException();
    }
}