using GoldEx.Sdk.Server.Domain.Entities.Identity;
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
    private const string TokenPrefix = "gex_pat_";

    public async Task InvokeAsync(HttpContext context, GoldExDbContext dbContext)
    {
        string? rawToken = null;

        if (context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var headerStr = authHeader.ToString();
            if (headerStr.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var candidate = headerStr["Bearer ".Length..].Trim();
                if (candidate.StartsWith(TokenPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    rawToken = candidate;
                }
            }
        }

        if (string.IsNullOrEmpty(rawToken) && context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyHeader))
        {
            var candidate = apiKeyHeader.ToString().Trim();
            if (candidate.StartsWith(TokenPrefix, StringComparison.OrdinalIgnoreCase))
            {
                rawToken = candidate;
            }
        }

        if (!string.IsNullOrEmpty(rawToken))
        {
            var bytes = Encoding.UTF8.GetBytes(rawToken);
            var hash = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

            var token = await dbContext.Set<PersonalAccessToken>()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.TokenHash == hash && !x.IsRevoked);

            if (token != null && !token.IsExpired)
            {
                var userRoles = await dbContext.Set<AppUserRole>()
                    .Where(x => x.UserId == token.UserId)
                    .Join(dbContext.Set<AppRole>(), ur => ur.RoleId, r => r.Id, (ur, r) => r.Name)
                    .ToListAsync();

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, token.UserId.ToString()),
                    new(ClaimTypes.Name, token.Name),
                    new("TokenType", "PersonalAccessToken")
                };

                foreach (var role in userRoles.Where(r => !string.IsNullOrEmpty(r)))
                {
                    claims.Add(new Claim(ClaimTypes.Role, role!));
                }

                var identity = new ClaimsIdentity(claims, "ApiKey");
                context.User = new ClaimsPrincipal(identity);

                token.RecordUsage();
                await dbContext.SaveChangesAsync();
            }
        }

        await next(context);
    }
}
