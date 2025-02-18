using GoldEx.Sdk.Common.Extensions;
using GoldEx.Shared.DTOs;
using GoldEx.Shared.Services;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace GoldEx.Client.Pages.Home.Components;

public partial class PriceBoard
{
    [Parameter] public string Class { get; set; } = default!;

    private IPriceClientService PriceService => GetRequiredService<IPriceClientService>();

    private readonly TableGroupDefinition<GetPriceResponse> _groupDefinition = new()
    {
        GroupName = "گروه",
        Indentation = false,
        Expandable = false,
        Selector = e => e.Type.GetDisplayName()
    };

    private IEnumerable<GetPriceResponse>? _items;
    private Timer? _timer;

    protected override async Task OnInitializedAsync()
    {
        await LoadLatestPricesAsync();
        await StartTimer();
        await base.OnInitializedAsync();
    }

    private async Task LoadLatestPricesAsync()
    {
        try
        {
            SetBusy();
            CancelToken();

            _items = await PriceService.GetLatestPricesAsync(CancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            AddExceptionToast(e);
        }
        finally
        {
            SetIdeal();
        }
    }

    private Task StartTimer()
    {
        _timer = new Timer(TimerCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));

        return Task.CompletedTask;
    }

    private async void TimerCallback(object? state)
    {
        await LoadLatestPricesAsync();
        StateHasChanged();
    }

    public override void Dispose()
    {
        _timer?.Dispose();
        CancellationTokenSource.Cancel(); 
        CancellationTokenSource.Dispose();
        base.Dispose();
    }
}