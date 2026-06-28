using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Domain.StoneTypeAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.StoneTypes;
using GoldEx.Shared.DTOs.StoneTypes;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class StoneTypeService(
    IStoneTypeRepository repository,
    IMapper mapper) : IStoneTypeService
{
    public async Task<List<GetStoneTypeResponse>> GetListAsync(StoneTypeRequestFilter filter, CancellationToken cancellationToken = default)
    {
        var requestFilter = new GoldEx.Sdk.Common.Data.RequestFilter
        {
            Skip = 0,
            Take = 1000
        };

        var items = await repository
            .Get(new StoneTypesByFilterSpecification(requestFilter, filter))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetStoneTypeResponse>>(items);
    }

    public async Task<GetStoneTypeResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new StoneTypesByIdSpecification(new StoneTypeId(id)))
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetStoneTypeResponse>(item);
    }

    public async Task CreateAsync(CreateStoneTypeRequest request, CancellationToken cancellationToken = default)
    {
        if (await repository.ExistsAsync(new StoneTypesByTitleSpecification(request.Title), cancellationToken))
            throw new InvalidOperationException("نام فارسی سنگ تکراری است");
        if (await repository.ExistsAsync(new StoneTypesByEnTitleSpecification(request.EnTitle), cancellationToken))
            throw new InvalidOperationException("نام انگلیسی سنگ تکراری است");
        if (await repository.ExistsAsync(new StoneTypesBySymbolSpecification(request.Symbol), cancellationToken))
            throw new InvalidOperationException("نماد سنگ تکراری است");

        var stoneType = StoneType.Create(request.Title, request.EnTitle, request.Symbol, request.Kind);
        await repository.CreateAsync(stoneType, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdateStoneTypeRequest request, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new StoneTypesByIdSpecification(new StoneTypeId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        if (item.Title != request.Title && await repository.ExistsAsync(new StoneTypesByTitleSpecification(request.Title), cancellationToken))
            throw new InvalidOperationException("نام فارسی سنگ تکراری است");
        if (item.EnTitle != request.EnTitle && await repository.ExistsAsync(new StoneTypesByEnTitleSpecification(request.EnTitle), cancellationToken))
            throw new InvalidOperationException("نام انگلیسی سنگ تکراری است");
        if (item.Symbol != request.Symbol && await repository.ExistsAsync(new StoneTypesBySymbolSpecification(request.Symbol), cancellationToken))
            throw new InvalidOperationException("نماد سنگ تکراری است");

        item.Update(request.Title, request.EnTitle, request.Symbol);
        await repository.UpdateAsync(item, cancellationToken);
    }

    public async Task ToggleStatusAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new StoneTypesByIdSpecification(new StoneTypeId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetActive(!item.IsActive);
        await repository.UpdateAsync(item, cancellationToken);
    }
}
