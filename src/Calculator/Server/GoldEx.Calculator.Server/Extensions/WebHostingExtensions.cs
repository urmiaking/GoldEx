using Microsoft.EntityFrameworkCore;
using GoldEx.Calculator.Client;
using GoldEx.Calculator.Server.Common.Middlewares;
using GoldEx.Calculator.Server.Components;
using GoldEx.Server.Application;
using GoldEx.Server.Infrastructure;
using System.Reflection;

namespace GoldEx.Calculator.Server.Extensions;

public static class WebHostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        builder.Services
            .AddClientServerServices()
            .AddServer(configuration)
            .AddApplication()
            .AddInfrastructure(configuration);

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        Assembly[] additionalAssemblies = [typeof(Routes).Assembly];

        // Branch before HTTPS, tenant resolution, license and authentication: localhost must not redirect.
        app.MapWhen(
            context => context.Request.Path.Equals("/health/ready", StringComparison.OrdinalIgnoreCase),
            branch => branch.Run(async context =>
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
                timeout.CancelAfter(TimeSpan.FromSeconds(4));
                try
                {
                    var database = context.RequestServices.GetRequiredService<GoldExDbContext>();
                    if (!await database.Database.CanConnectAsync(timeout.Token))
                    {
                        context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                        return;
                    }
                    await database.Database.ExecuteSqlRawAsync("SELECT 1", timeout.Token);
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync("Ready", timeout.Token);
                }
                catch (OperationCanceledException) when (context.RequestAborted.IsCancellationRequested)
                {
                    // The caller disconnected.
                }
                catch (Exception)
                {
                    context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
                }
            }));

        if (app.Environment.IsDevelopment())
        {
            app.UseWebAssemblyDebugging();
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
        app.UseAntiforgery();
        app.UseSession();
        app.MapStaticAssets();

        app.MapControllers();
        app.MapRazorComponents<App>()
            .AddInteractiveWebAssemblyRenderMode()
            .AddInteractiveServerRenderMode()
            .AddAdditionalAssemblies(additionalAssemblies);

        return app;
    }
}