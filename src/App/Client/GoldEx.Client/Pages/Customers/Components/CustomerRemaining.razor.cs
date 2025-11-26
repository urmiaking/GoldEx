using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using System.Timers;
using GoldEx.Shared.Helpers;
using MudBlazor;
using Timer = System.Timers.Timer;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class CustomerRemaining
{
    [Parameter, EditorRequired] public Guid CustomerId { get; set; }
    [Parameter] public Typo TextTypo { get; set; } = Typo.body2;
    [Parameter] public Guid? PriceUnitId { get; set; }
    [Parameter] public string? ItemClass { get; set; }
    [Parameter] public decimal? AddedValue { get; set; }

    private List<CustomerRemainingVm>? _remainingList;
    private bool _isLoading = true;
    private int _currentIndex;
    private Timer? _slideTimer;

    private Guid _previousCustomerId;
    private Guid? _previousPriceUnitId;

    private bool ShouldApplyAddedValue =>
        AddedValue.HasValue &&
        PriceUnitId.HasValue &&
        _remainingList?.Count == 1;

    protected override async Task OnParametersSetAsync()
    {
        if (CustomerId != _previousCustomerId || PriceUnitId != _previousPriceUnitId)
        {
            _previousCustomerId = CustomerId;
            _previousPriceUnitId = PriceUnitId;

            await LoadBalancesAsync();
        }
    }

    private async Task LoadBalancesAsync()
    {
         _isLoading = true; 
         StopSlideTimer(); 

        await GetCustomerBalancesAsync();

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

    private async Task GetCustomerBalancesAsync()
    {
        _isLoading = true;

        StateHasChanged();

        await SendRequestAsync<ITransactionService, List<GetCustomerRemainingResponse>>(
            action: (s, ct) => s.GetCustomerRemainingListAsync(CustomerId, PriceUnitId, ct),
            afterSend: response =>
            {
                _remainingList = response.Select(CustomerRemainingVm.CreateFrom).ToList();
            });

        _isLoading = false;
    }

    private Color GetMudColor(decimal amount)
    {
        if (ShouldApplyAddedValue) 
            amount += AddedValue!.Value;

        return amount switch
        {
            > 0 => Color.Error,
            < 0 => Color.Success,
            _ => Color.Default
        };
    }

    private string GetFormattedAmount(CustomerRemainingVm remaining)
    {
        var calculationAmount = remaining.Amount;

        if (ShouldApplyAddedValue) 
            calculationAmount += AddedValue!.Value;

        var absoluteAmount = Math.Abs(calculationAmount);
        return absoluteAmount.ToCurrencyFormat(remaining.PriceUnit);
    }

    public override ValueTask DisposeAsync()
    {
        StopSlideTimer();
        return base.DisposeAsync();
    }
}