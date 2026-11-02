using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Domain.NotificationAggregate;

public sealed class NotificationDelivery : IStoreFiltered
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public StoreId StoreId { get; private set; }
    public NotificationId? NotificationId { get; private set; }
    public Notification? Notification { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public NotificationPushSubscription Subscription { get; private set; } = null!;
    public DateTime NextAttemptAtUtc { get; private set; }
    public DateTime ExpiresAtUtc { get; private set; }
    public int Attempts { get; private set; }
    public string Status { get; private set; } = "Pending";
    public int? LastStatusCode { get; private set; }
    private NotificationDelivery() { }
    public static NotificationDelivery Create(NotificationPushSubscription subscription, NotificationId? notificationId, DateTime now) => new()
    {
        StoreId = subscription.StoreId, SubscriptionId = subscription.Id, NotificationId = notificationId,
        NextAttemptAtUtc = now, ExpiresAtUtc = now.AddDays(1)
    };
    public void Finish(string status, int? code = null) { Status = status; LastStatusCode = code; }
    public void Retry(DateTime now, int? code)
    {
        LastStatusCode = code;
        if (Attempts >= 5 || now >= ExpiresAtUtc) { Status = "Failed"; return; }
        Status = "Pending";
        NextAttemptAtUtc = now.AddMinutes(Math.Pow(2, Attempts));
    }
}
