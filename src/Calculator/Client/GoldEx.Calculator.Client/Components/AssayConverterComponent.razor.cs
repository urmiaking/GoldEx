using GoldEx.Calculator.Client.Services;
using GoldEx.Calculator.Client.ViewModels;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Calculator.Client.Components;

public partial class AssayConverterComponent
{
    [Parameter] public int Elevation { get; set; } = 24;
    [Parameter] public string? Class { get; set; }

    [Inject] private CalculationHistoryStore HistoryStore { get; set; } = default!;
    [Inject] private ISnackbar Snackbar { get; set; } = default!;

    private readonly AssayConverterVm _model = new();

    private void AddBatchItem()
    {
        _model.BatchItems.Add(new AssayBatchItem
        {
            Title = $"قطعه {_model.BatchItems.Count + 1}",
            Weight = 10m,
            Fineness = 750m
        });
    }

    private void RemoveBatchItem(int index)
    {
        if (index >= 0 && index < _model.BatchItems.Count && _model.BatchItems.Count > 1)
        {
            _model.BatchItems.RemoveAt(index);
        }
    }

    private async Task SaveSingleToHistoryAsync()
    {
        await HistoryStore.AddAsync(new CalculationHistoryItem
        {
            Title = $"تبدیل عیار {_model.InputFineness} به {_model.TargetFineness}",
            Category = "عیارسنجی",
            SummaryText = $"وزن اولیه: {_model.InputWeight:G29}g با عیار {_model.InputFineness}",
            ResultValue = $"{_model.ConvertedWeight:F3}",
            Unit = $"گرم (عیار {_model.TargetFineness})",
            Details = new Dictionary<string, string>
            {
                ["وزن اولیه"] = $"{_model.InputWeight:G29} گرم",
                ["عیار اولیه"] = $"{_model.InputFineness}",
                ["عیار هدف"] = $"{_model.TargetFineness}",
                ["وزن معادل"] = $"{_model.ConvertedWeight:F3} گرم",
                ["معادل به شرط ۷۵۰"] = $"{_model.Equivalent750Weight:F3} گرم",
                ["طلای ۲۴ عیار خالص"] = $"{_model.PureGoldWeight:F3} گرم"
            }
        });

        Snackbar.Add("نتیجه تبدیل عیار در تاریخچه ثبت شد", Severity.Success);
    }

    private async Task SaveBatchToHistoryAsync()
    {
        await HistoryStore.AddAsync(new CalculationHistoryItem
        {
            Title = $"انگ آبشده ({_model.BatchItems.Count} قطعه)",
            Category = "عیارسنجی",
            SummaryText = $"وزن کل: {_model.BatchTotalWeight:F3}g | عیار میانگین: {_model.BatchAverageFineness:F1}",
            ResultValue = $"{_model.BatchAverageFineness:F1}",
            Unit = "خط انگ",
            Details = new Dictionary<string, string>
            {
                ["تعداد قطعات"] = $"{_model.BatchItems.Count}",
                ["وزن کل قطعات"] = $"{_model.BatchTotalWeight:F3} گرم",
                ["مجموع به شرط ۷۵۰"] = $"{_model.BatchTotal750Weight:F3} گرم",
                ["عیار میانگین مخلوط"] = $"{_model.BatchAverageFineness:F1}"
            }
        });

        Snackbar.Add("محاسبه انگ آبشده در تاریخچه ثبت شد", Severity.Success);
    }
}
