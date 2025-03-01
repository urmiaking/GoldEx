using GoldEx.Shared.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Server.Infrastructure;

public class GoldExDbContextFactory(IServiceProvider serviceProvider) : IGoldExDbContextFactory
{
    private IDbContextFactory<GoldExDbContext> Factory => serviceProvider.GetRequiredService<IDbContextFactory<GoldExDbContext>>();

    public async Task<DbContext> CreateDbContextAsync()
    {
        var context = await Factory.CreateDbContextAsync();

        return context;
    }
}