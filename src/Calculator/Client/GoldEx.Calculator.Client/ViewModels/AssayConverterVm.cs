namespace GoldEx.Calculator.Client.ViewModels;

public sealed class AssayBatchItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..6];
    public string Title { get; set; } = "قطعه";
    public decimal Weight { get; set; } = 10m;
    public decimal Fineness { get; set; } = 750m;

    public decimal PureGoldWeight => Weight * (Fineness / 1000m);
    public decimal Equivalent750Weight => Fineness > 0 ? (Weight * Fineness) / 750m : 0m;
}

public sealed class AssayConverterVm
{
    // حالت تبدیل مستقیم تک قطعه
    public decimal InputWeight { get; set; } = 10m;
    public decimal InputFineness { get; set; } = 735m;
    public decimal TargetFineness { get; set; } = 750m;

    // وزن معادل بر اساس عیار هدف
    public decimal ConvertedWeight =>
        TargetFineness > 0
            ? (InputWeight * InputFineness) / TargetFineness
            : 0m;

    public decimal Equivalent750Weight =>
        (InputWeight * InputFineness) / 750m;

    public decimal PureGoldWeight =>
        InputWeight * (InputFineness / 1000m);

    // حالت ترکیب قطعات (آبشده / ری‌گیری چند قطعه)
    public List<AssayBatchItem> BatchItems { get; set; } =
    [
        new() { Title = "قطعه اول", Weight = 15.42m, Fineness = 740m },
        new() { Title = "قطعه دوم", Weight = 28.65m, Fineness = 765m }
    ];

    public decimal BatchTotalWeight => BatchItems.Sum(x => x.Weight);

    public decimal BatchTotalPureGold => BatchItems.Sum(x => x.PureGoldWeight);

    public decimal BatchTotal750Weight => BatchItems.Sum(x => x.Equivalent750Weight);

    public decimal BatchAverageFineness =>
        BatchTotalWeight > 0
            ? (BatchTotalPureGold / BatchTotalWeight) * 1000m
            : 0m;
}
