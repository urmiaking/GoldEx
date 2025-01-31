using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace GoldEx.Sdk.Server.Api.Authorization;

public class AuthorizePoliciesAttribute : TypeFilterAttribute
{
    public AuthorizePoliciesAttribute(params string[] policies) : base(typeof(AuthorizePoliciesFilter))
    {
        Arguments = [policies];
    }
}

public class AuthorizePoliciesFilter(IAuthorizationService authService, string[] policies) : IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        foreach (var policy in policies)
        {
            var authResult = await authService.AuthorizeAsync(context.HttpContext.User, policy);
            if (authResult.Succeeded)
                return;
        }

        context.Result = new UnauthorizedObjectResult(string.Empty);
    }
}
