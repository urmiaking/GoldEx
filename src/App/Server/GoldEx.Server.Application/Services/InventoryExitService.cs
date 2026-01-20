using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.InventoryExits;
using GoldEx.Server.Domain.InventoryExitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.InventoryExits;
using GoldEx.Shared.DTOs.InventoryExits;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InventoryExitService(
    IInventoryExitRepository repository,
    IServerInventoryStockService inventoryStockService,
    IAccountingTransactionService transactionService,
    IMapper mapper,
    ILogger<InventoryExitService> logger,
    CreateInventoryExitRequestValidator validator) : IInventoryExitService
{
    public async Task<PagedList<InventoryExitResponse>> GetListAsync(RequestFilter filter, CancellationToken cancellationToken = default)
    {
        var spec = new InventoryExitsByFilterSpecification(filter);

        var list = await repository.Get(spec).ToListAsync(cancellationToken);
        var total = await repository.CountAsync(spec, cancellationToken);

        return new PagedList<InventoryExitResponse>
        {
            Data = mapper.Map<List<InventoryExitResponse>>(list),
            Total = total,
            Skip = filter.Skip ?? 0,
            Take = filter.Take ?? 100
        };
    }

    public async Task ExitAsync(CreateInventoryExitRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            await validator.ValidateAndThrowAsync(request, cancellationToken);

            try
            {
                var inventoryExit = InventoryExit.Create(request.ExitReason, request.Description, request.ExitDate);
                await repository.CreateAsync(inventoryExit, cancellationToken);

                var inventoryStocks = await inventoryStockService.ExitInventoryAsync(inventoryExit.Id, request, cancellationToken);
                await transactionService.CreateForInventoryExitAsync(inventoryExit.Id, request, inventoryStocks, cancellationToken);

                await dbTransaction.CommitAsync(cancellationToken);
            }
            catch (Exception e)
            {
                logger.LogError(e, e.Message);
                await dbTransaction.RollbackAsync(cancellationToken);
                throw;
            }
        }
    }

    public async Task RollbackAsync(Guid id, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}