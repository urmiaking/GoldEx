using System.Net;
using Microsoft.AspNetCore.Http;

namespace GoldEx.Sdk.Server.Api.Extensions;

public static class HttpContextExtensions
{
    extension(HttpContext context)
    {
        public IPAddress? GetClientIpAddress()
        {
            var forwardedFor = context.Request.Headers["X-Forwarded-For"].ToString();

            if (string.IsNullOrEmpty(forwardedFor))
                return context.Connection.RemoteIpAddress;

            return IPEndPoint.TryParse(forwardedFor, out var endPoint) ? endPoint.Address : null;
        }

        public string GetClientUserAgent()
        {
            return context.Request.Headers["User-Agent"].ToString();
        }
    }
}
