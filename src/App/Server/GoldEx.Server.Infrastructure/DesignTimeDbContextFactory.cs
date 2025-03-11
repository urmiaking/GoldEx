using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace GoldEx.Server.Infrastructure;

internal class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<GoldExDbContext>
{
    public GoldExDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<GoldExDbContext>();
        builder.UseSqlServer();
        var options = builder.Options;
        
        return new GoldExDbContext(options, null!);
    }
}
