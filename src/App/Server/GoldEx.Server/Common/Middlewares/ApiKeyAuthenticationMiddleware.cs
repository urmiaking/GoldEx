using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Domain.OAuthAggregate;
using GoldEx.Server.Domain.PersonalAccessTokenAggregate;
using GoldEx.Server.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GoldEx.Server.Common.Middlewares;

public class ApiKeyAuthenticationMiddleware(RequestDelegate next)
{
    private const string PatTokenPrefix = "gex_pat_";
    private const string OAuthTokenPrefix = "gex_at_";

    public async Task InvokeAsync(HttpContext context, GoldExDbContext dbContext)
    {
        string? rawToken = null;

        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var headerStr = authHeader.ToString();
            if (headerStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                rawToken = headerStr["Bearer ".Length..].Trim();
            }
        }

        if (string.IsNullOrEmpty(rawToken) && context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            rawToken = apiKeyHeader.ToString().Trim();
        }

        if (!string.IsNullOrEmpty(rawToken))
        {
            var bytes = Encoding.UTF8.GetBytes(rawToken);
            var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            // 1. Try Personal Access Token
            if (rawToken.StartsWith(PatTokenPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var pat = await dbContext.Set<PersonalAccessToken>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.TokenHash == hash && !x.IsRevoked);

                if (pat != null && !pat.IsExpired)
                {
                    await SetUserPrincipalAsync(context, dbContext, pat.UserId, pat.Name, "PersonalAccessToken");
                    pat.RecordUsage();
                    await dbContext.SaveChangesAsync();
                }
            }
            // 2. Try OAuth Access Token
            else
            {
                var oauthToken = await dbContext.Set<OAuthToken>()
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(x => x.AccessTokenHash == hash && !x.IsRevoked);

                if (oauthToken != null && !oauthToken.IsAccessTokenExpired)
                {
                    await SetUserPrincipalAsync(context, dbContext, oauthToken.UserId, "OAuthClient:" + oauthToken.ClientId, "OAuth");
                    oauthToken.RecordUsage();
                    await dbContext.SaveChangesAsync();
                }
            }
        }

        await next(context);
    }

    private static async Task SetUserPrincipalAsync(
        HttpContext context,
        GoldExDbContext dbContext,
        Guid userId,
        string name,
        string tokenType)
    {
        var userRoles = await dbContext.Set<AppUserRole>()
            .Where(x => x.UserId == userId)
            .Join(dbContext.Set<AppRole>(), ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
            .ToListAsync();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, name),
            new("TokenType", tokenType)
        };

        foreach (var role in userRoles.Where(r => !string.IsNullOrEmpty(r)))
        {
            claims.Add(new Claim(ClaimTypes.Role, role!));
        }

        var identity = new ClaimsIdentity(claims, "Bearer");
        context.User = new ClaimsPrincipal(identity);
    }
}
