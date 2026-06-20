using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GoldEx.Server.Application.Services;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Common.Middlewares;

public class StoreResolutionMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, StoreContext storeContext, GoldExDbContext dbContext)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (Guid.TryParse(userIdClaim, out var userId))
            {
                Guid? resolvedStoreId = null;
                string? resolvedSlug = null;

                // 1. Try to get store from cookie
                if (context.Request.Cookies.TryGetValue("ActiveStoreId", out var cookieValue) && 
                    Guid.TryParse(cookieValue, out var cookieStoreId))
                {
                    // Verify that the user has access to this store
                    var storeUser = await dbContext.Set<StoreUser>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.UserId == userId && x.StoreId == new StoreId(cookieStoreId));

                    if (storeUser != null)
                    {
                        var store = await dbContext.Set<Store>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id == storeUser.StoreId && x.IsActive);

                        if (store != null)
                        {
                            resolvedStoreId = store.Id.Value;
                            resolvedSlug = store.Slug;
                        }
                    }
                }

                // 2. If not found in cookie or cookie was invalid, find default store
                if (resolvedStoreId == null)
                {
                    var defaultStoreUser = await dbContext.Set<StoreUser>()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(x => x.UserId == userId && x.IsDefault);

                    if (defaultStoreUser != null)
                    {
                        var store = await dbContext.Set<Store>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => x.Id == defaultStoreUser.StoreId && x.IsActive);

                        if (store != null)
                        {
                            resolvedStoreId = store.Id.Value;
                            resolvedSlug = store.Slug;
                        }
                    }
                }

                // 3. If no default store, fallback to first available active store
                if (resolvedStoreId == null)
                {
                    var userStores = await dbContext.Set<StoreUser>()
                        .AsNoTracking()
                        .Where(x => x.UserId == userId)
                        .Select(x => x.StoreId)
                        .ToListAsync();

                    if (userStores.Any())
                    {
                        var store = await dbContext.Set<Store>()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(x => userStores.Contains(x.Id) && x.IsActive);

                        if (store != null)
                        {
                            resolvedStoreId = store.Id.Value;
                            resolvedSlug = store.Slug;
                        }
                    }
                }

                // If resolved, populate the scoped store context and ensure the cookie is present
                if (resolvedStoreId.HasValue && resolvedSlug != null)
                {
                    storeContext.SetStore(resolvedStoreId.Value, resolvedSlug);

                    // Write or update the cookie if it's different or missing
                    if (cookieValue != resolvedStoreId.Value.ToString())
                    {
                        context.Response.Cookies.Append("ActiveStoreId", resolvedStoreId.Value.ToString(), new CookieOptions
                        {
                            HttpOnly = true,
                            Secure = true,
                            SameSite = SameSiteMode.Strict,
                            Expires = DateTimeOffset.UtcNow.AddDays(30)
                        });
                    }
                }
                else
                {
                    // User is authenticated but has no associated active store
                    var path = context.Request.Path.Value ?? string.Empty;
                    if (!IsWhitelisted(path))
                    {
                        if (path.StartsWith("/api", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Response.StatusCode = StatusCodes.Status403Forbidden;
                            return;
                        }
                        else
                        {
                            context.Response.Redirect("/NoStore");
                            return;
                        }
                    }
                }
            }
        }

        await next(context);
    }

    private static bool IsWhitelisted(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        // Normalizing path
        path = path.TrimEnd('/');

        if (path.Equals("/NoStore", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/api/Account/Logout", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (path.StartsWith("/_blazor", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_framework", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/_content", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/css", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("/js", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Check file extensions for static files
        var lastDot = path.LastIndexOf('.');
        if (lastDot > 0)
        {
            var ext = path.Substring(lastDot).ToLowerInvariant();
            string[] allowedExtensions = { ".css", ".js", ".png", ".jpg", ".jpeg", ".gif", ".ico", ".woff", ".woff2", ".ttf", ".svg", ".json", ".map", ".wasm" };
            if (allowedExtensions.Contains(ext))
            {
                return true;
            }
        }

        return false;
    }
}

