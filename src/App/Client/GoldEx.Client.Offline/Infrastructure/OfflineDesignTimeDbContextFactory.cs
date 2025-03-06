using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GoldEx.Client.Offline.Infrastructure;

public class OfflineDesignTimeDbContextFactory : IDesignTimeDbContextFactory<OfflineDbContext>
{
    public OfflineDbContext CreateDbContext(string[] args)
    {
        throw new Exception("CreateDbContext is not supported in bit Besql, use CreateDbContextAsync instead.");
    }

    public async Task<OfflineDbContext> CreateDbContextAsync(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OfflineDbContext>();
        optionsBuilder.UseSqlite("Data Source=GoldExDb.db");

        return new OfflineDbContext(optionsBuilder.Options);
    }

    OfflineDbContext IDesignTimeDbContextFactory<OfflineDbContext>.CreateDbContext(string[] args)
    {
        return CreateDbContextAsync(args).GetAwaiter().GetResult();
    }
}