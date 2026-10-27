using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Domain.NotificationAggregate;

public sealed class NotificationPushSubscription : IStoreFiltered
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public StoreId StoreId { get; private set; }
    public Guid UserId { get; private set; }
    public string EndpointHash { get; private set; } = "";
    public string Endpoint { get; private set; } = "";
    public string P256dh { get; private set; } = "";
    public string Auth { get; private set; } = "";
    public DateTime RefreshedAtUtc { get; private set; }
    public DateTime SubscribedAtUtc { get; private set; }
    public bool Enabled { get; private set; }

    private NotificationPushSubscription() { }
    public static NotificationPushSubscription Create(StoreId storeId, Guid userId, string hash,
        string endpoint, string p256dh, string auth, DateTime now) => new()
    {
        StoreId = storeId, UserId = userId, EndpointHash = hash, Endpoint = endpoint,
        P256dh = p256dh, Auth = auth, RefreshedAtUtc = now, SubscribedAtUtc = now, Enabled = true
    };
    public void Refresh(string p256dh, string auth, DateTime now)
    {
        if (!Enabled) SubscribedAtUtc = now;
        P256dh = p256dh; Auth = auth; RefreshedAtUtc = now; Enabled = true;
    }
    public void Disable() => Enabled = false;
}
