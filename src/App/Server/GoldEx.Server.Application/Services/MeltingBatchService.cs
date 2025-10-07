using FluentValidation;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Application.Validators.MeltingBatches;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.MeltingBatchAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.MeltingBatches;
using GoldEx.Server.Infrastructure.Specifications.Products;
using GoldEx.Server.Infrastructure.Specifications.Settings;
using GoldEx.Shared.DTOs.MeltingBatches;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Data;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class MeltingBatchService(
    IMeltingBatchRepository repository,
    ISettingRepository settingRepository,
    IProductRepository productRepository,
    IServerInventoryStockService inventoryService,
    IMapper mapper,
    ILogger<MeltingBatchService> logger,
    CreateMeltingBatchRequestValidator createValidator,
    SendToLabRequestValidator labRequestValidator,
    CompleteMeltingRequestValidator completeMeltingRequestValidator) : IMeltingBatchService
{
    public async Task<PagedList<GetMeltingBatchResponse>> GetListAsync(RequestFilter requestFilter, MeltingBatchFilter filter,
        CancellationToken cancellationToken = default)
    {
        var spec = new MeltingBatchesByFilterSpecification(requestFilter, filter);

        var list = await repository
            .Get(spec)
            .ToListAsync(cancellationToken);

        var total = await repository.CountAsync(spec, cancellationToken);

        return new PagedList<GetMeltingBatchResponse>
        {
            Data = mapper.Map<List<GetMeltingBatchResponse>>(list),
            Total = total,
            Skip = requestFilter.Skip ?? 0,
            Take = requestFilter.Take ?? 100
        };
    }

    public async Task<GetMeltingBatchResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new MeltingBatchesByIdSpecification(new MeltingBatchId(id)))
            .FirstOrDefaultAsync(cancellationToken) 
                   ?? throw new NotFoundException($"Melting batch with id '{id}' not found.");

        return mapper.Map<GetMeltingBatchResponse>(item);
    }

    public async Task CreateAsync(MeltingBatchRequestDto request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                var productIds = request.ProductIds.Select(x => new ProductId(x)).ToList();

                var products = await productRepository
                    .Get(new ProductsByIdsSpecification(productIds))
                    .ToListAsync(cancellationToken);

                var (selectedUnit, totalWeight) = await GetWeightUnitAsync(products, cancellationToken);

                var item = MeltingBatch.Create(request.Description, totalWeight, selectedUnit);
                await repository.CreateAsync(item, cancellationToken);

                await inventoryService.MeltProductsAsync(item.Id, productIds, cancellationToken);

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

    public async Task SendToLabAsync(Guid id, SendToLabRequestDto request, CancellationToken cancellationToken = default)
    {
        await labRequestValidator.ValidateAndThrowAsync((id, request), cancellationToken);

        var item = await repository
            .Get(new MeltingBatchesByIdSpecification(new MeltingBatchId(id)))
            .FirstOrDefaultAsync(cancellationToken) 
                   ?? throw new NotFoundException($"Melting batch with id '{id}' not found.");

        item.SendToLab(new CustomerId(request.AssayerId), request.Description);

        await repository.UpdateAsync(item, cancellationToken);
    }

    public async Task CompleteMeltingAsync(Guid id, CompleteMeltingRequestDto request, CancellationToken cancellationToken = default)
    {
        await completeMeltingRequestValidator.ValidateAndThrowAsync((id, request), cancellationToken);

        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                var item = await repository
                               .Get(new MeltingBatchesByIdSpecification(new MeltingBatchId(id)))
                               .FirstOrDefaultAsync(cancellationToken)
                           ?? throw new NotFoundException($"Melting batch with id '{id}' not found.");

                item.CompleteMelting(request.Description);

                await repository.UpdateAsync(item, cancellationToken);

                await inventoryService.CreateMoltenGoldAsync(item, request.AssayNumber, request.Fineness, request.Weight, cancellationToken);

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

    private async Task<(GoldUnitType selectedUnit, decimal totalWeight)> GetWeightUnitAsync(List<Product> products, CancellationToken cancellationToken)
    {
        decimal totalWeight;
        GoldUnitType selectedUnit;

        var gramBasedProducts = products.Where(p => p.GoldUnitType == GoldUnitType.Gram).ToList();
        var mesghalBasedProducts = products.Where(p => p.GoldUnitType == GoldUnitType.Mesghal).ToList();

        var setting = await settingRepository.Get(new SettingsDefaultSpecification()).FirstOrDefaultAsync(cancellationToken);

        var mesghalToGramFactor = setting?.GramPerMesghal ?? 4.6083m;

        if (gramBasedProducts.Count > mesghalBasedProducts.Count)
        {
            selectedUnit = GoldUnitType.Gram;
            var gramWeights = gramBasedProducts.Sum(p => p.Weight);
            var convertedMesghalWeights = mesghalBasedProducts.Sum(p => p.Weight * mesghalToGramFactor);
            totalWeight = gramWeights + convertedMesghalWeights;
        }
        else if (mesghalBasedProducts.Count > gramBasedProducts.Count)
        {
            selectedUnit = GoldUnitType.Mesghal;
            var mesghalWeights = mesghalBasedProducts.Sum(p => p.Weight);
            var convertedGramWeights = gramBasedProducts.Sum(p => p.Weight / mesghalToGramFactor);
            totalWeight = mesghalWeights + convertedGramWeights;
        }
        else
        {
            selectedUnit = GoldUnitType.Gram;
            var gramWeights = gramBasedProducts.Sum(p => p.Weight);
            var convertedMesghalWeights = mesghalBasedProducts.Sum(p => p.Weight * mesghalToGramFactor);
            totalWeight = gramWeights + convertedMesghalWeights;
        }

        return (selectedUnit, totalWeight);
    }
}