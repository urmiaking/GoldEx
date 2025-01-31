namespace GoldEx.Server.Extensions;

internal static class WebApplicationExtensions
{
    internal static async Task InitializeAsync(this WebApplication app, IServiceProvider serviceProvider)
    {
        await serviceProvider.MigrateAsync();
        await serviceProvider.SeedAsync();
    }
}