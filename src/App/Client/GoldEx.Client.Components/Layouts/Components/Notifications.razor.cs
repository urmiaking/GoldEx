using GoldEx.Shared.DTOs.Notifications;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;

namespace GoldEx.Client.Components.Layouts.Components;

public partial class Notifications
{
    [Parameter] public bool ShowTrigger { get; set; } = true;
    [Parameter] public EventCallback<int> UnreadCountChanged { get; set; }
    [Inject] private IJSRuntime JS { get; set; } = default!;
    private List<GetNotificationResponse> _items = [];
    private int _unreadCount;
    private bool _working;
    private bool _loading;
    private ElementReference _panel;
    private IJSObjectReference? _module, _binding;
    private readonly CancellationTokenSource _lifetime = new();
    private Task? _polling;
    private PushConfigurationResponse _pushConfiguration = new(false, null);
    private PushState _pushState = new(false, "default", false, null);
    private string? _pushFeedback;
    private string PushDescription => !_pushState.Supported ? "این مرورگر از اعلان دستگاه پشتیبانی نمی‌کند. در آیفون، برنامه را به صفحهٔ اصلی اضافه کنید و از آنجا باز کنید." :
        !_pushConfiguration.Enabled ? "ارسال اعلان دستگاه هنوز آماده نیست؛ اعلان‌های داخل برنامه در دسترس‌اند." :
        _pushState.Permission == "denied" ? "اجازهٔ اعلان بسته است. آن را از تنظیمات مرورگر برای این سایت فعال کنید." :
        _pushState.Subscribed ? "یادآوری‌ها حتی وقتی صفحهٔ برنامه بسته باشد به این دستگاه می‌رسند." : "با اجازهٔ شما، سررسیدهای گذشته را روی همین دستگاه یادآوری می‌کنیم.";

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        Navigation.LocationChanged += LocationChanged;
        await LoadNotificationsAsync();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (!firstRender) return;
        _module = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/GoldEx.Client.Components/js/notifications.js");
        _binding = await _module.InvokeAsync<IJSObjectReference>("bind", _panel);
        await SendRequestAsync<IPushNotificationService, PushConfigurationResponse>((s, ct) => s.GetConfigurationAsync(ct), afterSend: result => _pushConfiguration = result);
        _pushState = await _module.InvokeAsync<PushState>("getState");
        if (_pushState.Permission == "granted" && _pushConfiguration.Enabled)
            await EnablePushAsync(false);
        _polling = PollAsync(_lifetime.Token);
        StateHasChanged();
    }
    public async Task OpenAsync()
    {
        if (_binding is null) return;
        await _binding.InvokeVoidAsync("open");
        await LoadNotificationsAsync();
    }
    public async Task CloseAsync()
    {
        if (_binding is not null) await _binding.InvokeVoidAsync("close");
    }
    private void LocationChanged(object? sender, LocationChangedEventArgs args) => _ = InvokeAsync(CloseAsync);
    private async Task PollAsync(CancellationToken ct)
    {
        try
        {
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            while (await timer.WaitForNextTickAsync(ct)) await InvokeAsync(LoadNotificationsAsync);
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
    }
    private async Task LoadNotificationsAsync()
    {
        if (_loading || _working) return;
        _loading = true;
        try
        {
        await SendRequestAsync<INotificationService, List<GetNotificationResponse>>((s, ct) => s.GetListAsync(null, ct), afterSend: result =>
        {
            _items = result; _unreadCount = result.Count(x => !x.IsRead);
        });
        await UnreadCountChanged.InvokeAsync(_unreadCount);
        }
        finally { _loading = false; }
    }
    private Task HandleDelete(Guid id) => ChangeAsync((s, ct) => s.DeleteAsync(id, ct));
    private Task HandleMarkAsRead(Guid id) => ChangeAsync((s, ct) => s.MarkAsReadAsync(id, ct));
    private Task HandleMarkAllAsRead() => ChangeAsync((s, ct) => s.MarkAllAsReadAsync(ct));
    private async Task ChangeAsync(Func<INotificationService, CancellationToken, Task> action)
    {
        if (_working || _loading) return;
        _working = true;
        try { await SendRequestAsync(action); }
        finally { _working = false; }
        await LoadNotificationsAsync();
    }
    private async Task EnablePushAsync() => await EnablePushAsync(true);
    private async Task EnablePushAsync(bool prompt)
    {
        if (_module is null || _working || _loading) return;
        _working = true; _pushFeedback = null;
        try
        {
            var subscription = await _module.InvokeAsync<BrowserSubscription>("subscribe", _pushConfiguration.PublicKey, prompt);
            _pushState = new(subscription.Supported, subscription.Permission, false, subscription.Endpoint);
            if (prompt && subscription.Endpoint is null)
                _pushFeedback = subscription.Permission == "denied" ? "اجازهٔ اعلان بسته است؛ تنظیمات این سایت را در مرورگر بررسی کنید." :
                    "اجازهٔ اعلان دستگاه هنوز تأیید نشده است. برای تکمیل فعال‌سازی، پیام مرورگر را تأیید کنید.";
            if (subscription.Endpoint is not null && subscription.P256dh is not null && subscription.Auth is not null)
            {
                await SendRequestAsync<IPushNotificationService>((s, ct) => s.RegisterAsync(new(subscription.Endpoint, subscription.P256dh, subscription.Auth), ct),
                    afterSend: () => { _pushState = _pushState with { Subscribed = true }; return Task.CompletedTask; });
            }
        }
        catch (JSException) { _pushFeedback = "برقراری ارتباط با سرویس اعلان ممکن نشد. اتصال اینترنت و اجازهٔ اعلان مرورگر را بررسی کنید."; }
        finally { _working = false; }
    }
    private async Task DisablePushAsync()
    {
        if (_module is null || _working || _loading) return;
        _working = true;
        try
        {
            if (_pushState.Endpoint is not null)
                await SendRequestAsync<IPushNotificationService>((s, ct) => s.RevokeAsync(new(_pushState.Endpoint), ct));
            await _module.InvokeVoidAsync("disable");
            _pushState = await _module.InvokeAsync<PushState>("getState");
            _pushFeedback = "اعلان این دستگاه غیرفعال شد.";
        }
        finally { _working = false; }
    }
    private async Task TestPushAsync()
    {
        if (_working || _loading || _pushState.Endpoint is null) return;
        _working = true;
        try
        {
            await SendRequestAsync<IPushNotificationService, PushTestResponse>((s, ct) => s.SendTestAsync(new(_pushState.Endpoint), ct), afterSend: result =>
                _pushFeedback = result.Status == "Accepted" ? "سرویس اعلان، پیام آزمایشی را پذیرفت. دریافت آن را روی دستگاه بررسی کنید." :
                    result.Status == "Pending" ? "پیام در صف ارسال است؛ پس از برقراری ارتباط دوباره تلاش می‌کنیم." : "ارسال اعلان انجام نشد؛ اعلان دستگاه را دوباره فعال کنید.");
        }
        finally { _working = false; }
    }
    private async Task HandleButtonAction((string action, ButtonActionType actionType, Guid? invoiceId) item)
    {
        if (item.actionType == ButtonActionType.NavigateToUrl)
        {
            await CloseAsync(); Navigation.NavigateTo(item.action);
        }
        else if (item.actionType == ButtonActionType.SendReminderSms && item.invoiceId.HasValue)
        {
            await SendRequestAsync<IInvoiceService>((s, ct) => s.SendReminderAsync(item.invoiceId.Value, ct), afterSend: () =>
            {
                AddSuccessToast("پیامک با موفقیت ارسال شد"); return Task.CompletedTask;
            });
        }
    }
    public override async ValueTask DisposeAsync()
    {
        Navigation.LocationChanged -= LocationChanged;
        await _lifetime.CancelAsync();
        if (_polling is not null) await _polling;
        _lifetime.Dispose();
        try
        {
            if (_binding is not null) { await _binding.InvokeVoidAsync("dispose"); await _binding.DisposeAsync(); }
            if (_module is not null) await _module.DisposeAsync();
        }
        catch (JSDisconnectedException) { }
        await base.DisposeAsync();
    }
    private sealed record PushState(bool Supported, string Permission, bool Subscribed, string? Endpoint);
    private sealed record BrowserSubscription(bool Supported, string Permission, string? Endpoint, string? P256dh, string? Auth);
}
