using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Client.Pages.Dashboard.Components;

public partial class InventoryWeightChart
{
    private GoldUnitType _selectedUnitType = GoldUnitType.Gram;
    private List<GetInventoryWeightChartResponse>? _chartResponse;
    private double[] _chartData = [];
    private string[] _chartLabels = [];

    protected override async Task OnInitializedAsync()
    {
        await LoadChartAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadChartAsync()
    {
        await SendRequestAsync<IInventoryStockService, List<GetInventoryWeightChartResponse>>(
            action: (s, ct) => s.GetInventoryWeightChartAsync(_selectedUnitType, ct),
            afterSend: response =>
            {
                _chartResponse = response;
                UpdateChartData();
            });
    }

    private async Task OnUnitTypeChanged(GoldUnitType newUnit)
    {
        _selectedUnitType = newUnit;
        await LoadChartAsync();
    }

    private void UpdateChartData()
    {
        if (_chartResponse == null) return;

        _chartData = _chartResponse.Select(x => (double)x.Weight).ToArray();
        _chartLabels = _chartResponse.Select(x => x.Label).ToArray();
    }
}