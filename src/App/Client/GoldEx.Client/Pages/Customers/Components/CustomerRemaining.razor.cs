using GoldEx.Client.Pages.Customers.ViewModels;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Transactions;
using GoldEx.Shared.Helpers;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using System.Timers;
using Timer = System.Timers.Timer;

namespace GoldEx.Client.Pages.Customers.Components;

public partial class CustomerRemaining
{
    [Parameter, EditorRequired] public Guid CustomerId { get; set; }
    [Parameter] public Typo TextTypo { get; set; } = Typo.body2;
    [Parameter] public GetPriceUnitTitleResponse? PriceUnit { get; set; }
    [Parameter] public string? ItemClass { get; set; }
    [Parameter] public decimal? AddedValue { get; set; }
    [Parameter] public bool EnableManualSlide { get; set; } = false;

    private List<CustomerRemainingVm>? _remainingList;
    private bool _isLoading = true;
    private int _currentIndex;
    private Timer? _slideTimer;

    private Guid _previousCustomerId;
    private Guid? _previousPriceUnitId;

    private bool ShouldApplyAddedValue =>
        AddedValue.HasValue &&
        PriceUnit != null &&
        _remainingList?.Count == 1;

    protected override async Task OnParametersSetAsync()
    {
        if (CustomerId != _previousCustomerId || PriceUnit?.Id != _previousPriceUnitId)
        {
            _previousCustomerId = CustomerId;
            _previousPriceUnitId = PriceUnit?.Id;

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
            action: (s, ct) => s.GetCustomerRemainingListAsync(CustomerId, PriceUnit?.Id, ct),
            afterSend: response =>
            {
                var list = response.Select(CustomerRemainingVm.CreateFrom).ToList();

                if (!list.Any() && PriceUnit != null)
                {
                    list.Add(new CustomerRemainingVm
                    {
                        PriceUnit = PriceUnit.Title,
                        Amount = 0
                    });
                }

                _remainingList = list;

                if (_remainingList.Count > 1) 
                    StartSlideTimer();
                else
                    _currentIndex = 0;
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

    private string? GetClickableStyle()
    {
        if (!EnableManualSlide || _remainingList?.Count <= 1)
            return null;

        return "cursor: pointer;";
    }

    private void OnManualSlide()
    {
        if (!EnableManualSlide)
            return;

        if (_remainingList is not { Count: > 1 })
            return;

        _currentIndex = (_currentIndex + 1) % _remainingList.Count;

        StartSlideTimer(); // or StopSlideTimer(), depending on desired UX

        StateHasChanged();
    }

    public override ValueTask DisposeAsync()
    {
        StopSlideTimer();
        return base.DisposeAsync();
    }
}