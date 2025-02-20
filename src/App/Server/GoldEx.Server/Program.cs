using GoldEx.Server.Common.Middlewares;
using GoldEx.Server.Components.Account;
using GoldEx.Server.Components;
using GoldEx.Server.Extensions;
using GoldEx.Client;
using System.Reflection;
using GoldEx.Client.Components.Layouts;
using GoldEx.Client.Extensions;
using GoldEx.Server;
using GoldEx.Server.Application;
using GoldEx.Server.Infrastructure;
using GoldEx.Shared.Routings;
using HealthChecks.UI.Client;
using HealthChecks.UI.Configuration;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var logger = GetStartupLogger();

var builder = WebApplication.CreateBuilder(args);
var setupServices = SetupServices();

var app = builder.Build();

if (setupServices)
{
    using var scope = app.Services.CreateScope();
    var serviceProvider = scope.ServiceProvider;

    await app.InitializeAsync(serviceProvider);

    SetupPipeline();
}
else
{
    Shutdown();
}

app.Run();

return;

bool SetupServices()
{
    try
    {
        var configuration = builder.Configuration;

        builder.Services
            .AddClient()
            .AddServer(configuration)
            .AddApplication()
            .AddInfrastructure();

        return true;
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Startup failure.");
        return false;
    }
}

void SetupPipeline()
{
    Assembly[] additionalAssemblies = [typeof(Routes).Assembly];

    if (app.Environment.IsDevelopment())
    {
        app.UseWebAssemblyDebugging();
        app.UseMigrationsEndPoint();
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        app.UseHsts();
    }

    app.Use((context, next) =>
    {
        context.Response.Headers["Cross-Origin-Embedder-Policy"] = "require-corp";
        context.Response.Headers["Cross-Origin-Opener-Policy"] = "same-origin";
        return next.Invoke();
    });

    app.UseMiddleware<ExceptionHandlerMiddleware>();

    app.UseHttpsRedirection();

    var cacheMaxAgeOneWeek = (60 * 60 * 24 * 7).ToString();
    app.UseStaticFiles(new StaticFileOptions
    {
        OnPrepareResponse = ctx =>
        {
            ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cacheMaxAgeOneWeek}");
        }
    });

    app.UseRouting();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseAntiforgery();

    app.UseSession();

    app.MapStaticAssets();

    app.MapControllers();
    app.MapRazorComponents<App>()
        .AddInteractiveWebAssemblyRenderMode()
        .AddInteractiveServerRenderMode()
        .AddAdditionalAssemblies(additionalAssemblies);

    app.MapAdditionalIdentityEndpoints();

    //HealthCheck Middleware
    app.MapHealthChecks(ApiRoutes.Health.Base, new HealthCheckOptions
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }).RequireAuthorization();
    app.UseHealthChecksUI(delegate (Options options)
    {
        options.UIPath = ClientRoutes.Health.Base;
        options.AsideMenuOpened = false;
        options.PageTitle = "GoldEx Health Monitor";
    });
}

ILogger GetStartupLogger()
{
    using var loggerFactory = LoggerFactory
        .Create(loggingBuilder => loggingBuilder.SetMinimumLevel(LogLevel.Trace)
            .AddConsole());

    return loggerFactory.CreateLogger<Program>();
}

void Shutdown()
{
    logger.LogWarning("Shutting down in 5 seconds.");

    _ = new Timer(_ =>
    {
        using var scope = app.Services.CreateScope();

        var serviceProvider = scope.ServiceProvider;
        var lifetimeService = serviceProvider.GetRequiredService<IHostApplicationLifetime>();

        lifetimeService.StopApplication();
    }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
}