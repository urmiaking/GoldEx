using DevExpress.AspNetCore;
using DevExpress.AspNetCore.Reporting;
using DevExpress.Security.Resources;
using GoldEx.Client;
using GoldEx.Client.Extensions;
using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Application.Models;
using GoldEx.Server.Application;
using GoldEx.Server.Application.Extensions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Common.Middlewares;
using GoldEx.Server.Components;
using GoldEx.Server.Components.Account;
using GoldEx.Server.Infrastructure;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Routings;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Ui.Web.Extensions;
using System.Reflection;
using VHDLicenseManager;

namespace GoldEx.Server.Extensions;

public static class WebHostingExtensions
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplication ConfigureServices()
        {
            var configuration = builder.Configuration;

            SetupIconDirectory(builder);

            builder.Services
                .AddClientServerServices()
                .AddServer(configuration)
                .AddApplication().AddHostedServices()
                .AddInfrastructure(configuration);

            return builder.Build();
        }
    }

    extension(WebApplication app)
    {
        public WebApplication ConfigurePipeline()
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
                app.UseForwardedHeaders();
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

            app.UseReporting(settings =>
            {
                settings.UserDesignerOptions.DataBindingMode =
                    DevExpress.XtraReports.UI.DataBindingMode.ExpressionsAdvanced;
            });
            app.UseDevExpressControls();

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

            app.Use(async (context, next) =>
            {
                if (context.Request.Path.StartsWithSegments("/serilog-ui"))
                {
                    if (!context.User.Identity?.IsAuthenticated ?? !context.User.IsInRole(BuiltinRoles.Administrators))
                    {
                        context.Response.Redirect(ClientRoutes.Accounts.AccessDenied);
                        return;
                    }
                }
                await next();
            });

            var sharedPath = Path.Combine(app.Environment.ContentRootPath, "shared");
            var localUploadsPath = Path.Combine(app.Environment.ContentRootPath, "uploads");
            var sharedBlogContentPath = Path.Combine(sharedPath, "content", "blogs");

            // Ensure directories exist
            if (!Directory.Exists(sharedPath)) Directory.CreateDirectory(sharedPath);
            if (!Directory.Exists(localUploadsPath)) Directory.CreateDirectory(localUploadsPath);
            if (!Directory.Exists(sharedBlogContentPath)) Directory.CreateDirectory(sharedBlogContentPath);

            // ---------------------------------------------------------
            // RULE 1: Blog Content (Specific Override)
            // ---------------------------------------------------------
            // When browser asks for: /uploads/content/blogs/xyz.jpg
            // Server looks in:       /shared/content/blogs/xyz.jpg
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(sharedBlogContentPath),
                RequestPath = "/uploads/content/blogs"
            });

            // ---------------------------------------------------------
            // RULE 2: General Uploads (Fallback)
            // ---------------------------------------------------------
            // When browser asks for: /uploads/icons/favicon.ico
            // Server looks in:       /uploads/icons/favicon.ico
            // (It won't find blogs here anymore, but Rule 1 handled that)
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(localUploadsPath),
                RequestPath = "/uploads"
            });

            // ---------------------------------------------------------
            // RULE 3: Shared Folder (Optional direct access)
            // ---------------------------------------------------------
            // When browser asks for: /shared/...
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(sharedPath),
                RequestPath = "/shared"
            });

            app.UseSerilogUi();

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
                AllowCachingResponses = true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.MapGet("/__test", ctx =>
            {
                return ctx.Response.WriteAsync(
                    $"Scheme = {ctx.Request.Scheme}, Host = {ctx.Request.Host}");
            });

            AppDomain.CurrentDomain.SetData("DXResourceDirectory", app.Environment.ContentRootPath);

            return app;
        }

        public WebApplication ConfigureSqlSerilog()
        {
            using var scope = app.Services.CreateScope();

            // Reconfigure Serilog with MSSqlServer sink
            var loggerFactory = scope.ServiceProvider.GetRequiredService<ILoggerFactory>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .CreateLogger();

            loggerFactory.AddSerilog(Log.Logger);

            return app;
        }

        public async Task InitializeLicenseAsync()
        {
            using var scope = app.Services.CreateScope();

            var licenseStore = scope.ServiceProvider.GetRequiredService<ILicenseStore>();
            var productLicense = scope.ServiceProvider.GetRequiredService<ProductLicense>();
            var logger = scope.ServiceProvider.GetRequiredService<ILogger<ProductLicense>>();
            var url = scope.ServiceProvider.GetRequiredService<IConfiguration>()["License:BaseUrl"];

            var appLicense = await licenseStore.GetAsync();

            if (appLicense is null) 
            {
                logger.LogWarning("No license found. registration required.");
                productLicense.UpdateLicense(LicensePlan.Unregistered, DateTime.MinValue, DateTime.MinValue);
                return;
            }

            try
            {
                using var license = new License(url);
                var response = await license.GetLicenseAsync(nameof(GoldEx), appLicense.LicenseId);

                if (response is null)
                {
                    logger.LogError("Invalid license.");
                    productLicense.UpdateLicense(LicensePlan.Unregistered, DateTime.MinValue, DateTime.MinValue);
                    return;
                }

                productLicense.UpdateLicense(
                    response.Type.GetLicensePlan(),
                    response.RegisteredAt,
                    response.Expiry
                );

                logger.LogInformation($"License applied successfully. License plan: {productLicense.Plan.ToString()}, Expire date: {productLicense.ExpireDate}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "License server unavailable.");
                productLicense.UpdateLicense(LicensePlan.Unregistered, DateTime.MinValue, DateTime.MinValue);
            }
        }
    }

    private static void SetupIconDirectory(WebApplicationBuilder builder)
    {
        var env = builder.Environment;

        var iconPath = env.GetAppIconDirectory();
        var absoluteIconPath = Path.Combine(env.WebRootPath, iconPath);

        AppDomain.CurrentDomain.SetData("DXResourceDirectory", absoluteIconPath);
        AccessSettings.StaticResources.TrySetRules(DirectoryAccessRule.Allow(absoluteIconPath));
    }
}