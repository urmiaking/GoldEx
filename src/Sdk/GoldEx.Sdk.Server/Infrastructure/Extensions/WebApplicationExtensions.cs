using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Sdk.Server.Infrastructure.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication ApplyDatabaseMigrations<TContext>(
        this WebApplication app)
        where TContext : DbContext
    {
        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();

        var pendingMigrations = context.Database.GetPendingMigrations().ToList();

        if (pendingMigrations.Count > 0)
        {
            context.Database.Migrate();
        }

        return app;
    }

    public static async Task SeedDatabaseAsync(this WebApplication app, CancellationToken cancellationToken = default)
    {
        var context = new DbSeedContext();

        using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

        foreach (var service in scope.ServiceProvider.GetServices<IDbSeeder>().OrderBy(x => x.Order))
            await service.SeedAsync(context, cancellationToken);
    }
}