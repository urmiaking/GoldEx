using GoldEx.Shared.Constants;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Sdk.Server.Infrastructure;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure.Configurations;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

using GoldEx.Server.Domain.CustomerTransferVoucherAggregate;
using GoldEx.Server.Domain.ProductAttributeAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Domain.SmartTrayAggregate;

namespace GoldEx.Server.Infrastructure;

public class GoldExDbContext(
    DbContextOptions<GoldExDbContext> options,
    IStoreContext? storeContext = null,
    IHttpContextAccessor? httpContextAccessor = null)
    : GoldExDbContextBase<AppUser, AppRole, Guid, AppUserClaim, AppUserRole, AppUserLogin, AppRoleClaim, AppUserToken, AppUserPasskey>(
        options)
{
    public DbSet<CustomerTransferVoucher> CustomerTransferVouchers => Set<CustomerTransferVoucher>();
    public DbSet<ProductAttribute> ProductAttributes => Set<ProductAttribute>();
    public DbSet<CategoryAttribute> CategoryAttributes => Set<CategoryAttribute>();
    public DbSet<SmartTray> SmartTrays => Set<SmartTray>();
    public DbSet<SmartTrayItem> SmartTrayItems => Set<SmartTrayItem>();
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
        => SaveChangesAsync(true, cancellationToken);

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        EnsureWritable();
        ApplyStoreIdToAddedEntities();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    public override int SaveChanges() => SaveChanges(true);

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        EnsureWritable();
        ApplyStoreIdToAddedEntities();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    private void EnsureWritable()
    {
        if (DemoAccess.IsDemo(httpContextAccessor?.HttpContext?.User) && ChangeTracker.HasChanges())
            throw new UnauthorizedAccessException(DemoAccess.ReadOnlyMessage);
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