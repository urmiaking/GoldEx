using GoldEx.Server.Domain.NotificationAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface INotificationPushRepository
{
    StoreId CurrentStoreId { get; }
    IQueryable<NotificationPushSubscription> Subscriptions { get; }
    IQueryable<NotificationDelivery> Deliveries { get; }
    IQueryable<NotificationUserState> UserStates { get; }
    IQueryable<Notification> Notifications { get; }
    IQueryable<Store> ActiveStores { get; }
    Task<NotificationPushSubscription?> FindBrowserSubscriptionAsync(string hash, CancellationToken ct);
    Task<bool> CanNotifyAsync(Guid userId, StoreId storeId, CancellationToken ct);
    Task<bool> IsOverdueAsync(InvoiceId invoiceId, CancellationToken ct);
    void Add<T>(T entity) where T : class;
    void Remove<T>(T entity) where T : class;
    Task SaveAsync(CancellationToken ct);
    Task<bool> ClaimAsync(Guid deliveryId, DateTime now, CancellationToken ct);
    Task RevokeUserAsync(Guid userId, CancellationToken ct);
}
