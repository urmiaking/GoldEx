using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.InventoryExits;
using GoldEx.Server.Domain.InventoryExitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.InventoryExits;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class InventoryExitService(
    IInventoryExitRepository repository,
    IServerInventoryStockService inventoryStockService,
    IAccountingTransactionService transactionService,
    ILogger<InventoryExitService> logger,
    CreateInventoryExitRequestValidator validator) : IInventoryExitService
{
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
}