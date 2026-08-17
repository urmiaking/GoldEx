using GoldEx.Server.Mcp;
using GoldEx.Shared.Routings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Controllers;

[ApiController]
public class McpController(GoldExMcpEngine mcpEngine) : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    [HttpPost(ApiRoutes.Mcp.Base)]
    [HttpPost(ApiRoutes.Mcp.Message)]
    public async Task<IActionResult> HandleMessageAsync([FromBody] JsonRpcRequest request, CancellationToken cancellationToken)
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Unauthorized(JsonRpcResponse.CreateError(
                request.Id,
                -32001,
                "عدم احراز هویت. لطفاً کلید دسترسی معتبر (Bearer Token) ارسال فرمایید."));
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
            Response.StatusCode = StatusCodes.Status401Unauthorized;
            await Response.WriteAsync("Unauthorized. Please provide a valid Bearer token.", cancellationToken);
            return;
        }

        Response.ContentType = "text/event-stream";
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        var endpointUrl = $"{Request.Scheme}://{Request.Host}{ApiRoutes.Mcp.Base}";
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
