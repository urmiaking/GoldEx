using GoldEx.Server.Mcp;
using GoldEx.Shared.Constants;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Controllers;

[ApiController]
[IgnoreAntiforgeryToken]
[AllowAnonymous]
[EnableRateLimiting(RateLimitPolicies.Mcp)]
public class McpController(GoldExMcpEngine mcpEngine) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    private string GetBaseUrl()
    {
        var host = Request.Headers["X-Forwarded-Host"].FirstOrDefault();
        if (string.IsNullOrEmpty(host))
        {
            host = Request.Host.Value;
        }

        return $"https://{host}";
    }

    [HttpOptions(ApiRoutes.Mcp.Base)]
    [HttpOptions(ApiRoutes.Mcp.Message)]
    public IActionResult Options()
    {
        return Ok();
    }

    [HttpPost(ApiRoutes.Mcp.Base)]
    [HttpPost(ApiRoutes.Mcp.Message)]
    public async Task<IActionResult> HandleMessageAsync([FromBody] JsonRpcRequest request, CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            var baseUrl = GetBaseUrl();
            Response.Headers.Append("WWW-Authenticate", $"Bearer error=\"invalid_token\", error_description=\"Unauthorized\", resource_metadata=\"{baseUrl}/.well-known/oauth-protected-resource\", scope=\"mcp\"");
            Response.Headers.Append("Link", $"<{baseUrl}/.well-known/oauth-protected-resource>; rel=\"oauth-protected-resource\", <{baseUrl}/.well-known/oauth-authorization-server>; rel=\"oauth-authorization-server\"");

            return Unauthorized(JsonRpcResponse.CreateError(
                request?.Id ?? "1",
                -32001,
                "عدم احراز هویت. لطفاً از طریق OAuth 2.0 یا کلید دسترسی شخصی (Bearer Token) احراز هویت نمایید."));
        }

        try
        {
            switch (request.Method?.ToLowerInvariant())
            {
                case "initialize":
                    return Ok(JsonRpcResponse.Success(request.Id, new
                    {
                        protocolVersion = "2024-11-05",
                        serverInfo = new
                        {
                            name = "GoldEx MCP Server",
                            version = "1.0.0"
                        },
                        capabilities = new
                        {
                            tools = new { listChanged = false },
                            resources = new { listChanged = false },
                            prompts = new { listChanged = false }
                        }
                    }));

                case "ping":
                    return Ok(JsonRpcResponse.Success(request.Id, new { }));

                case "tools/list":
                    return Ok(JsonRpcResponse.Success(request.Id, new
                    {
                        tools = mcpEngine.GetTools()
                    }));

                case "tools/call":
                    if (request.Params is JsonElement callParams &&
                        callParams.TryGetProperty("name", out var toolNameElem))
                    {
                        var toolName = toolNameElem.GetString() ?? string.Empty;
                        var args = callParams.TryGetProperty("arguments", out var argsElem) ? argsElem : default;

                        var toolResult = await mcpEngine.CallToolAsync(toolName, args, cancellationToken);
                        return Ok(JsonRpcResponse.Success(request.Id, toolResult));
                    }
                    return BadRequest(JsonRpcResponse.CreateError(request.Id, -32602, "پارامترهای فراخوانی ابزار نامعتبر است."));

                case "resources/list":
                    return Ok(JsonRpcResponse.Success(request.Id, new
                    {
                        resources = mcpEngine.GetResources()
                    }));

                case "resources/read":
                    if (request.Params is JsonElement resParams &&
                        resParams.TryGetProperty("uri", out var uriElem))
                    {
                        var uri = uriElem.GetString() ?? string.Empty;
                        if (uri == "goldex://prices/live")
                        {
                            var prices = await mcpEngine.CallToolAsync("get_live_gold_prices", default, cancellationToken);
                            return Ok(JsonRpcResponse.Success(request.Id, new
                            {
                                contents = new[]
                                {
                                    new { uri, mimeType = "text/markdown", text = prices.Content.FirstOrDefault()?.Text ?? "" }
                                }
                            }));
                        }
                    }
                    return NotFound(JsonRpcResponse.CreateError(request.Id, -32602, "منبع درخواستی یافت نشد."));

                case "prompts/list":
                    return Ok(JsonRpcResponse.Success(request.Id, new
                    {
                        prompts = mcpEngine.GetPrompts()
                    }));

                default:
                    return NotFound(JsonRpcResponse.CreateError(
                        request.Id,
                        -32601,
                        $"متد ناشناخته: {request.Method}"));
            }
        }
        catch (Exception ex)
        {
            return StatusCode(500, JsonRpcResponse.CreateError(request.Id, -32603, $"خطای داخلی سرور MCP: {ex.Message}"));
        }
    }

    [HttpGet(ApiRoutes.Mcp.Base)]
    [HttpGet(ApiRoutes.Mcp.Sse)]
    public async Task GetSseAsync(CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            var baseUrl = GetBaseUrl();
            Response.Headers.Append("WWW-Authenticate", $"Bearer error=\"invalid_token\", error_description=\"Unauthorized\", resource_metadata=\"{baseUrl}/.well-known/oauth-protected-resource\", scope=\"mcp\"");
            Response.Headers.Append("Link", $"<{baseUrl}/.well-known/oauth-protected-resource>; rel=\"oauth-protected-resource\", <{baseUrl}/.well-known/oauth-authorization-server>; rel=\"oauth-authorization-server\"");
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            await Response.WriteAsync("Unauthorized. Please authenticate using OAuth 2.0 or Personal Access Token.", cancellationToken);
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        var endpointUrl = $"{GetBaseUrl()}{ApiRoutes.Mcp.Base}";
        await Response.WriteAsync($"event: endpoint\ndata: {endpointUrl}\n\n", cancellationToken);
        await Response.Body.FlushAsync(cancellationToken);

        // Keep connection open for clients that listen to SSE
        while (!cancellationToken.IsCancellationRequested)
        {
            await Task.Delay(15000, cancellationToken);
            await Response.WriteAsync(": keep-alive\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}
