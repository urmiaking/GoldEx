using GoldEx.Sdk.Common;
using GoldEx.Server.Application.Services;
using GoldEx.Server.Common.Authorization;
using GoldEx.Server.Domain.BlogPostAggregate;
using GoldEx.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;
using GoldEx.Shared.Constants;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace GoldEx.Server.Common.Middlewares;

/// <summary>Public demo traffic is fail-closed: no privileged endpoints, circuits, or store switching.</summary>
public sealed class DemoAccessMiddleware(RequestDelegate next)
{
    internal const string PinnedStoreKey = "GoldEx.DemoStorePinned";
    private static readonly string[] PagePrefixes =
    [
        "/dashboard", "/calculator", "/price-board", "/customers", "/invoices",
        "/inventory-stocks", "/finances", "/reporting", "/products", "/about-goldex", "/smart-tray", "/transactions"
    ];

    public async Task InvokeAsync(HttpContext context, DemoAccessService demo, StoreContext storeContext)
    {
        var host = context.Request.Host.Host;
        var path = context.Request.Path;
        var get = HttpMethods.IsGet(context.Request.Method);

        // Auth lifecycle is not a business mutation. Clear stale demo cookies before serving a fresh login form.
        if (demo.IsHost(host) && DemoAccess.IsDemo(context.User) && IsDemoSessionExit(context))
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            await context.SignOutAsync(IdentityConstants.ExternalScheme);
            context.Response.Headers.CacheControl = "no-store";
            context.Response.StatusCode = StatusCodes.Status303SeeOther;
            context.Response.Headers.Location = "/Account/Login";
            return;
        }

        if (!DemoAccess.IsDemo(context.User))
        {
            if (demo.Enabled && demo.IsHost(host))
            {
                if (context.User.Identity?.IsAuthenticated == true)
                {
                    if (!context.User.IsInRole(BuiltinRoles.Administrators))
                    {
                        await DenyAsync(context);
                        return;
                    }
                }
                else
                {
                    // No anonymous API on the demo host may accidentally resolve the production default store.
                    if (get && path == "/")
                    {
                        context.Response.Redirect("/Account/Demo");
                        return;
                    }
                    if (get && path == "/api/Account/Logout")
                    {
                        context.Response.Headers.CacheControl = "no-store";
                        context.Response.Redirect("/Account/Login");
                        return;
                    }
                    if (!IsPublicPath(path, get))
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }
                    var store = await demo.GetStoreAsync(context.RequestAborted);
                    if (store is not null)
                    {
                        storeContext.SetStore(store.Id.Value, store.Slug);
                        context.Items[PinnedStoreKey] = true;
                    }
                }
            }
            await next(context);
            return;
        }

        var session = await demo.ValidatePrincipalAsync(context.User, host, context.RequestAborted);
        if (session is null)
        {
            await context.SignOutAsync(IdentityConstants.ApplicationScheme);
            context.Response.Headers.CacheControl = "no-store";
            if (path.StartsWithSegments("/api") || !get)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await context.Response.WriteAsJsonAsync(new { code = "DEMO_EXPIRED", message = DemoAccess.ExpiredMessage });
            }
            else
                context.Response.Redirect("/Account/Login?demoExpired=1");
            return;
        }

        // Never honor a cookie/default-store fallback for demo users.
        storeContext.SetStore(session.StoreId, session.Slug);
        context.Items[PinnedStoreKey] = true;
        context.Response.Headers.CacheControl = "no-store";

        if (!IsPriceTransport(context) && !IsAllowedRead(context, session.StoreId)
            && !await IsPublishedBlogAssetAsync(context))
        {
            await DenyAsync(context);
            return;
        }

        await next(context);
    }

    // Only the public market-price hub, which exposes no client-callable business methods.
    // SmartTrayHub broadcasts tenant data and must remain blocked.
    public static bool IsPriceTransport(HttpContext context)
    {
        var path = context.Request.Path;
        var method = context.Request.Method;
        return (path == ApiRoutes.Hubs.Prices + "/negotiate" && HttpMethods.IsPost(method))
            || (path == ApiRoutes.Hubs.Prices
                && (HttpMethods.IsGet(method) || HttpMethods.IsPost(method) || HttpMethods.IsDelete(method)));
    }

    public static bool IsDemoSessionExit(HttpContext context)
    {
        var path = context.Request.Path;
        return (path == "/api/Account/Logout" && HttpMethods.IsGet(context.Request.Method))
            || (path == "/Account/Login" && (HttpMethods.IsGet(context.Request.Method) || HttpMethods.IsPost(context.Request.Method)));
    }

    internal static bool IsAllowedRead(HttpContext context, Guid storeId)
    {
        if (!HttpMethods.IsGet(context.Request.Method))
            return false;
        var path = context.Request.Path;
        var endpoint = context.GetEndpoint();
        if (endpoint?.Metadata.GetMetadata<ControllerActionDescriptor>() is not null)
            return endpoint.Metadata.GetMetadata<DemoReadAttribute>() is not null;

        if (path == "/api/Account/auth-state" || path == "/api/Account/Logout")
            return true;
        if (IsAssetPath(path) || path.StartsWithSegments($"/uploads/stores/{storeId}"))
            return true;
        if (path == "/" || path == "/Account/Login")
            return true;
        // Block server-rendered/report-sharing and designer routes until separately audited.
        if (path.StartsWithSegments("/invoices/shared") || path.StartsWithSegments("/invoices/print")
            || path.StartsWithSegments("/reporting/designer"))
            return false;
        return PagePrefixes.Any(p => path.StartsWithSegments(p));
    }

    private static async Task<bool> IsPublishedBlogAssetAsync(HttpContext context)
    {
        if (!HttpMethods.IsGet(context.Request.Method)
            || !context.Request.Path.StartsWithSegments("/uploads/content/blogs", out var remainder))
            return false;
        var segments = (remainder.Value ?? "").Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (segments.Length < 2 || !Guid.TryParse(segments[0], out var id))
            return false;
        var db = context.RequestServices.GetRequiredService<GoldExDbContext>();
        return await db.Set<BlogPost>().AsNoTracking().AnyAsync(x => x.Id == new BlogPostId(id) && x.IsActive, context.RequestAborted);
    }

    private static bool IsPublicPath(PathString path, bool get)
        => path == "/Account/Demo" || path == "/Account/Login"
            || (get && (path == "/Account/Lockout" || path == "/api/Account/auth-state" || IsAssetPath(path)));

    private static bool IsAssetPath(PathString path)
        => path.StartsWithSegments("/_framework") || path.StartsWithSegments("/_content")
            || path.StartsWithSegments("/assets") || path.StartsWithSegments("/css")
            || path.StartsWithSegments("/js") || path.StartsWithSegments("/fonts")
            || path == "/app.js" || path == "/favicon.ico" || path == "/apple-touch-icon-180x180.png"
            || path == "/manifest.webmanifest"
            || path == "/service-worker.js" || path == "/service-worker-assets.js" || path == "/notification-worker.js"
            || (path.Value is { } manifest && manifest.StartsWith("/manifest.", StringComparison.OrdinalIgnoreCase) && manifest.EndsWith(".webmanifest", StringComparison.OrdinalIgnoreCase) && !manifest[1..].Contains('/'))
            || (path.Value is { } value
                && ((value.StartsWith("/GoldEx.Server.", StringComparison.OrdinalIgnoreCase) && value.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
                    || (value.StartsWith("/app.", StringComparison.OrdinalIgnoreCase) && value.EndsWith(".js", StringComparison.OrdinalIgnoreCase))));

    private static async Task DenyAsync(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        context.Response.Headers.CacheControl = "no-store";
        await context.Response.WriteAsJsonAsync(new { code = "DEMO_READ_ONLY", message = DemoAccess.ReadOnlyMessage });
    }
}