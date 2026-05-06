using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MudBlazor;

namespace GoldEx.Client.Pages.Dashboard.Components;

public partial class InventoryWeightChart
{
    private WarehouseActionType _selectedActionType = WarehouseActionType.Out;
    private List<GetInventoryWeightChartResponse>? _chartResponse;
    private double[] _chartData = [];
    private string[] _chartLabels = [];
    private List<ChartSeries<double>> _chartSeries = [];

    private string CardTitle => _selectedActionType is WarehouseActionType.In
        ? "ارزش اجناس موجود (گرم)"
        : "ارزش اجناس فروخته شده (گرم)";

    protected override async Task OnInitializedAsync()
    {
        await LoadChartAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadChartAsync()
    {
        _chartResponse = null;

        await SendRequestAsync<IInventoryStockService, List<GetInventoryWeightChartResponse>>(
            action: (s, ct) => s.GetInventoryWeightChartAsync(_selectedActionType, ct),
            afterSend: response =>
            {
                _chartResponse = response;
                UpdateChartData();
            });
    }

    private async Task OnActionTypeChanged(WarehouseActionType newActionType)
    {
        _selectedActionType = newActionType;
        await LoadChartAsync();
    }

    private void UpdateChartData()
    {
        if (_chartResponse == null) return;

        _chartData = _chartResponse.Select(x => (double)x.Weight).ToArray();
        _chartLabels = _chartResponse.Select(x => x.Label).ToArray();
        _chartSeries = [new ChartSeries<double> { Data = _chartData }];
    }

    private string GetDisplayName(WarehouseActionType actionType)
    {
        switch(actionType)
        {
            case WarehouseActionType.In:
                return "موجودی فعلی";
            case WarehouseActionType.Out:
                return "فروخته شده";
            default:
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, null);
        }
    }
}