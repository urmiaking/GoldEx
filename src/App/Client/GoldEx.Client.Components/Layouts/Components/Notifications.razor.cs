using GoldEx.Shared.DTOs.Notifications;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
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
            _items = result.Where(x => !x.IsRead).ToList(); _unreadCount = _items.Count;
        });
        await UnreadCountChanged.InvokeAsync(_unreadCount);
        }
        finally { _loading = false; }
    }
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
    // MeetFlow-style inbox: opening an item is the only per-notification
    // action. Never execute reminder-SMS (or delete) commands implicitly.
    private async Task HandleOpenNotificationAsync(GetNotificationResponse notification)
    {
        if (_working || _loading) return;
        var target = GetNotificationDestination(notification);
        _working = true;
        try
        {
            await SendRequestAsync<INotificationService>(
                (service, cancellationToken) => service.MarkAsReadAsync(notification.Id, cancellationToken));
            _items.RemoveAll(x => x.Id == notification.Id);
            _unreadCount = _items.Count;
            await UnreadCountChanged.InvokeAsync(_unreadCount);
        }
        finally
        {
            _working = false;
        }

        if (target is not null)
        {
            await CloseAsync();
            Navigation.NavigateTo(target);
        }
    }

    private static string? GetNotificationDestination(GetNotificationResponse notification)
    {
        // Only follow internal application routes from the server-provided
        // View action; never treat the SendReminderSms action as navigation.
        var url = notification.Buttons?
            .FirstOrDefault(button => button.ActionType == ButtonActionType.NavigateToUrl)?.Action;
        if (!string.IsNullOrWhiteSpace(url)
            && url.StartsWith("/", StringComparison.Ordinal)
            && !url.StartsWith("//", StringComparison.Ordinal)
            && !url.Contains('\\')
            && !url.Any(char.IsControl)
            && Uri.TryCreate(url, UriKind.Relative, out _))
            return url;

        return notification.InvoiceId is Guid invoiceId
            ? ClientRoutes.Invoices.SetInvoice.Replace("{id?}", invoiceId.ToString("D"), StringComparison.Ordinal)
            : null;
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
