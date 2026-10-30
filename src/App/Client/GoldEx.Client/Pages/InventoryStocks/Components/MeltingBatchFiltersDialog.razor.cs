using GoldEx.Client.Components.Services;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Components;
using MudBlazor;
namespace GoldEx.Client.Pages.InventoryStocks.Components;
public partial class MeltingBatchFiltersDialog
{
    [CascadingParameter] private ResponsiveDialogSession? Session { get; set; }
    [Parameter] public DateRange DateRange { get; set; } = new();
    [Parameter] public MeltingBatchStatus? Status { get; set; }
    [Parameter] public string SortLabel { get; set; } = "BatchNumber";
    [Parameter] public bool SortDescending { get; set; } = true;
    private DateRange _dateRange = new();
    private MeltingBatchStatus? _status;
    private string _sort = "BatchNumber";
    private bool _descending = true;
    public record FilterResult(DateRange DateRange, MeltingBatchStatus? Status, string SortLabel, bool SortDescending);
    protected override void OnParametersSet()
    {
        _dateRange = new DateRange(DateRange.Start, DateRange.End);
        _status = Status;
        _sort = SortLabel;
        _descending = SortDescending;
    }
    private void Apply() => Session?.Close(new FilterResult(new(_dateRange.Start, _dateRange.End), _status, _sort, _descending));
    private void Clear()
    {
        _dateRange = new();
        _status = null;
        Apply();
    }
}