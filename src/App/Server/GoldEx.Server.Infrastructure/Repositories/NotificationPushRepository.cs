using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Domain.NotificationAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using GoldEx.Shared.Services.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
public sealed class NotificationPushRepository(GoldExDbContext db, IHttpContextAccessor? http = null) : INotificationPushRepository
{
    // GoldExComponentBase creates child service scopes during SSR. Resolve the request's selected store
    // rather than silently falling back to Guid.Empty in a newly-created child StoreContext.
    public StoreId CurrentStoreId => new(http?.HttpContext?.RequestServices.GetService<IStoreContext>()?.StoreId ?? db.CurrentStoreId.Value);
    public IQueryable<NotificationPushSubscription> Subscriptions => db.Set<NotificationPushSubscription>().IgnoreQueryFilters().Where(x => x.StoreId == CurrentStoreId);
    public IQueryable<NotificationDelivery> Deliveries => db.Set<NotificationDelivery>().IgnoreQueryFilters().Where(x => x.StoreId == CurrentStoreId);
    public IQueryable<NotificationUserState> UserStates => db.Set<NotificationUserState>().IgnoreQueryFilters().Where(x => x.StoreId == CurrentStoreId);
    public IQueryable<Notification> Notifications => db.Set<Notification>().IgnoreQueryFilters().Where(x => x.StoreId == CurrentStoreId);
    public IQueryable<Store> ActiveStores => db.Set<Store>().Where(x => x.IsActive);
    public Task<NotificationPushSubscription?> FindBrowserSubscriptionAsync(string hash, CancellationToken ct) =>
        db.Set<NotificationPushSubscription>().IgnoreQueryFilters().FirstOrDefaultAsync(x => x.EndpointHash == hash, ct);
    public Task<bool> CanNotifyAsync(Guid userId, StoreId storeId, CancellationToken ct) =>
        db.Set<StoreUser>().AnyAsync(x => x.UserId == userId && x.StoreId == storeId &&
            db.Set<AppUserRole>().Any(r => r.UserId == userId &&
                (r.Role.Name == BuiltinRoles.Administrators || r.Role.Name == BuiltinRoles.Owners)), ct);
    public async Task<bool> IsOverdueAsync(InvoiceId invoiceId, CancellationToken ct)
    {
        var today = DateOnly.FromDateTime(DateTime.Now);
        var invoice = await db.Set<Invoice>().IgnoreQueryFilters().AsNoTracking().Include(x => x.InvoicePayments)
            .AsSplitQuery().FirstOrDefaultAsync(x => x.StoreId == CurrentStoreId && x.Id == invoiceId && x.DueDate < today, ct);
        return invoice is not null && invoice.TotalUnpaidAmount > 0;
    }
    public void Add<T>(T entity) where T : class => db.Add(entity);
    public void Remove<T>(T entity) where T : class => db.Remove(entity);
    public async Task SaveAsync(CancellationToken ct) => await db.SaveChangesAsync(ct);
    public async Task<bool> ClaimAsync(Guid deliveryId, DateTime now, CancellationToken ct)
    {
        foreach (var entry in db.ChangeTracker.Entries<NotificationDelivery>().Where(x => x.Entity.Id == deliveryId).ToList())
            entry.State = EntityState.Detached;
        // An atomic lease allows multiple app instances and recovers interrupted sends after two minutes.
        return await Deliveries.Where(x => x.Id == deliveryId &&
            x.NextAttemptAtUtc <= now && (x.Status == "Pending" || x.Status == "Sending"))
            .ExecuteUpdateAsync(s => s.SetProperty(x => x.Status, "Sending")
                .SetProperty(x => x.Attempts, x => x.Attempts + 1)
                .SetProperty(x => x.NextAttemptAtUtc, now.AddMinutes(2)), ct) == 1;
    }
    public async Task RevokeUserAsync(Guid userId, CancellationToken ct)
    {
        // Logout revokes every device for this account; existing grants restore on the next sign-in.
        var subscriptions = await db.Set<NotificationPushSubscription>().IgnoreQueryFilters()
            .Where(x => x.UserId == userId && x.Enabled).ToListAsync(ct);
        foreach (var subscription in subscriptions) subscription.Disable();
        await db.SaveChangesAsync(ct);
    }
}
