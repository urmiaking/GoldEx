using GoldEx.Server.Common.Authorization;
using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.MoltenGold;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.MoltenGoldDesk)]
[Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}, Demo")]
public class MoltenGoldDeskController(IMoltenGoldTradeService service) : ApiControllerBase
{
    [HttpGet("settings")]
    [DemoRead]
    public Task<MoltenGoldDeskSettings> GetSettingsAsync(CancellationToken ct) => service.GetSettingsAsync(ct);

    [HttpGet("stock")]
    [DemoRead]
    public Task<List<MoltenGoldStockChoice>> SearchStockAsync([FromQuery] string query, CancellationToken ct)
        => service.SearchStockAsync(query, ct);

    [HttpGet]
    [DemoRead]
    public Task<List<MoltenGoldTradeResponse>> GetListAsync([FromQuery] MoltenGoldTradeFilter filter, [FromQuery] int skip, CancellationToken ct)
        => service.GetListAsync(filter, skip, ct);

    [HttpGet("{id:guid}/settlements")]
    [DemoRead]
    public Task<List<MoltenGoldSettlementChoice>> SearchSettlementsAsync(Guid id, [FromQuery] string query = "", CancellationToken ct = default)
        => service.SearchSettlementsAsync(id, query, ct);

    [HttpPost("{id:guid}/gold-settlement")]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> SettleGoldAsync(Guid id, SettleMoltenGold request, CancellationToken ct)
    {
        await service.SettleGoldAsync(id, request, ct);
        return NoContent();
    }

    [HttpGet("{id:guid}/payment-trades")]
    [DemoRead]
    public Task<List<MoltenGoldPaymentChoice>> SearchPaymentTradesAsync(Guid id, [FromQuery] Guid customerId, CancellationToken ct)
        => service.SearchPaymentTradesAsync(id, customerId, ct);

    [HttpPost]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> CreateAsync(CreateMoltenGoldTrade request, CancellationToken ct)
    {
        await service.CreateAsync(request, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/delivery")]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> DeliverAsync(Guid id, DeliverMoltenGold request, CancellationToken ct)
    {
        await service.DeliverAsync(id, request, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/payment")]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> PayAsync(Guid id, PayMoltenGold request, CancellationToken ct)
    {
        await service.PayAsync(id, request, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/transfer")]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> TransferAsync(Guid id, TransferMoltenGold request, CancellationToken ct)
    {
        await service.TransferAsync(id, request, ct);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = $"{BuiltinRoles.Administrators}, {BuiltinRoles.Owners}")]
    public async Task<IActionResult> CancelAsync(Guid id, CancellationToken ct)
    {
        await service.CancelAsync(id, ct);
        return NoContent();
    }
}
