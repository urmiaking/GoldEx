using GoldEx.Shared.DTOs.Prices;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace GoldEx.Shared.Services;

public class PriceStateService(IServiceProvider serviceProvider) : IPriceStateService, IDisposable
{
    private readonly ConcurrentDictionary<string, object> _cache = new();
    private readonly SemaphoreSlim _lock = new(1, 1);
    
    private TimeSpan? _cachedTtl;
    private DateTime _ttlExpiry = DateTime.MinValue;
    
    private event Action? _onPricesUpdated;
    private Timer? _timer;
    private readonly object _timerLock = new();

    public event Action? OnPricesUpdated
    {
        add
        {
            lock (_timerLock)
            {
                _onPricesUpdated += value;
                StartTimerIfNeeded();
            }
        }
        remove
        {
            lock (_timerLock)
            {
                _onPricesUpdated -= value;
                StopTimerIfNoSubscribers();
            }
        }
    }

    public async Task<List<GetPriceResponse>> GetListAsync(bool? isPinned = null, CancellationToken cancellationToken = default)
    {
        var key = $"list_{isPinned?.ToString() ?? "all"}";
        return await GetOrAddAsync(key, async (priceService, ct) => 
            await priceService.GetListAsync(isPinned, ct), cancellationToken);
    }

    public async Task<GetPriceResponse?> GetAsync(GoldUnitType unitType, Guid? priceUnitId, bool applySafetyMargin, CancellationToken cancellationToken = default)
    {
        var key = $"price_{unitType}_{priceUnitId?.ToString() ?? "null"}_{applySafetyMargin}";
        return await GetOrAddAsync(key, async (priceService, ct) => 
            await priceService.GetAsync(unitType, priceUnitId, applySafetyMargin, ct), cancellationToken);
    }

    public async Task<GetExchangeRateResponse> GetExchangeRateAsync(Guid primaryPriceUnitId, Guid secondaryPriceUnitId, CancellationToken cancellationToken = default)
    {
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

            var ttl = await GetTtlAsync(cancellationToken);

            var task = Task.Run(async () =>
            {
                using var scope = serviceProvider.CreateScope();
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

    private async Task<TimeSpan> GetTtlAsync(CancellationToken cancellationToken)
    {
        if (_cachedTtl.HasValue && DateTime.UtcNow < _ttlExpiry)
        {
            return _cachedTtl.Value;
        }

        try
        {
            using var scope = serviceProvider.CreateScope();
            var settingService = scope.ServiceProvider.GetRequiredService<ISettingService>();
            var setting = await settingService.GetAsync(cancellationToken);
            if (setting?.PriceUpdateInterval > TimeSpan.Zero)
            {
                var seconds = Math.Clamp(setting.PriceUpdateInterval.TotalSeconds, 5, 300);
                _cachedTtl = TimeSpan.FromSeconds(seconds);
                _ttlExpiry = DateTime.UtcNow.AddMinutes(2);
                return _cachedTtl.Value;
            }
        }
        catch
        {
            // Fail silent
        }

        _cachedTtl = TimeSpan.FromSeconds(30);
        _ttlExpiry = DateTime.UtcNow.AddSeconds(30);
        return _cachedTtl.Value;
    }

    private void StartTimerIfNeeded()
    {
        if (_timer == null)
        {
            var interval = _cachedTtl ?? TimeSpan.FromSeconds(30);
            _timer = new Timer(TimerCallback, null, interval, interval);

            _ = Task.Run(async () =>
            {
                var actualInterval = await GetTtlAsync(CancellationToken.None);
                lock (_timerLock)
                {
                    _timer?.Change(actualInterval, actualInterval);
                }
            });
        }
    }

    private void StopTimerIfNoSubscribers()
    {
        if (_onPricesUpdated == null)
        {
            _timer?.Dispose();
            _timer = null;
        }
    }

    private async void TimerCallback(object? state)
    {
        await RefreshAsync(CancellationToken.None);
    }

    public void Dispose()
    {
        _timer?.Dispose();
        _lock.Dispose();
    }

    private class CacheEntry<T>
    {
        public required Task<T> Task { get; set; }
        public DateTime ExpiryTime { get; set; }
        public bool IsExpired => DateTime.UtcNow > ExpiryTime;
    }
}
