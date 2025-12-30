using GoldEx.Sdk.Server.Infrastructure.Common;

namespace GoldEx.Sdk.Server.Infrastructure.Abstractions;

public interface IDbSeeder
{
    int Order { get; }
    Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken = default);
}