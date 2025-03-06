using GoldEx.Shared.Infrastructure;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Client.Extensions;

public static class WebAssemblyHostExtensions
{
    public static async Task InitializeDbAsync(this WebAssemblyHost app)
    {
        var dbContextFactory = app.Services.GetRequiredService<IGoldExDbContextFactory>();

        await using var context = await dbContextFactory.CreateDbContextAsync();

        await context.Database.MigrateAsync();
    }
}