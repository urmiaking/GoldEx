using GoldEx.Shared.DTOs.Health;
using GoldEx.Shared.Services.Abstractions;
using Humanizer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using MudBlazor;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class HealthMonitor
{
    private HealthCheckResponse? _healthResponse;

    private IHealthService HealthService => GetRequiredService<IHealthService>();

    protected override async Task OnInitializedAsync()
    {
        await LoadHealthAsync();
        await base.OnInitializedAsync();
    }

    private async Task LoadHealthAsync()
    {
        try
        {
            SetBusy();
            CancelToken();

            var response = await HealthService.GetAsync(CancellationTokenSource.Token);

            _healthResponse = response;
        }
        catch
        {
            _healthResponse = new HealthCheckResponse { Status = HealthStatus.Unhealthy };
        }
        finally
        {
            SetIdeal();
        }
    }

    private Color GetStatusColor(HealthStatus? status) => status switch
    {
        HealthStatus.Healthy => Color.Success,
        HealthStatus.Degraded => Color.Warning,
        HealthStatus.Unhealthy => Color.Error,
        null => Color.Info,
        _ => Color.Default
    };

    private string GetStatusIcon(HealthStatus? status)
    {
        return status switch
        {
            HealthStatus.Healthy => Icons.Material.Filled.CheckCircle,
            HealthStatus.Degraded => Icons.Material.Filled.Warning,
            HealthStatus.Unhealthy => Icons.Material.Filled.Error,
            null => Icons.Material.Filled.Refresh,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
        };
    }

    private string? GetDescription(Entry entryValue)
    {
        return !string.IsNullOrEmpty(entryValue.Description) ? entryValue.Description : $"تاخیر سرویس: {entryValue.Duration.Humanize(2)}";
    }
}