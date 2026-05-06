using GoldEx.Sdk.Server.Application.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace GoldEx.Sdk.Server.Application.Services;

internal sealed class UserContext(IServiceProvider serviceProvider) : IUserContext
{
    private IHttpContextAccessor? HttpContextAccessor => serviceProvider.GetService<IHttpContextAccessor>();
    private HttpContext? HttpContext => HttpContextAccessor?.HttpContext;

    public Guid? GetUserId()
    {
        if (HttpContext is null)
            return null;

        var userIdString = HttpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (Guid.TryParse(userIdString, out var userIdGuid))
            return userIdGuid;

        return null;
    }

    public bool IsInRole(string role)
    {
        return HttpContext is not null && HttpContext.User.IsInRole(role);
    }

    public bool IsAuthenticated()
    {
        return HttpContext?.User.Identity?.IsAuthenticated ?? false;
    }
}