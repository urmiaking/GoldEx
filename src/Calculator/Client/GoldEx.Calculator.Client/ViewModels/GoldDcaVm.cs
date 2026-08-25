namespace GoldEx.Calculator.Client.ViewModels;

public sealed class DcaPurchaseItem
{
    public string Id { get; init; } = Guid.NewGuid().ToString("N")[..6];
    public string Title { get; set; } = "پله خرید";
    public DateTime Date { get; set; } = DateTime.Today;
    public decimal WeightGrams { get; set; } = 5m;
    public decimal BuyGramPrice { get; set; } = 4500000m;

    public decimal TotalCost => WeightGrams * BuyGramPrice;
}

public sealed class GoldDcaVm
{
    public decimal LiveGramPrice { get; set; } = 5200000m;
    public decimal SalesFeePercent { get; set; } = 1.5m; // درصد تخمینی کسر کارمزد/سود موقع فروش

    public List<DcaPurchaseItem> Purchases { get; set; } =
    [
        new() { Title = "پله اول (فروردین)", WeightGrams = 10m, BuyGramPrice = 4200000m },
        new() { Title = "پله دوم (خرداد)", WeightGrams = 15m, BuyGramPrice = 4650000m },
        new() { Title = "پله سوم (مرداد)", WeightGrams = 8.5m, BuyGramPrice = 4900000m }
    ];

    public decimal TotalWeight => Purchases.Sum(x => x.WeightGrams);

    public decimal TotalInvested => Purchases.Sum(x => x.TotalCost);

    public decimal WeightedAverageBuyPrice =>
        TotalWeight > 0 ? TotalInvested / TotalWeight : 0m;

    public decimal CurrentMarketValue => TotalWeight * LiveGramPrice;

    public decimal NetProfitLossAmount => CurrentMarketValue - TotalInvested;

    public decimal NetProfitLossPercent =>
        TotalInvested > 0 ? (NetProfitLossAmount / TotalInvested) * 100m : 0m;

    public decimal BreakEvenGramPrice =>
        SalesFeePercent < 100m && SalesFeePercent >= 0m
            ? WeightedAverageBuyPrice / (1m - (SalesFeePercent / 100m))
            : WeightedAverageBuyPrice;
}
