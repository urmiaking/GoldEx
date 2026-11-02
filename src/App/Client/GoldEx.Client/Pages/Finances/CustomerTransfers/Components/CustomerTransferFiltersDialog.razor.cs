using GoldEx.Client.Components.Services;
using GoldEx.Shared.DTOs.PriceUnits;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Finances.CustomerTransfers.Components;

public partial class CustomerTransferFiltersDialog
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [Parameter] public DateRange DateRange { get; set; } = new();
    [Parameter] public List<GetPriceUnitTitleResponse> PriceUnits { get; set; } = [];
    [Parameter] public Guid? PriceUnitId { get; set; }
    [Parameter] public string SortLabel { get; set; } = "TransferDate";
    [Parameter] public bool SortDescending { get; set; } = true;

    private DateRange _dateRange = new();
    private Guid? _unitId;
    private string _sortLabel = "TransferDate";
    private bool _sortDescending = true;

    public record FilterResult(DateRange DateRange, Guid? PriceUnitId, string SortLabel, bool SortDescending);

    protected override void OnParametersSet()
    {
        _dateRange = new DateRange(DateRange.Start, DateRange.End);
        _unitId = PriceUnitId;
        _sortLabel = SortLabel;
        _sortDescending = SortDescending;
    }

    private void Apply() => Session?.Close(new FilterResult(
        new DateRange(_dateRange.Start, _dateRange.End), _unitId, _sortLabel, _sortDescending));
    private void Clear()
    {
        _dateRange = new DateRange();
        _unitId = null;
        Apply();
    }
    private void Cancel() => Session?.Cancel();
}