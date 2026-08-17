using GoldEx.Sdk.Server.Application.Abstractions;
using GoldEx.Server.Application.Abstractions;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Shared.DTOs.OAuth;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Controllers;

[ApiController]
[IgnoreAntiforgeryToken]
public class OAuthController(
    IOAuthService oauthService,
    IStoreContext storeContext,
    GoldExDbContext dbContext) : ControllerBase
{
    // -------------------------------------------------------------
    // 1. RFC 9728: OAuth 2.0 Protected Resource Metadata
    // -------------------------------------------------------------
    [HttpGet("/.well-known/oauth-protected-resource")]
    [HttpGet("/.well-known/oauth-protected-resource/mcp")]
    [HttpGet("/.well-known/oauth-protected-resource/api/mcp")]
    [HttpGet("/mcp/.well-known/oauth-protected-resource")]
    [AllowAnonymous]
    public IActionResult GetProtectedResourceMetadata()
    {
        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var baseUrl = $"{scheme}://{host}";

        var metadata = new OAuthProtectedResourceMetadata
        {
            Resource = $"{baseUrl}/mcp",
            AuthorizationServers = [baseUrl],
            ScopesSupported = ["mcp", "read", "write"],
            BearerMethodsSupported = ["header"]
        };

        return Ok(metadata);
    }

    // -------------------------------------------------------------
    // 2. RFC 8414: OAuth 2.0 Authorization Server Metadata
    // -------------------------------------------------------------
    [HttpGet("/.well-known/oauth-authorization-server")]
    [HttpGet("/.well-known/oauth-authorization-server/oauth")]
    [HttpGet("/.well-known/openid-configuration")]
    [AllowAnonymous]
    public IActionResult GetAuthorizationServerMetadata()
    {
        var scheme = Request.Scheme;
        var host = Request.Host.Value;
        var baseUrl = $"{scheme}://{host}";

        var metadata = new OAuthAuthorizationServerMetadata
        {
            Issuer = baseUrl,
            AuthorizationEndpoint = $"{baseUrl}/oauth/authorize",
            TokenEndpoint = $"{baseUrl}/oauth/token",
            RegistrationEndpoint = $"{baseUrl}/oauth/register",
            RevocationEndpoint = $"{baseUrl}/oauth/revoke",
            ResponseTypesSupported = ["code"],
            ResponseModesSupported = ["query", "fragment"],
            GrantTypesSupported = ["authorization_code", "refresh_token"],
            TokenEndpointAuthMethodsSupported = ["client_secret_post", "client_secret_basic", "none"],
            CodeChallengeMethodsSupported = ["S256", "plain"],
            ScopesSupported = ["mcp", "read", "write", "openid", "profile"],
            SubjectTypesSupported = ["public"]
        };

        return Ok(metadata);
    }

    // -------------------------------------------------------------
    // 3. RFC 7591: Dynamic Client Registration
    // -------------------------------------------------------------
    [HttpPost("/oauth/register")]
    [AllowAnonymous]
    public async Task<IActionResult> RegisterClientAsync(CancellationToken cancellationToken)
    {
        try
        {
            ClientRegistrationRequest? request = null;

            if (Request.HasFormContentType)
            {
                var form = await Request.ReadFormAsync(cancellationToken);
                request = new ClientRegistrationRequest
                {
                    ClientName = form["client_name"].ToString(),
                    TokenEndpointAuthMethod = form["token_endpoint_auth_method"].ToString(),
                    Scope = form["scope"].ToString()
                };

                var redirectUrisStr = form["redirect_uris"].ToString();
                if (!string.IsNullOrEmpty(redirectUrisStr))
                {
                    request.RedirectUris = redirectUrisStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }

                var grantTypesStr = form["grant_types"].ToString();
                if (!string.IsNullOrEmpty(grantTypesStr))
                {
                    request.GrantTypes = grantTypesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }

                var responseTypesStr = form["response_types"].ToString();
                if (!string.IsNullOrEmpty(responseTypesStr))
                {
                    request.ResponseTypes = responseTypesStr.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
                }
            }
            else if (Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync(cancellationToken);
                if (!string.IsNullOrWhiteSpace(body))
                {
                    var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    request = JsonSerializer.Deserialize<ClientRegistrationRequest>(body, jsonOptions);
                }
            }

            request ??= new ClientRegistrationRequest();

            var response = await oauthService.RegisterClientAsync(request, cancellationToken);
            return StatusCode(StatusCodes.Status201Created, response);
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "invalid_client_metadata", error_description = ex.Message });
        }
    }

    // -------------------------------------------------------------
    // 4. RFC 6749 & RFC 7636: Authorization Endpoint (Consent Page)
    // -------------------------------------------------------------
    [HttpGet("/oauth/authorize")]
    [AllowAnonymous]
    public async Task<IActionResult> AuthorizeGetAsync(
        [FromQuery(Name = "response_type")] string? responseType,
        [FromQuery(Name = "client_id")] string? clientId,
        [FromQuery(Name = "redirect_uri")] string? redirectUri,
        [FromQuery(Name = "scope")] string? scope,
        [FromQuery(Name = "state")] string? state,
        [FromQuery(Name = "code_challenge")] string? codeChallenge,
        [FromQuery(Name = "code_challenge_method")] string? codeChallengeMethod,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(redirectUri))
        {
            return BadRequest("پارامترهای client_id و redirect_uri الزامی هستند.");
        }

        // If user is not logged in, redirect to login page with returnUrl
        if (User.Identity?.IsAuthenticated != true)
        {
            var currentUrl = $"{Request.Path}{Request.QueryString}";
            return Redirect($"/Account/Login?returnUrl={WebUtility.UrlEncode(currentUrl)}");
        }

        var client = await oauthService.GetClientAsync(clientId, cancellationToken);
        var clientDisplayName = client?.ClientName ?? (clientId.Contains("gemini", StringComparison.OrdinalIgnoreCase) ? "Google Gemini" :
            clientId.Contains("chatgpt", StringComparison.OrdinalIgnoreCase) ? "ChatGPT" : "دستیار هوش مصنوعی (MCP Client)");

        var storeTitle = "فروشگاه گلدکس";
        if (storeContext.StoreId.HasValue)
        {
            var store = await dbContext.Set<Store>()
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == new StoreId(storeContext.StoreId.Value), cancellationToken);

            if (store != null)
            {
                storeTitle = store.Name;
            }
        }

        var userName = User.Identity?.Name ?? "کاربر";

        var html = GetConsentHtml(
            clientName: clientDisplayName,
            storeName: storeTitle,
            userName: userName,
            clientId: clientId,
            redirectUri: redirectUri,
            scope: scope ?? "mcp",
            state: state ?? "",
            codeChallenge: codeChallenge ?? "",
            codeChallengeMethod: codeChallengeMethod ?? "S256");

        return Content(html, "text/html; charset=utf-8");
    }

    [HttpPost("/oauth/authorize")]
    public async Task<IActionResult> AuthorizePostAsync(
        [FromForm(Name = "action")] string? action,
        [FromForm(Name = "client_id")] string clientId,
        [FromForm(Name = "redirect_uri")] string redirectUri,
        [FromForm(Name = "scope")] string? scope,
        [FromForm(Name = "state")] string? state,
        [FromForm(Name = "code_challenge")] string? codeChallenge,
        [FromForm(Name = "code_challenge_method")] string? codeChallengeMethod,
        CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized();
        }

        if (action == "deny")
        {
            var stateParam = !string.IsNullOrEmpty(state) ? $"&state={WebUtility.UrlEncode(state)}" : "";
            return Redirect($"{redirectUri}?error=access_denied&error_description=User%20denied%20access{stateParam}");
        }

        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var storeId = storeContext.StoreId.HasValue ? new StoreId(storeContext.StoreId.Value) : default;

        var authCode = await oauthService.CreateAuthorizationCodeAsync(
            clientId: clientId,
            userId: userId,
            storeId: storeId,
            redirectUri: redirectUri,
            scope: scope,
            codeChallenge: codeChallenge,
            codeChallengeMethod: codeChallengeMethod,
            cancellationToken: cancellationToken);

        var separator = redirectUri.Contains('?') ? "&" : "?";
        var redirectUrl = $"{redirectUri}{separator}code={WebUtility.UrlEncode(authCode.Code)}";
        if (!string.IsNullOrEmpty(state))
        {
            redirectUrl += $"&state={WebUtility.UrlEncode(state)}";
        }

        return Redirect(redirectUrl);
    }

    // -------------------------------------------------------------
    // 5. RFC 6749: Token Exchange Endpoint
    // -------------------------------------------------------------
    [HttpPost("/oauth/token")]
    [AllowAnonymous]
    public async Task<IActionResult> TokenAsync(CancellationToken cancellationToken)
    {
        OAuthTokenRequest? tokenRequest = null;

        if (Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync(cancellationToken);
            tokenRequest = new OAuthTokenRequest
            {
                GrantType = form["grant_type"].ToString(),
                Code = form["code"].ToString(),
                RedirectUri = form["redirect_uri"].ToString(),
                ClientId = form["client_id"].ToString(),
                ClientSecret = form["client_secret"].ToString(),
                CodeVerifier = form["code_verifier"].ToString(),
                RefreshToken = form["refresh_token"].ToString(),
                Scope = form["scope"].ToString()
            };
        }
        else if (Request.ContentType?.Contains("application/json", StringComparison.OrdinalIgnoreCase) == true)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(body))
            {
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                tokenRequest = JsonSerializer.Deserialize<OAuthTokenRequest>(body, jsonOptions);
            }
        }

        tokenRequest ??= new OAuthTokenRequest();

        // Support Basic Auth header for client credentials (client_id:client_secret)
        if (string.IsNullOrEmpty(tokenRequest.ClientId) &&
            Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var headerStr = authHeader.ToString();
            if (headerStr.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            {
                var credsBase64 = headerStr["Basic ".Length..].Trim();
                try
                {
                    var credsBytes = Convert.FromBase64String(credsBase64);
                    var creds = System.Text.Encoding.UTF8.GetString(credsBytes).Split(':', 2);
                    if (creds.Length >= 1) tokenRequest.ClientId = creds[0];
                    if (creds.Length == 2) tokenRequest.ClientSecret = creds[1];
                }
                catch { }
            }
        }

        try
        {
            if (tokenRequest.GrantType == "authorization_code")
            {
                var response = await oauthService.ExchangeCodeForTokenAsync(tokenRequest, cancellationToken);
                return Ok(response);
            }
            else if (tokenRequest.GrantType == "refresh_token")
            {
                var response = await oauthService.RefreshTokenAsync(tokenRequest.RefreshToken ?? "", tokenRequest.ClientId, cancellationToken);
                return Ok(response);
            }
            else
            {
                return BadRequest(new { error = "unsupported_grant_type", error_description = $"نوع مجوز '{tokenRequest.GrantType}' پشتیبانی نمی‌شود." });
            }
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = "invalid_grant", error_description = ex.Message });
        }
    }

    // -------------------------------------------------------------
    // 6. RFC 7009: Token Revocation Endpoint
    // -------------------------------------------------------------
    [HttpPost("/oauth/revoke")]
    [AllowAnonymous]
    public async Task<IActionResult> RevokeAsync([FromForm(Name = "token")] string? formToken, CancellationToken cancellationToken)
    {
        var token = formToken;
        if (string.IsNullOrEmpty(token) && Request.HasFormContentType)
        {
            var form = await Request.ReadFormAsync(cancellationToken);
            token = form["token"].ToString();
        }

        if (!string.IsNullOrEmpty(token))
        {
            await oauthService.RevokeTokenAsync(token, cancellationToken);
        }

        return Ok(new { status = "revoked" });
    }

    // -------------------------------------------------------------
    // HTML UI Generator for OAuth Consent Screen
    // -------------------------------------------------------------
    private static string GetConsentHtml(
        string clientName,
        string storeName,
        string userName,
        string clientId,
        string redirectUri,
        string scope,
        string state,
        string codeChallenge,
        string codeChallengeMethod)
    {
        return $$"""
        <!DOCTYPE html>
        <html lang="fa" dir="rtl">
        <head>
            <meta charset="utf-8" />
            <meta name="viewport" content="width=device-width, initial-scale=1.0" />
            <title>اجازه دسترسی به هوش مصنوعی - گلدکس</title>
            <link rel="stylesheet" href="/css/app.css" />
            <style>
                body {
                    margin: 0;
                    padding: 0;
                    font-family: system-ui, -apple-system, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;
                    background-color: #0f172a;
                    color: #f8fafc;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    min-height: 100vh;
                }
                .consent-card {
                    background: #1e293b;
                    border: 1px solid rgba(255, 255, 255, 0.1);
                    border-radius: 16px;
                    box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.5);
                    max-width: 480px;
                    width: 90%;
                    padding: 32px;
                    text-align: center;
                }
                .app-icon {
                    width: 64px;
                    height: 64px;
                    background: linear-gradient(135deg, #d97706, #f59e0b);
                    border-radius: 16px;
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    margin: 0 auto 20px auto;
                    font-size: 32px;
                    box-shadow: 0 10px 15px -3px rgba(217, 119, 6, 0.4);
                }
                .title {
                    font-size: 1.25rem;
                    font-weight: 700;
                    margin-bottom: 8px;
                    color: #ffffff;
                }
                .subtitle {
                    font-size: 0.9rem;
                    color: #94a3b8;
                    margin-bottom: 24px;
                    line-height: 1.5;
                }
                .info-box {
                    background: #0f172a;
                    border-radius: 12px;
                    padding: 16px;
                    text-align: right;
                    margin-bottom: 24px;
                    border: 1px solid rgba(255, 255, 255, 0.05);
                }
                .info-item {
                    display: flex;
                    align-items: center;
                    justify-content: space-between;
                    font-size: 0.85rem;
                    padding: 6px 0;
                    border-bottom: 1px solid rgba(255, 255, 255, 0.05);
                }
                .info-item:last-child {
                    border-bottom: none;
                }
                .info-label {
                    color: #94a3b8;
                }
                .info-val {
                    color: #f1f5f9;
                    font-weight: 600;
                }
                .permissions-list {
                    text-align: right;
                    font-size: 0.85rem;
                    color: #cbd5e1;
                    margin-top: 12px;
                    padding-right: 20px;
                }
                .permissions-list li {
                    margin-bottom: 6px;
                }
                .btn-group {
                    display: flex;
                    gap: 12px;
                }
                .btn {
                    flex: 1;
                    padding: 12px 20px;
                    border-radius: 10px;
                    font-size: 0.95rem;
                    font-weight: 600;
                    cursor: pointer;
                    border: none;
                    transition: all 0.2s ease;
                }
                .btn-primary {
                    background: linear-gradient(135deg, #d97706, #f59e0b);
                    color: #ffffff;
                }
                .btn-primary:hover {
                    opacity: 0.9;
                    transform: translateY(-1px);
                }
                .btn-secondary {
                    background: rgba(255, 255, 255, 0.1);
                    color: #e2e8f0;
                }
                .btn-secondary:hover {
                    background: rgba(255, 255, 255, 0.15);
                }
            </style>
        </head>
        <body>
            <div class="consent-card">
                <div class="app-icon">🤖</div>
                <div class="title">اتصال دستیار هوش مصنوعی</div>
                <div class="subtitle">
                    نرم‌افزار <strong>{{WebUtility.HtmlEncode(clientName)}}</strong> درخواست دسترسی به اطلاعات فروشگاه شما را دارد.
                </div>

                <div class="info-box">
                    <div class="info-item">
                        <span class="info-label">فروشگاه فعال:</span>
                        <span class="info-val">{{WebUtility.HtmlEncode(storeName)}}</span>
                    </div>
                    <div class="info-item">
                        <span class="info-label">کاربر تاییدکننده:</span>
                        <span class="info-val">{{WebUtility.HtmlEncode(userName)}}</span>
                    </div>
                    <div class="info-item">
                        <span class="info-label">دسترسی‌های مجاز:</span>
                        <span class="info-val" style="color: #10b981;">ابزارهای هوش مصنوعی MCP</span>
                    </div>
                    <ul class="permissions-list">
                        <li>استعلام نرخ‌های لحظه‌ای طلا، سکه و ارز</li>
                        <li>مشاهده مانده حساب و بدهی مشتریان</li>
                        <li>استعلام موجودی انبار طلا، سکه و ارز</li>
                        <li>محاسبات قیمت طلا، سود و فاکتورها</li>
                    </ul>
                </div>

                <form method="post" action="/oauth/authorize">
                    <input type="hidden" name="client_id" value="{{WebUtility.HtmlEncode(clientId)}}" />
                    <input type="hidden" name="redirect_uri" value="{{WebUtility.HtmlEncode(redirectUri)}}" />
                    <input type="hidden" name="scope" value="{{WebUtility.HtmlEncode(scope)}}" />
                    <input type="hidden" name="state" value="{{WebUtility.HtmlEncode(state)}}" />
                    <input type="hidden" name="code_challenge" value="{{WebUtility.HtmlEncode(codeChallenge)}}" />
                    <input type="hidden" name="code_challenge_method" value="{{WebUtility.HtmlEncode(codeChallengeMethod)}}" />

                    <div class="btn-group">
                        <button type="submit" name="action" value="allow" class="btn btn-primary">
                            تایید و اتصال به هوش مصنوعی
                        </button>
                        <button type="submit" name="action" value="deny" class="btn btn-secondary">
                            انصراف
                        </button>
                    </div>
                </form>
            </div>
        </body>
        </html>
        """;
    }
}
