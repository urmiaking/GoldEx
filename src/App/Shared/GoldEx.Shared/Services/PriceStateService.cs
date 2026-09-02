using GoldEx.Shared.Contracts.Hubs;
using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;

namespace GoldEx.Shared.Services;

public class PriceStateService : IPriceStateService, IAsyncDisposable, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PriceStateService>? _logger;
    private readonly ConcurrentDictionary<string, object> _cache = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly SemaphoreSlim _hubLock = new(1, 1);

    private HubConnection? _hubConnection;
    private bool _isConnecting;
    private bool _isDisposed;

    private event Action? _onPricesUpdated;
    public event Action<List<PriceChangedNotificationDto>>? OnPriceBatchChanged;
    public event Action<PriceChangedNotificationDto>? OnPriceChanged;

    private Timer? _fallbackTimer;
    private readonly object _timerLock = new();

    public event Action? OnPricesUpdated
    {
        add
        {
            lock (_timerLock)
            {
                _onPricesUpdated += value;
                StartSignalRIfNeeded();
            }
        }
        remove
        {
            lock (_timerLock)
            {
                _onPricesUpdated -= value;
                StopFallbackTimerIfNoSubscribers();
            }
        }
    }

    public PriceStateService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetService<ILogger<PriceStateService>>();
    }

    public async Task<List<GetPriceResponse>> GetListAsync(bool? isPinned = null, CancellationToken cancellationToken = default)
    {
        StartSignalRIfNeeded();

        var key = $"list_{isPinned?.ToString() ?? "all"}";
        return await GetOrAddAsync(key, async (priceService, ct) =>
            await priceService.GetListAsync(isPinned, ct), cancellationToken);
    }

    public async Task<GetPriceResponse?> GetAsync(GoldUnitType unitType, Guid? priceUnitId, bool applySafetyMargin, CancellationToken cancellationToken = default)
    {
        StartSignalRIfNeeded();

        var key = $"price_{unitType}_{priceUnitId?.ToString() ?? "null"}_{applySafetyMargin}";
        return await GetOrAddAsync(key, async (priceService, ct) =>
            await priceService.GetAsync(unitType, priceUnitId, applySafetyMargin, ct), cancellationToken);
    }

    public async Task<GetExchangeRateResponse> GetExchangeRateAsync(Guid primaryPriceUnitId, Guid secondaryPriceUnitId, CancellationToken cancellationToken = default)
    {
        StartSignalRIfNeeded();

        var key = $"rate_{primaryPriceUnitId}_{secondaryPriceUnitId}";
        return await GetOrAddAsync(key, async (priceService, ct) =>
            await priceService.GetExchangeRateAsync(primaryPriceUnitId, secondaryPriceUnitId, ct), cancellationToken);
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            _cache.Clear();
        }
        finally
        {
            _lock.Release();
        }

        _onPricesUpdated?.Invoke();
    }

    private async Task<T> GetOrAddAsync<T>(string key, Func<IPriceService, CancellationToken, Task<T>> factory, CancellationToken cancellationToken)
    {
        if (_cache.TryGetValue(key, out var cachedObj) && cachedObj is CacheEntry<T> existing && !existing.IsExpired)
        {
            return await existing.Task;
        }

        await _lock.WaitAsync(cancellationToken);
        try
        {
            if (_cache.TryGetValue(key, out cachedObj) && cachedObj is CacheEntry<T> existing2 && !existing2.IsExpired)
            {
                return await existing2.Task;
            }

            var ttl = TimeSpan.FromMinutes(5);

            var task = Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var priceService = scope.ServiceProvider.GetRequiredService<IPriceService>();
                return await factory(priceService, cancellationToken);
            }, cancellationToken);

            var entry = new CacheEntry<T>
            {
                Task = task,
                ExpiryTime = DateTime.UtcNow.Add(ttl)
            };

            _cache[key] = entry;
            return await task;
        }
        finally
        {
            _lock.Release();
        }
    }

    #region SignalR Real-Time Push & Self-Healing

    private void StartSignalRIfNeeded()
    {
        if (_isDisposed) return;
        if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected) return;

        _ = Task.Run(async () =>
        {
            try
            {
                await EnsureSignalRConnectedAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogDebug(ex, "Background SignalR start encountered non-fatal exception.");
            }
        });
    }

    private async Task EnsureSignalRConnectedAsync()
    {
        if (_isDisposed) return;
        if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
            return;

        await _hubLock.WaitAsync();
        try
        {
            if (_isDisposed) return;
            if (_hubConnection is not null && (_hubConnection.State == HubConnectionState.Connected || _isConnecting))
                return;

            _isConnecting = true;

            using var scope = _serviceProvider.CreateScope();
            var client = scope.ServiceProvider.GetService<HttpClient>();
            var baseAddress = client?.BaseAddress?.ToString().TrimEnd('/');

            var hubUrl = !string.IsNullOrEmpty(baseAddress)
                ? $"{baseAddress}{ApiRoutes.Hubs.Prices}"
                : ApiRoutes.Hubs.Prices;

            if (_hubConnection == null)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(hubUrl)
                    .WithAutomaticReconnect(new[]
                    {
                        TimeSpan.Zero,
                        TimeSpan.FromSeconds(2),
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(10),
                        TimeSpan.FromSeconds(30)
                    })
                    .Build();

                _hubConnection.On<List<PriceChangedNotificationDto>>("ReceivePriceUpdates", HandlePriceUpdates);

                _hubConnection.Reconnecting += ex =>
                {
                    _logger?.LogWarning(ex, "PriceHub connection lost. Reconnecting...");
                    StartFallbackTimerIfNeeded();
                    return Task.CompletedTask;
                };

                _hubConnection.Reconnected += async connectionId =>
                {
                    _logger?.LogInformation("PriceHub reconnected: {ConnectionId}. Healing state with full sync.", connectionId);
                    StopFallbackTimer();
                    await RefreshAsync(CancellationToken.None);
                };

                _hubConnection.Closed += async ex =>
                {
                    _logger?.LogWarning(ex, "PriceHub connection closed. Starting fallback and retry loop.");
                    StartFallbackTimerIfNeeded();
                    _ = RetryConnectionLoopAsync();
                };
            }

            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
                _logger?.LogInformation("PriceHub connected successfully to {HubUrl}.", hubUrl);
                StopFallbackTimer();
            }
        }
        catch (Exception ex)
        {
            _logger?.LogDebug("Could not connect to PriceHub ({Message}). Activating fallback polling.", ex.Message);
            StartFallbackTimerIfNeeded();
        }
        finally
        {
            _isConnecting = false;
            _hubLock.Release();
        }
    }

    private async Task RetryConnectionLoopAsync()
    {
        while (!_isDisposed)
        {
            try
            {
                var delay = TimeSpan.FromSeconds(15) + TimeSpan.FromMilliseconds(Random.Shared.Next(0, 5000));
                await Task.Delay(delay);

                if (_isDisposed) return;
                if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected) return;

                await EnsureSignalRConnectedAsync();

                if (_hubConnection is not null && _hubConnection.State == HubConnectionState.Connected)
                {
                    await RefreshAsync(CancellationToken.None);
                    break;
                }
            }
            catch
            {
                // Continue retry loop
            }
        }
    }

    private void HandlePriceUpdates(List<PriceChangedNotificationDto> updates)
    {
        if (updates.Count == 0) return;

        // 1. In-place memory cache patch without issuing HTTP requests
        PatchCache(updates);

        // 2. Dispatch granular events
        OnPriceBatchChanged?.Invoke(updates);

        foreach (var update in updates)
        {
            OnPriceChanged?.Invoke(update);
        }

        // 3. Dispatch backward-compatible event for existing components
        _onPricesUpdated?.Invoke();
    }

    private void PatchCache(List<PriceChangedNotificationDto> updates)
    {
        var updateMap = updates.ToDictionary(u => u.Id);

        foreach (var entry in _cache)
        {
            if (entry.Value is CacheEntry<List<GetPriceResponse>> listEntry && listEntry.Task.IsCompletedSuccessfully)
            {
                var currentList = listEntry.Task.Result;
                var modified = false;

                for (int i = 0; i < currentList.Count; i++)
                {
                    var item = currentList[i];
                    if (updateMap.TryGetValue(item.Id, out var update))
                    {
                        currentList[i] = item with
                        {
                            Value = update.Value,
                            Unit = update.Unit,
                            Change = update.Change,
                            LastUpdate = update.LastUpdate
                        };
                        modified = true;
                    }
                }

                if (modified)
                {
                    listEntry.ExpiryTime = DateTime.UtcNow.AddMinutes(5);
                }
            }
            else if (entry.Key.StartsWith("price_") || entry.Key.StartsWith("rate_"))
            {
                // Invalidate single price calculations so they re-evaluate with updated rates
                _cache.TryRemove(entry.Key, out _);
            }
        }
    }

    #endregion

    #region Fallback Timer (Dormant when SignalR is connected)

    private void StartFallbackTimerIfNeeded()
    {
        lock (_timerLock)
        {
            if (_fallbackTimer == null && (_onPricesUpdated != null || OnPriceChanged != null || OnPriceBatchChanged != null))
            {
                var fallbackInterval = TimeSpan.FromMinutes(2);
                _fallbackTimer = new Timer(FallbackTimerCallback, null, fallbackInterval, fallbackInterval);
            }
        }
    }

    private void StopFallbackTimer()
    {
        lock (_timerLock)
        {
            _fallbackTimer?.Dispose();
            _fallbackTimer = null;
        }
    }

    private void StopFallbackTimerIfNoSubscribers()
    {
        lock (_timerLock)
        {
            if (_onPricesUpdated == null && OnPriceChanged == null && OnPriceBatchChanged == null)
            {
                _fallbackTimer?.Dispose();
                _fallbackTimer = null;
            }
        }
    }

    private async void FallbackTimerCallback(object? state)
    {
        if (_hubConnection?.State == HubConnectionState.Connected)
        {
            StopFallbackTimer();
            return;
        }

        try
        {
            await RefreshAsync(CancellationToken.None);
        }
        catch
        {
            // Silently swallow in fallback timer
        }
    }

    #endregion

    public void Dispose()
    {
        _isDisposed = true;
        StopFallbackTimer();
        _lock.Dispose();
        _hubLock.Dispose();

        if (_hubConnection != null)
        {
            _ = _hubConnection.DisposeAsync();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _isDisposed = true;
        StopFallbackTimer();
        _lock.Dispose();
        _hubLock.Dispose();

        if (_hubConnection != null)
        {
            await _hubConnection.DisposeAsync();
        }
    }

    private class CacheEntry<T>
    {
        public required Task<T> Task { get; set; }
        public DateTime ExpiryTime { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiryTime;
    }
}
