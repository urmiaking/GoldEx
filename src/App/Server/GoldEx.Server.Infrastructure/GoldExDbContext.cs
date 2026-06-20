using System.Reflection;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Sdk.Server.Infrastructure;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure.Configurations;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure;

public class GoldExDbContext(
    DbContextOptions<GoldExDbContext> options,
    IStoreContext? storeContext = null)
    : GoldExDbContextBase<AppUser, AppRole, Guid, AppUserClaim, AppUserRole, AppUserLogin, AppRoleClaim, AppUserToken, AppUserPasskey>(
        options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(CustomerConfiguration).Assembly);

        base.OnModelCreating(builder);

        // Apply global query filter for IStoreFiltered entities dynamically
        var applyFilterMethod = typeof(GoldExDbContext)
            .GetMethod(nameof(ApplyQueryFilter), BindingFlags.NonPublic | BindingFlags.Instance);

        if (applyFilterMethod != null)
        {
            foreach (var entityType in builder.Model.GetEntityTypes())
            {
                if (typeof(IStoreFiltered).IsAssignableFrom(entityType.ClrType))
                {
                    var genericMethod = applyFilterMethod.MakeGenericMethod(entityType.ClrType);
                    genericMethod.Invoke(this, [builder]);
                }
            }
        }
    }

    public StoreId CurrentStoreId => new StoreId(storeContext != null ? (storeContext.StoreId ?? Guid.Empty) : Guid.Empty);

    private void ApplyQueryFilter<TEntity>(ModelBuilder builder) where TEntity : class, IStoreFiltered
    {
        builder.Entity<TEntity>().HasQueryFilter(e => e.StoreId == CurrentStoreId);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        configurationBuilder.Properties<StoreId>()
            .HaveConversion<StoreIdConverter>();
    }

    private class StoreIdConverter : Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<StoreId, Guid>
    {
        public StoreIdConverter() : base(
            id => id.Value,
            value => new StoreId(value))
        {
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyStoreIdToAddedEntities();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyStoreIdToAddedEntities();
        return base.SaveChanges();
    }

    private void ApplyStoreIdToAddedEntities()
    {
        if (storeContext?.StoreId == null)
            return;

        var activeStoreId = new StoreId(storeContext.StoreId.Value);

        var entries = ChangeTracker.Entries<IStoreFiltered>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in entries)
        {
            if (entry.Entity.StoreId == default)
            {
                entry.Property(nameof(IStoreFiltered.StoreId)).CurrentValue = activeStoreId;
            }
        }
    }
}