using GoldEx.Client.Helpers;
using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using System.Timers;
using MudBlazor;
using Timer = System.Timers.Timer;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class CustomerRemaining
{
    [Parameter, EditorRequired] public Guid CustomerId { get; set; }

    private List<CustomerRemainingVm>? _remainingList;
    private bool _isLoading = true;
    private int _currentIndex = 0;
    private Timer? _slideTimer;

    protected override async Task OnInitializedAsync()
    {
        await LoadBalancesAsync();
    }

    private async Task LoadBalancesAsync()
    {
        await GetCustomerBalancesAsync(CustomerId);

        if (_remainingList?.Any() == true && _remainingList.Count > 1)
        {
            StartSlideTimer();
        }
    }

    private void StartSlideTimer()
    {
        StopSlideTimer();

        _slideTimer = new Timer(5000); // 5 seconds
        _slideTimer.Elapsed += OnSlideTimerElapsed;
        _slideTimer.AutoReset = true;
        _slideTimer.Start();
    }

    private void StopSlideTimer()
    {
        _slideTimer?.Stop();
        _slideTimer?.Dispose();
        _slideTimer = null;
    }

    private async void OnSlideTimerElapsed(object? sender, ElapsedEventArgs e)
    {
        await InvokeAsync(() =>
        {
            if (_remainingList?.Any() == true)
            {
                _currentIndex = (_currentIndex + 1) % _remainingList.Count;
                StateHasChanged();
            }
        });
    }

    private async Task GetCustomerBalancesAsync(Guid customerId)
    {
        _isLoading = true;

        await SendRequestAsync<ITransactionService, List<GetCustomerRemainingResponse>>(
            action: (s, ct) => s.GetCustomerRemainingListAsync(CustomerId, ct),
            afterSend: response =>
            {
                _remainingList = response.Select(CustomerRemainingVm.CreateFrom).ToList();
            });

        _isLoading = false;
    }

    private static Color GetMudColor(decimal amount)
    {
        return amount switch
        {
            > 0 => Color.Error,
            < 0 => Color.Success,
            _ => Color.Default
        };
    }

    private static string GetFormattedAmount(CustomerRemainingVm remaining)
    {
        var absoluteAmount = Math.Abs(remaining.Amount);
        return absoluteAmount.ToCurrencyFormat(remaining.PriceUnit);
    }

    public override ValueTask DisposeAsync()
    {
        StopSlideTimer();
        return base.DisposeAsync();
    }
}