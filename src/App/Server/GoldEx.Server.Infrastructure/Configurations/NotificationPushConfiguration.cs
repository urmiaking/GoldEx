using GoldEx.Server.Domain.NotificationAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldEx.Server.Infrastructure.Configurations;

internal sealed class NotificationPushSubscriptionConfiguration : IEntityTypeConfiguration<NotificationPushSubscription>
{
    public void Configure(EntityTypeBuilder<NotificationPushSubscription> b)
    {
        b.ToTable("NotificationPushSubscriptions"); b.HasKey(x => x.Id);
        b.Property(x => x.EndpointHash).HasMaxLength(64);
        b.HasIndex(x => x.EndpointHash).IsUnique();
        b.HasIndex(x => new { x.StoreId, x.UserId });
        b.Property(x => x.Endpoint).HasMaxLength(2048);
        b.Property(x => x.P256dh).HasMaxLength(128);
        b.Property(x => x.Auth).HasMaxLength(64);
    }
}
internal sealed class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> b)
    {
        b.ToTable("NotificationDeliveries"); b.HasKey(x => x.Id);
        b.Property(x => x.NotificationId).HasConversion(x => x.HasValue ? x.Value.Value : (Guid?)null, x => x.HasValue ? new NotificationId(x.Value) : null);
        b.Property(x => x.Status).HasMaxLength(16);
        b.HasIndex(x => new { x.NotificationId, x.SubscriptionId }).IsUnique().HasFilter("[NotificationId] IS NOT NULL");
        b.HasIndex(x => new { x.StoreId, x.Status, x.NextAttemptAtUtc });
        b.HasOne(x => x.Notification).WithMany().HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Subscription).WithMany().HasForeignKey(x => x.SubscriptionId).OnDelete(DeleteBehavior.Cascade);
    }
}
internal sealed class NotificationUserStateConfiguration : IEntityTypeConfiguration<NotificationUserState>
{
    public void Configure(EntityTypeBuilder<NotificationUserState> b)
    {
        b.ToTable("NotificationUserStates"); b.HasKey(x => x.Id);
        b.Property(x => x.NotificationId).HasConversion(x => x.Value, x => new NotificationId(x));
        b.HasIndex(x => new { x.NotificationId, x.UserId }).IsUnique();
        b.HasOne(x => x.Notification).WithMany().HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Cascade);
    }
}
