using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Abstractions;
using GoldEx.Sdk.Server.Application.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.NotificationAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Notifications;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal sealed class NotificationPushService(INotificationPushRepository repository, IWebPushSender sender,
    IUserContext user, ILogger<NotificationPushService>? logger = null) : IPushNotificationService, INotificationPushProcessor
{
    private Guid UserId => user.GetUserId() ?? throw new UnauthorizedAccessException();
    private StoreId StoreId => repository.CurrentStoreId;
    public Task<PushConfigurationResponse> GetConfigurationAsync(CancellationToken ct = default) =>
        Task.FromResult(new PushConfigurationResponse(sender.IsConfigured, sender.IsConfigured ? sender.PublicKey : null));

    public async Task RegisterAsync(RegisterPushRequest request, CancellationToken ct = default)
    {
        if (!sender.IsConfigured) throw new InvalidOperationException("ارسال اعلان دستگاه آماده نیست.");
        var errors = PushSubscriptionValidator.GetErrors(request);
        if (errors.Count > 0)
        {
            logger?.LogWarning("Push registration rejected: {Codes}; provider {Provider}.",
                string.Join(',', errors.Select(x => x.ErrorCode)),
                Uri.TryCreate(request.Endpoint, UriKind.Absolute, out var endpoint) ? endpoint.Host : "invalid");
            throw new FluentValidation.ValidationException(errors);
        }
        if (!await repository.CanNotifyAsync(UserId, StoreId, ct)) throw new ForbiddenException("دسترسی به اعلان‌های این فروشگاه ندارید.");
        var hash = PushSubscriptionValidator.Hash(request.Endpoint);
        // Endpoints are browser-wide. Reassign by replacing the record, canceling the old binding's queue.
        var existing = await repository.FindBrowserSubscriptionAsync(hash, ct);
        if (existing is not null && (existing.UserId != UserId || existing.StoreId != StoreId))
        {
            repository.Remove(existing);
            await repository.SaveAsync(ct);
            existing = null;
        }
        if (existing is null)
            repository.Add(NotificationPushSubscription.Create(StoreId, UserId, hash, request.Endpoint, request.P256dh, request.Auth, DateTime.UtcNow));
        else existing.Refresh(request.P256dh, request.Auth, DateTime.UtcNow);
        await repository.SaveAsync(ct);
    }

    public async Task RevokeAsync(RevokePushRequest request, CancellationToken ct = default)
    {
        var hash = PushSubscriptionValidator.Hash(request.Endpoint);
        var subscription = await repository.Subscriptions.FirstOrDefaultAsync(x => x.EndpointHash == hash && x.UserId == UserId, ct);
        if (subscription is not null) { subscription.Disable(); await repository.SaveAsync(ct); }
    }

    public async Task<PushTestResponse> SendTestAsync(RevokePushRequest request, CancellationToken ct = default)
    {
        var hash = PushSubscriptionValidator.Hash(request.Endpoint);
        var subscription = await repository.Subscriptions.FirstOrDefaultAsync(x => x.EndpointHash == hash && x.UserId == UserId && x.Enabled, ct)
            ?? throw new NotFoundException();
        var recent = DateTime.UtcNow.AddMinutes(-1);
        var previous = await repository.Deliveries.Where(x => x.SubscriptionId == subscription.Id && x.NotificationId == null && x.ExpiresAtUtc > recent.AddDays(1))
            .OrderByDescending(x => x.ExpiresAtUtc).FirstOrDefaultAsync(ct);
        if (previous is not null) return new(previous.Id, previous.Status, previous.LastStatusCode);
        var delivery = NotificationDelivery.Create(subscription, null, DateTime.UtcNow);
        repository.Add(delivery);
        await repository.SaveAsync(ct);
        await DispatchAsync(delivery.Id, ct);
        var result = await repository.Deliveries.AsNoTracking().FirstAsync(x => x.Id == delivery.Id, ct);
        return new(result.Id, result.Status, result.LastStatusCode);
    }

    public async Task ProcessAsync(CancellationToken ct)
    {
        if (!sender.IsConfigured) return;
        var now = DateTime.UtcNow;
        var subscriptions = await repository.Subscriptions.Where(x => x.Enabled && x.RefreshedAtUtc > now.AddDays(-90)).ToListAsync(ct);
        var since = DateTime.Now.AddDays(-1);
        var notifications = await repository.Notifications.Where(x => !x.IsRead && x.CreatedAt > since).ToListAsync(ct);
        foreach (var subscription in subscriptions)
        {
            if (!await repository.CanNotifyAsync(subscription.UserId, subscription.StoreId, ct)) { subscription.Disable(); continue; }
            foreach (var notification in notifications.Where(x => x.StoreId == subscription.StoreId && x.CreatedAt.ToUniversalTime() >= subscription.SubscribedAtUtc))
            {
                if (await repository.UserStates.AnyAsync(x => x.NotificationId == notification.Id && x.UserId == subscription.UserId && x.IsRead, ct)) continue;
                if (!await repository.Deliveries.AnyAsync(x => x.NotificationId == notification.Id && x.SubscriptionId == subscription.Id, ct))
                    repository.Add(NotificationDelivery.Create(subscription, notification.Id, now));
            }
        }
        await repository.SaveAsync(ct);
        var ids = await repository.Deliveries.AsNoTracking().Where(x =>
            (x.Status == "Pending" || x.Status == "Sending") && x.NextAttemptAtUtc <= now)
            .OrderBy(x => x.NextAttemptAtUtc).Select(x => x.Id).Take(50).ToListAsync(ct);
        foreach (var id in ids) await DispatchAsync(id, ct);
    }

    private async Task DispatchAsync(Guid id, CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        if (!await repository.ClaimAsync(id, now, ct)) return;
        // Claim uses a bulk update; reload without tracking to see its current lease/attempt count.
        var delivery = await repository.Deliveries.AsNoTracking().Include(x => x.Subscription).Include(x => x.Notification)
            .FirstOrDefaultAsync(x => x.Id == id, ct);
        if (delivery is null) return;
        if (delivery.Attempts > 5) { await SaveOutcomeAsync(delivery, "Failed", null, ct); return; }
        var subscription = delivery.Subscription;
        if (now >= delivery.ExpiresAtUtc || !subscription.Enabled || subscription.RefreshedAtUtc < now.AddDays(-90) ||
            delivery.StoreId != subscription.StoreId || !await repository.CanNotifyAsync(subscription.UserId, subscription.StoreId, ct) ||
            (delivery.Notification?.InvoiceId is { } invoiceId && !await repository.IsOverdueAsync(invoiceId, ct)) ||
            delivery.Notification is { IsRead: true } ||
            (delivery.NotificationId.HasValue && await repository.UserStates.AnyAsync(x => x.NotificationId == delivery.NotificationId && x.UserId == subscription.UserId && x.IsRead, ct)))
        {
            await SaveOutcomeAsync(delivery, "Canceled", null, ct); return;
        }
        var message = delivery.Notification is { } notification
            ? new WebPushMessage(notification.Title, "یک فاکتور سررسیدگذشته دارید. برای بررسی، برنامه را باز کنید.",
                notification.Buttons?.FirstOrDefault(x => x.ActionType == GoldEx.Shared.Enums.ButtonActionType.NavigateToUrl)?.Action ?? "/", $"goldex-{notification.Id.Value}")
            : new WebPushMessage("GoldEx", "اعلان‌های این دستگاه فعال است. این یک اعلان آزمایشی است.", "/", "goldex-push-test");
        var result = await sender.SendAsync(subscription.Endpoint, subscription.P256dh, subscription.Auth, message, ct);
        await SaveOutcomeAsync(delivery, result.Outcome, result.StatusCode, ct);
    }

    private async Task SaveOutcomeAsync(NotificationDelivery detached, string outcome, int? status, CancellationToken ct)
    {
        var delivery = await repository.Deliveries.FirstAsync(x => x.Id == detached.Id, ct);
        // Refresh the tracked claim values before applying retry rules.
        if (outcome is "Retry" or "NotConfigured") delivery.Retry(DateTime.UtcNow, status);
        else delivery.Finish(outcome, status);
        if (outcome == "Gone")
        {
            var subscription = await repository.Subscriptions.FirstOrDefaultAsync(x => x.Id == detached.SubscriptionId, ct);
            subscription?.Disable();
        }
        await repository.SaveAsync(ct);
    }
}
