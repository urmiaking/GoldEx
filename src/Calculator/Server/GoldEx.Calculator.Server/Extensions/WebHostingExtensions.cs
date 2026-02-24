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