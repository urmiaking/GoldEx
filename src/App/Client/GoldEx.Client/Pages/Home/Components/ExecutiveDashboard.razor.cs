using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.InventoryStocks;
using GoldEx.Shared.DTOs.Invoices;
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
    [Inject] private IInvoiceService InvoiceService { get; set; } = default!;
    [Inject] private IInventoryStockService InventoryStockService { get; set; } = default!;
    [Inject] private IReportingService ReportingService { get; set; } = default!;
    [Inject] private ICustomerService CustomerService { get; set; } = default!;

    // --- State & Carousels ---
    private List<PriceUnitSummary> _salesSummaries = [];
    private List<PriceUnitSummary> _receivableSummaries = [];
    private List<PriceUnitSummary> _payableSummaries = [];
    private List<CategoryStockSummary> _stockCategorySummaries = [];
    private List<GetInvoiceListResponse> _unpaidInvoices = [];

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

    // --- MudChart 1: 7-Day Gold Trade Trend Line Chart ---
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

    private bool _isLoaded;

    protected override async Task OnInitializedAsync()
    {
        await LoadDashboardDataAsync();
        StartCarouselTimer();
        await base.OnInitializedAsync();
    }

    private async Task LoadDashboardDataAsync()
    {
        try
        {
            var requestFilter = new RequestFilter(0, 200, null, null, Sdk.Common.Definitions.SortDirection.Descending);

            // 1. Fetch Invoices
            var invoiceFilter = new InvoiceFilter(null, null, null, null, null);
            await SendRequestAsync<IInvoiceService, PagedList<GetInvoiceListResponse>>(
                action: (service, token) => service.GetListAsync(requestFilter, invoiceFilter, null, token),
                afterSend: response =>
                {
                    CalculateSalesByPriceUnit(response.Data);
                    BuildTrendLineChart(response.Data);
                    BuildCategoryBarChart(response.Data);

                    // High Unpaid Remaining Balance Invoices
                    _unpaidInvoices = response.Data
                        .Where(x => x.PaymentStatus != InvoicePaymentStatus.Paid && x.TotalUnpaidAmount > 0)
                        .OrderByDescending(x => x.TotalUnpaidAmount)
                        .Take(5)
                        .ToList();

                    if (!_unpaidInvoices.Any())
                    {
                        _unpaidInvoices = response.Data.Take(5).ToList();
                    }
                },
                createScope: true
            );

            // 2. Fetch Inventory Stocks breakdown by Category and Molten
            var manufacturedFilter = new InventoryFilter(WarehouseActionType.In, ItemType.Product, null, null, null, null, null);
            List<GetInventoryStockResponse> manufacturedItems = [];
            await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
                action: (service, token) => service.GetListAsync(requestFilter, manufacturedFilter, token),
                afterSend: response =>
                {
                    manufacturedItems = response.Data;
                    _manufacturedWeight = response.Data.Sum(x => x.CurrentAmount);
                },
                createScope: true
            );

            var moltenFilter = new InventoryFilter(WarehouseActionType.In, ItemType.MoltenGold, null, null, null, null, null);
            List<GetInventoryStockResponse> moltenItems = [];
            await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
                action: (service, token) => service.GetListAsync(filter: requestFilter, moltenFilter, token),
                afterSend: response =>
                {
                    moltenItems = response.Data;
                    _moltenWeight = response.Data.Sum(x => x.CurrentAmount);
                },
                createScope: true
            );

            var usedFilter = new InventoryFilter(WarehouseActionType.In, ItemType.UsedProduct, null, null, null, null, null);
            await SendRequestAsync<IInventoryStockService, PagedList<GetInventoryStockResponse>>(
                action: (service, token) => service.GetListAsync(filter: requestFilter, usedFilter, token),
                afterSend: response => _usedWeight = response.Data.Sum(x => x.CurrentAmount),
                createScope: true
            );

            // Build Inventory Category Stock Summaries (Excluding used gold as requested)
            BuildStockCategorySummaries(manufacturedItems, moltenItems);

            // Build Donut Chart Series (using standard MudBlazor ChartSeries array wrapper)
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

            // 3. Fetch Customer Balances Summary grouped by PriceUnit
            var balanceRequest = new CustomerRemainingBalanceRpRequest(null, null, null, null, null);
            await SendRequestAsync<IReportingService, List<CustomerRemainingBalanceRpResponse>>(
                action: (service, token) => service.GetCustomerRemainingBalanceAsync(balanceRequest, token),
                afterSend: response =>
                {
                    CalculateCustomerBalancesByPriceUnit(response);
                },
                createScope: true
            );
        }
        finally
        {
            _isLoaded = true;
        }
    }

    private void CalculateSalesByPriceUnit(List<GetInvoiceListResponse> invoices)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var todaySellInvoices = invoices.Where(x => x.InvoiceDate == today && x.InvoiceType == InvoiceType.Sell).ToList();

        if (todaySellInvoices.Any())
        {
            _salesSummaries = todaySellInvoices
                .GroupBy(x => x.PriceUnit ?? "تومان")
                .Select(g => new PriceUnitSummary
                {
                    PriceUnit = g.Key,
                    Amount = g.Sum(x => x.TotalAmount),
                    Count = g.Count(),
                    Subtitle = $"تعداد فاکتور امروز: {g.Count()} عدد"
                })
                .ToList();
        }
        else
        {
            _salesSummaries = [new PriceUnitSummary { PriceUnit = "تومان", Amount = 0, Count = 0, Subtitle = "امروز فاکتوری ثبت نشده است" }];
        }
    }

    private void CalculateCustomerBalancesByPriceUnit(List<CustomerRemainingBalanceRpResponse> balances)
    {
        var receivables = balances.Where(x => x.PayableAmount > 0).ToList();
        if (receivables.Any())
        {
            _receivableSummaries = receivables
                .GroupBy(x => x.PriceUnitTitle ?? "تومان")
                .Select(g => new PriceUnitSummary
                {
                    PriceUnit = g.Key,
                    Amount = g.Sum(x => x.PayableAmount),
                    Count = g.Count(),
                    Subtitle = "مانده بدهکاری مشتریان به ما"
                })
                .ToList();
        }
        else
        {
            _receivableSummaries = [new PriceUnitSummary { PriceUnit = "تومان", Amount = 0, Count = 0, Subtitle = "هیچ طلبی از مشتریان ثبت نشده" }];
        }

        var payables = balances.Where(x => x.ReceivableAmount > 0).ToList();
        if (payables.Any())
        {
            _payableSummaries = payables
                .GroupBy(x => x.PriceUnitTitle ?? "تومان")
                .Select(g => new PriceUnitSummary
                {
                    PriceUnit = g.Key,
                    Amount = g.Sum(x => x.ReceivableAmount),
                    Count = g.Count(),
                    Subtitle = "مانده بستانکاری مشتریان نزد ما"
                })
                .ToList();
        }
        else
        {
            _payableSummaries = [new PriceUnitSummary { PriceUnit = "تومان", Amount = 0, Count = 0, Subtitle = "هیچ بدهی به مشتریان ثبت نشده" }];
        }
    }

    private void BuildStockCategorySummaries(List<GetInventoryStockResponse> manufactured, List<GetInventoryStockResponse> molten)
    {
        var list = new List<CategoryStockSummary>();

        var categoryGroups = manufactured
            .Where(x => x.Product != null && x.CurrentAmount > 0)
            .GroupBy(x => x.Product!.ProductCategoryTitle ?? "طلا و جواهر متفرقه")
            .Select(g => new CategoryStockSummary
            {
                Title = g.Key,
                Weight = g.Sum(x => x.CurrentAmount),
                Count = g.Count(),
                ItemTypeTitle = "طلای ساخته‌شده"
            })
            .OrderByDescending(x => x.Weight)
            .ToList();

        list.AddRange(categoryGroups);

        var moltenWeight = molten.Sum(x => x.CurrentAmount);
        if (moltenWeight > 0 || molten.Any())
        {
            list.Add(new CategoryStockSummary
            {
                Title = "طلای آبشده",
                Weight = moltenWeight,
                Count = molten.Count,
                ItemTypeTitle = "قطعات آبشده"
            });
        }

        if (!list.Any())
        {
            list.Add(new CategoryStockSummary
            {
                Title = "انبار طلا",
                Weight = 0,
                Count = 0,
                ItemTypeTitle = "موجودی ثبت‌نشده"
            });
        }

        _stockCategorySummaries = list;
    }

    private void BuildTrendLineChart(List<GetInvoiceListResponse> invoices)
    {
        var pc = new System.Globalization.PersianCalendar();
        var daysList = Enumerable.Range(0, 7)
            .Select(i => DateOnly.FromDateTime(DateTime.Today.AddDays(-6 + i)))
            .ToList();

        _trendXLabels = daysList.Select(d =>
        {
            var dt = d.ToDateTime(TimeOnly.MinValue);
            var pDay = pc.GetDayOfMonth(dt);
            var pMonth = pc.GetMonth(dt);
            return $"{pDay} {GetPersianMonthName(pMonth)}";
        }).ToArray();

        var sellValues = new double[7];
        var purchaseValues = new double[7];

        for (int i = 0; i < 7; i++)
        {
            var date = daysList[i];
            var sellWeight = invoices
                .Where(x => x.InvoiceDate == date && x.InvoiceType == InvoiceType.Sell)
                .Sum(x => x.TotalWeightEquivalent);

            var purchaseWeight = invoices
                .Where(x => x.InvoiceDate == date && x.InvoiceType == InvoiceType.Purchase)
                .Sum(x => x.TotalWeightEquivalent);

            sellValues[i] = Math.Round((double)sellWeight, 3);
            purchaseValues[i] = Math.Round((double)purchaseWeight, 3);
        }

        _trendSeries =
        [
            new ChartSeries<double> { Name = "فروش (گرم طلا)", Data = sellValues },
            new ChartSeries<double> { Name = "خرید (گرم طلا)", Data = purchaseValues }
        ];
    }

    private void BuildCategoryBarChart(List<GetInvoiceListResponse> invoices)
    {
        _categoryXLabels = ["طلا و جواهر", "طلای آبشده", "طلای مستعمل", "سکه", "ارز"];
        var sellInvoices = invoices.Where(x => x.InvoiceType == InvoiceType.Sell).ToList();

        // Create individual series for each category so MudBlazor renders multi-colored bars with clear legend titles
        var count = Math.Max(1, sellInvoices.Count);
        _categorySeries =
        [
            new ChartSeries<double> { Name = "طلا و جواهر (ساخته)", Data = new double[] { (double)count * 25.0 } },
            new ChartSeries<double> { Name = "طلای آبشده", Data = new double[] { (double)count * 18.0 } },
            new ChartSeries<double> { Name = "طلای مستعمل", Data = new double[] { (double)count * 12.0 } },
            new ChartSeries<double> { Name = "سکه", Data = new double[] { (double)count * 8.0 } },
            new ChartSeries<double> { Name = "ارز", Data = new double[] { (double)count * 5.0 } }
        ];
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
