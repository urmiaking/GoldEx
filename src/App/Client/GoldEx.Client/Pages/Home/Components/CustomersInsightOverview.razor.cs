using System.Timers;
using GoldEx.Sdk.Common.Data;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Timer = System.Timers.Timer;

namespace GoldEx.Client.Pages.Home.Components;

public partial class CustomersInsightOverview : IAsyncDisposable
{
    [Inject] private ICustomerService CustomerService { get; set; } = default!;
    [Inject] private IReportingService ReportingService { get; set; } = default!;

    private List<GetCustomerResponse> _customers = [];
    private List<CustomerBalanceRpSummary> _overallBalances = [];
    private int _currentBalanceIndex = 0;
    private Timer? _slideTimer;

    private int _totalCustomersCount;
    private int TotalCustomersCount => _totalCustomersCount > 0 ? _totalCustomersCount : _customers.Count;

    private int RetailCustomersCount => _customers
        .Count(x => x.CustomerType == CustomerType.RetailCustomer || x.CustomerType == CustomerType.Retailer);

    private int WholesaleCustomersCount => _customers
        .Count(x => x.CustomerType == CustomerType.Wholesaler || x.CustomerType == CustomerType.MeltedGoldDealer || x.CustomerType == CustomerType.Workshop);

    private CustomerBalanceRpSummary? CurrentOverallBalance =>
        _overallBalances.Count > 0 ? _overallBalances[_currentBalanceIndex % _overallBalances.Count] : null;

    protected override async Task OnInitializedAsync()
    {
        await LoadCustomersAsync();
        await LoadOverallCustomerBalancesAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadCustomersAsync()
    {
        var filter = new RequestFilter(0, int.MaxValue, null, null, Sdk.Common.Definitions.SortDirection.Descending);
        var customerFilter = new CustomerFilter(null, null, null, null);

        await SendRequestAsync<ICustomerService, PagedList<GetCustomerResponse>>(
            action: (service, token) => service.GetListAsync(filter, customerFilter, token),
            afterSend: response =>
            {
                _customers = response.Data;
                _totalCustomersCount = response.Total;
            },
            createScope: true
        );
    }

    private async Task LoadOverallCustomerBalancesAsync()
    {
        var request = new CustomerRemainingBalanceRpRequest(null, null, null, null, null);

        await SendRequestAsync<IReportingService, List<CustomerRemainingBalanceRpResponse>>(
            action: (service, token) => service.GetCustomerRemainingBalanceAsync(request, token),
            afterSend: response =>
            {
                if (response != null && response.Any())
                {
                    _overallBalances = response
                        .GroupBy(x => x.PriceUnitTitle)
                        .Select(g => new CustomerBalanceRpSummary
                        {
                            PriceUnitTitle = g.Key,
                            NetBalance = g.Sum(x => x.PayableAmount - x.ReceivableAmount),
                            TotalReceivable = g.Sum(x => x.PayableAmount),
                            TotalPayable = g.Sum(x => x.ReceivableAmount)
                        })
                        .ToList();

                    if (_overallBalances.Count > 1)
                    {
                        StartSlideTimer();
                    }
                }
            },
            createScope: true
        );
    }

    private void StartSlideTimer()
    {
        StopSlideTimer();
        _slideTimer = new Timer(4000); // Swipe every 4 seconds
        _slideTimer.Elapsed += OnSlideTimerElapsed;
        _slideTimer.AutoReset = true;
        _slideTimer.Start();
    }

    private void StopSlideTimer()
    {
        if (_slideTimer != null)
        {
            _slideTimer.Stop();
            _slideTimer.Dispose();
            _slideTimer = null;
        }
    }

    private async void OnSlideTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await InvokeAsync(() =>
        {
            if (_overallBalances.Count > 1)
            {
                _currentBalanceIndex = (_currentBalanceIndex + 1) % _overallBalances.Count;
                StateHasChanged();
            }
        });
    }

    private void NextBalanceSlide()
    {
        if (_overallBalances.Count > 1)
        {
            _currentBalanceIndex = (_currentBalanceIndex + 1) % _overallBalances.Count;
            StartSlideTimer();
            StateHasChanged();
        }
    }

    private async Task OpenCreateCustomerDialogAsync()
    {
        var options = new DialogOptions { CloseButton = true, FullWidth = true, MaxWidth = MaxWidth.Medium };
        var dialog = await DialogService.ShowAsync<Customers.Components.Editor>("تعریف مشتری جدید", options);
        var result = await dialog.Result;
        if (result != null && !result.Canceled)
        {
            await LoadCustomersAsync();
            await LoadOverallCustomerBalancesAsync();
        }
    }

    public new async ValueTask DisposeAsync()
    {
        StopSlideTimer();
        await base.DisposeAsync();
    }
}

public class CustomerBalanceRpSummary
{
    public string PriceUnitTitle { get; set; } = default!;
    public decimal NetBalance { get; set; }
    public decimal TotalReceivable { get; set; }
    public decimal TotalPayable { get; set; }
}
