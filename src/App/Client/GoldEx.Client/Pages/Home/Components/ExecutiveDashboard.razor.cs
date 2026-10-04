using GoldEx.Shared.DTOs.Dashboard;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Timers;
using Timer = System.Timers.Timer;

namespace GoldEx.Client.Pages.Home.Components;

public partial class ExecutiveDashboard : IAsyncDisposable
{
    [Inject] private IDashboardService DashboardService { get; set; } = default!;
    [Inject] private IInventoryStockService InventoryStockService { get; set; } = default!;
    [Inject] private IReportingService ReportingService { get; set; } = default!;

    // --- Progressive Loading Flags ---
    private bool _isSalesLoaded;
    private bool _isInventoryLoaded;
    private bool _isCustomerBalancesLoaded;
    private bool _isTrendLoaded;
    private bool _isCategoryLoaded;
    private bool _isUnpaidLoaded;

    // --- State & Carousels ---
    private List<PriceUnitSummary> _salesSummaries = [];
    private List<PriceUnitSummary> _receivableSummaries = [];
    private List<PriceUnitSummary> _payableSummaries = [];
    private List<CategoryStockSummary> _stockCategorySummaries = [];
    private List<TopUnpaidInvoiceDto> _unpaidInvoices = [];

    private int _salesIndex;
    private int _receivableIndex;
    private int _payableIndex;
    private int _stockCategoryIndex;

    private Timer? _carouselTimer;

    // --- Totals for Charts ---
    private decimal _manufacturedWeight;
    private decimal _moltenWeight;
    private decimal _usedWeight;
    private decimal TotalStockGoldWeight => _manufacturedWeight + _moltenWeight + _usedWeight;

    // --- MudChart 1: 30-Day Gold Trade Trend Line Chart ---
    private string[] _trendXLabels = [];
    private List<ChartSeries<double>> _trendSeries = [];
    private readonly LineChartOptions _lineChartOptions = new()
    {
        InterpolationOption = InterpolationOption.NaturalSpline,
        LineStrokeWidth = 3
    };

    // --- MudChart 2: Inventory Capital Donut Chart ---
    private List<ChartSeries<double>> _donutSeries = [];
    private string[] _donutLabels = ["طلای ساخته‌شده", "طلای آبشده", "طلای مستعمل"];

    // --- MudChart 3: Sales by Product Category Bar Chart ---
    private string[] _categoryXLabels = ["طلا و جواهر", "طلای آبشده", "طلای مستعمل", "سکه", "ارز"];
    private List<ChartSeries<double>> _categorySeries = [];
    private readonly ChartOptions _barChartOptions = new()
    {
        ShowLegend = true
    };

    protected override async Task OnInitializedAsync()
    {
        StartCarouselTimer();

        // Launch progressive parallel loading for all dashboard widgets
        _ = LoadTodaySalesAsync();
        _ = LoadInventoryOverviewAsync();
        _ = LoadCustomerBalancesAsync();
        _ = LoadTradeTrendAsync();
        _ = LoadCategorySalesAsync();
        _ = LoadTopUnpaidInvoicesAsync();

        await base.OnInitializedAsync();
    }

    private async Task LoadTodaySalesAsync()
    {
        try
        {
            await SendRequestAsync<IDashboardService, List<TodaySalesSummaryDto>>(
                action: (service, token) => service.GetTodaySalesAsync(token),
                afterSend: response =>
                {
                    _salesSummaries = response.Select(x => new PriceUnitSummary
                    {
                        PriceUnit = x.PriceUnit,
                        Amount = x.Amount,
                        Count = x.Count,
                        Subtitle = x.Subtitle
                    }).ToList();
                },
                createScope: true
            );
        }
        finally
        {
            _isSalesLoaded = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadInventoryOverviewAsync()
    {
        try
        {
            await SendRequestAsync<IInventoryStockService, GetInventoryOverviewResponse>(
                action: (service, token) => service.GetInventoryOverviewAsync(token),
                afterSend: response =>
                {
                    _manufacturedWeight = response.ManufacturedGoldWeight;
                    _moltenWeight = response.MoltenGoldWeight;
                    _usedWeight = response.UsedGoldWeight;

                    _stockCategorySummaries =
                    [
                        new CategoryStockSummary
                        {
                            Title = "طلای ساخته‌شده",
                            Weight = response.ManufacturedGoldWeight,
                            Count = response.ManufacturedGoldCount,
                            ItemTypeTitle = "طلا و جواهر"
                        },
                        new CategoryStockSummary
                        {
                            Title = "طلای آبشده",
                            Weight = response.MoltenGoldWeight,
                            Count = response.MoltenGoldCount,
                            ItemTypeTitle = "قطعات آبشده"
                        }
                    ];

                    if (response.UsedGoldWeight > 0 || response.UsedGoldCount > 0)
                    {
                        _stockCategorySummaries.Add(new CategoryStockSummary
                        {
                            Title = "طلای مستعمل",
                            Weight = response.UsedGoldWeight,
                            Count = response.UsedGoldCount,
                            ItemTypeTitle = "مستعمل و متفرقه"
                        });
                    }

                    // Build Donut Chart Series
                    var w1 = (double)_manufacturedWeight;
                    var w2 = (double)_moltenWeight;
                    var w3 = (double)_usedWeight;

                    if (w1 == 0 && w2 == 0 && w3 == 0)
                    {
                        _donutSeries = [new ChartSeries<double> { Data = new double[] { 40, 35, 25 } }];
                    }
                    else
                    {
                        _donutSeries = [new ChartSeries<double> { Data = new double[] { w1 > 0 ? w1 : 1, w2 > 0 ? w2 : 1, w3 > 0 ? w3 : 1 } }];
                    }
                },
                createScope: true
            );
        }
        finally
        {
            _isInventoryLoaded = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadCustomerBalancesAsync()
    {
        try
        {
            await SendRequestAsync<IDashboardService, CustomerBalancesSummaryDto>(
                action: (service, token) => service.GetCustomerBalancesSummaryAsync(token),
                afterSend: response =>
                {
                    _receivableSummaries = response.Receivables.Select(x => new PriceUnitSummary
                    {
                        PriceUnit = x.PriceUnit,
                        Amount = x.Amount,
                        Count = x.Count,
                        Subtitle = x.Subtitle
                    }).ToList();

                    _payableSummaries = response.Payables.Select(x => new PriceUnitSummary
                    {
                        PriceUnit = x.PriceUnit,
                        Amount = x.Amount,
                        Count = x.Count,
                        Subtitle = x.Subtitle
                    }).ToList();
                },
                createScope: true
            );
        }
        finally
        {
            _isCustomerBalancesLoaded = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadTradeTrendAsync()
    {
        try
        {
            await SendRequestAsync<IDashboardService, List<TradeTrendPointDto>>(
                action: (service, token) => service.GetTradeTrend30DaysAsync(token),
                afterSend: points =>
                {
                    var pc = new System.Globalization.PersianCalendar();
                    _trendXLabels = points.Select(p =>
                    {
                        var dt = p.Date.ToDateTime(TimeOnly.MinValue);
                        return pc.GetDayOfMonth(dt).ToString();
                    }).ToArray();

                    var sellValues = points.Select(p => (double)Math.Round(p.SellWeight, 3)).ToArray();
                    var purchaseValues = points.Select(p => (double)Math.Round(p.PurchaseWeight, 3)).ToArray();

                    _trendSeries =
                    [
                        new ChartSeries<double> { Name = "فروش (گرم طلا)", Data = sellValues },
                        new ChartSeries<double> { Name = "خرید (گرم طلا)", Data = purchaseValues }
                    ];
                },
                createScope: true
            );
        }
        finally
        {
            _isTrendLoaded = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadCategorySalesAsync()
    {
        try
        {
            var categorySalesRequest = new CategorySalesRpRequest(null, null, null, null);
            await SendRequestAsync<IReportingService, List<CategorySalesRpResponse>>(
                action: (service, token) => service.GetCategorySalesSummaryAsync(categorySalesRequest, token),
                afterSend: response => BuildCategoryBarChart(response),
                createScope: true
            );
        }
        finally
        {
            _isCategoryLoaded = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task LoadTopUnpaidInvoicesAsync()
    {
        try
        {
            await SendRequestAsync<IDashboardService, List<TopUnpaidInvoiceDto>>(
                action: (service, token) => service.GetTopUnpaidInvoicesAsync(5, token),
                afterSend: response =>
                {
                    _unpaidInvoices = response;
                },
                createScope: true
            );
        }
        finally
        {
            _isUnpaidLoaded = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void BuildCategoryBarChart(List<CategorySalesRpResponse> categorySales)
    {
        var topCategories = categorySales
            .Where(x => x.TotalWeight > 0)
            .OrderByDescending(x => x.TotalWeight)
            .Take(7)
            .ToList();

        if (topCategories.Any())
        {
            _categoryXLabels = topCategories.Select(x => x.CategoryTitle).ToArray();
            var values = topCategories.Select(x => (double)Math.Round(x.TotalWeight, 2)).ToArray();
            _categorySeries =
            [
                new ChartSeries<double> { Name = "فروش (گرم طلا)", Data = values }
            ];
        }
        else
        {
            _categoryXLabels = ["النگو", "انگشتر", "دستبند", "طلای آبشده", "گردنبند", "گوشواره"];
            _categorySeries =
            [
                new ChartSeries<double> { Name = "فروش (گرم طلا)", Data = new double[] { 0, 0, 0, 0, 0, 0 } }
            ];
        }
    }

    private void StartCarouselTimer()
    {
        StopCarouselTimer();
        _carouselTimer = new Timer(4000); // 4 seconds
        _carouselTimer.Elapsed += OnCarouselTimerElapsed;
        _carouselTimer.AutoReset = true;
        _carouselTimer.Start();
    }

    private void StopCarouselTimer()
    {
        _carouselTimer?.Stop();
        _carouselTimer?.Dispose();
        _carouselTimer = null;
    }

    private async void OnCarouselTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await InvokeAsync(() =>
        {
            if (_salesSummaries.Count > 1) _salesIndex = (_salesIndex + 1) % _salesSummaries.Count;
            if (_receivableSummaries.Count > 1) _receivableIndex = (_receivableIndex + 1) % _receivableSummaries.Count;
            if (_payableSummaries.Count > 1) _payableIndex = (_payableIndex + 1) % _payableSummaries.Count;
            if (_stockCategorySummaries.Count > 1) _stockCategoryIndex = (_stockCategoryIndex + 1) % _stockCategorySummaries.Count;

            StateHasChanged();
        });
    }

    private void ToggleSalesIndex()
    {
        if (_salesSummaries.Count > 1)
        {
            _salesIndex = (_salesIndex + 1) % _salesSummaries.Count;
            StateHasChanged();
        }
    }

    private void ToggleReceivableIndex()
    {
        if (_receivableSummaries.Count > 1)
        {
            _receivableIndex = (_receivableIndex + 1) % _receivableSummaries.Count;
            StateHasChanged();
        }
    }

    private void TogglePayableIndex()
    {
        if (_payableSummaries.Count > 1)
        {
            _payableIndex = (_payableIndex + 1) % _payableSummaries.Count;
            StateHasChanged();
        }
    }

    private void ToggleStockCategoryIndex()
    {
        if (_stockCategorySummaries.Count > 1)
        {
            _stockCategoryIndex = (_stockCategoryIndex + 1) % _stockCategorySummaries.Count;
            StateHasChanged();
        }
    }

    public override async ValueTask DisposeAsync()
    {
        StopCarouselTimer();
        await base.DisposeAsync();
    }

    private static string GetPersianMonthName(int month) => month switch
    {
        1 => "فروردین",
        2 => "اردیبهشت",
        3 => "خرداد",
        4 => "تیر",
        5 => "مرداد",
        6 => "شهریور",
        7 => "مهر",
        8 => "آبان",
        9 => "آذر",
        10 => "دی",
        11 => "بهمن",
        12 => "اسفند",
        _ => ""
    };
}

public class PriceUnitSummary
{
    public string PriceUnit { get; set; } = default!;
    public decimal Amount { get; set; }
    public int Count { get; set; }
    public string Subtitle { get; set; } = default!;
}

public class CategoryStockSummary
{
    public string Title { get; set; } = default!;
    public decimal Weight { get; set; }
    public int Count { get; set; }
    public string ItemTypeTitle { get; set; } = default!;
}
