using GoldEx.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Client.Offline.Infrastructure;

public class OfflineDbContextFactory(IServiceProvider serviceProvider) : IGoldExDbContextFactory
{
    private IDbContextFactory<OfflineDbContext> Factory => serviceProvider.GetRequiredService<IDbContextFactory<OfflineDbContext>>();

    public async Task<DbContext> CreateDbContextAsync()
    {
        var dbContext = await Factory.CreateDbContextAsync();

        return dbContext;
    }
}