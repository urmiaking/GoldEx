using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Domain.NotificationAggregate;

public sealed class NotificationUserState : IStoreFiltered
{
    public Guid Id { get; private set; } = Guid.CreateVersion7();
    public StoreId StoreId { get; private set; }
    public NotificationId NotificationId { get; private set; }
    public Notification Notification { get; private set; } = null!;
    public Guid UserId { get; private set; }
    public bool IsRead { get; private set; }
    public bool Dismissed { get; private set; }
    private NotificationUserState() { }
    public static NotificationUserState Create(Notification notification, Guid userId) => new()
    {
        StoreId = notification.StoreId, NotificationId = notification.Id, UserId = userId
    };
    public void MarkRead() => IsRead = true;
    public void Dismiss() { IsRead = true; Dismissed = true; }
}
