using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Client.Data;

public class OfflineDbContextFactory : IDesignTimeDbContextFactory<OfflineDbContext>
{
    public OfflineDbContext CreateDbContext(string[] args)
    {
        //This method should not be used in bit Besql.
        throw new Exception("CreateDbContext is not supported in bit Besql, use CreateDbContextAsync instead.");
    }

    public async Task<OfflineDbContext> CreateDbContextAsync(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OfflineDbContext>();
        // Configure your DbContext options here (connection string, etc.)
        // Example:
        optionsBuilder.UseSqlite("Data Source=Offline-ClientDb.db");

        return new OfflineDbContext(optionsBuilder.Options);
    }

    OfflineDbContext IDesignTimeDbContextFactory<OfflineDbContext>.CreateDbContext(string[] args)
    {
        return this.CreateDbContextAsync(args).GetAwaiter().GetResult();
    }
}