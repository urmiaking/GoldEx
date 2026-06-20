using GoldEx.Sdk.Common;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Api;
using GoldEx.Shared.DTOs.Stores;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Controllers;

[Route(ApiRoutes.Stores.Base)]
[Authorize]
public class StoresController(IStoreService storeService) : ApiControllerBase
{
    [HttpPost(ApiRoutes.Stores.Switch)]
    public async Task<IActionResult> SwitchStoreAsync(Guid storeId, CancellationToken cancellationToken)
    {
        await storeService.SwitchStoreAsync(storeId, cancellationToken);
        return Ok();
    }

    [HttpGet(ApiRoutes.Stores.GetUserStores)]
    public async Task<IActionResult> GetUserStoresAsync(CancellationToken cancellationToken)
    {
        var result = await storeService.GetUserStoresAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet(ApiRoutes.Stores.GetList)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> GetListAsync([FromQuery] RequestFilter filter, CancellationToken cancellationToken)
    {
        var result = await storeService.GetListAsync(filter, cancellationToken);
        return Ok(result);
    }

    [HttpPost(ApiRoutes.Stores.Create)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> CreateStoreAsync([FromBody] StoreRequest request, CancellationToken cancellationToken)
    {
        await storeService.CreateStoreAsync(request, cancellationToken);
        return Created();
    }

    [HttpPut(ApiRoutes.Stores.Update)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> UpdateStoreAsync([FromRoute] Guid id, [FromBody] StoreRequest request, CancellationToken cancellationToken)
    {
        await storeService.UpdateStoreAsync(id, request, cancellationToken);
        return NoContent();
    }

    [HttpDelete(ApiRoutes.Stores.Delete)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> DeleteStoreAsync([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        await storeService.DeleteStoreAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet(ApiRoutes.Stores.GetStoreUsers)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> GetStoreUsersAsync([FromRoute] Guid storeId, CancellationToken cancellationToken)
    {
        var result = await storeService.GetStoreUsersAsync(storeId, cancellationToken);
        return Ok(result);
    }

    [HttpPost(ApiRoutes.Stores.AssignStoreUsers)]
    [Authorize(Roles = BuiltinRoles.Administrators)]
    public async Task<IActionResult> AssignStoreUsersAsync([FromRoute] Guid storeId, [FromBody] AssignStoreUsersRequest request, CancellationToken cancellationToken)
    {
        await storeService.AssignStoreUsersAsync(storeId, request, cancellationToken);
        return NoContent();
    }
}
