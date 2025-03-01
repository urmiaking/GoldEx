using GoldEx.Client.Offline.Infrastructure;
using GoldEx.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Client.Offline;

public class OfflineDbContextFactory(IServiceProvider serviceProvider) : IGoldExDbContextFactory
{
    private IDbContextFactory<OfflineDbContext> Factory => serviceProvider.GetRequiredService<IDbContextFactory<OfflineDbContext>>();

    public async Task<DbContext> CreateDbContextAsync()
    {
        var dbContext = await Factory.CreateDbContextAsync();

        // dbContext.Set<T>() as IQueryable

        return dbContext;
    }
}