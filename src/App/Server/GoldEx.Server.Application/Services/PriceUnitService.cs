using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Application.Validators.PriceUnits;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Services;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class PriceUnitService(
    IPriceUnitRepository repository,
    IFileService fileService,
    IWebHostEnvironment webHostEnvironment,
    IMapper mapper,
    CreatePriceUnitRequestValidator createValidator,
    UpdatePriceUnitRequestValidator updateValidator) : IPriceUnitService
{
    public async Task<GetPriceUnitResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(id)))
            .Include(x => x.Price)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetPriceUnitResponse>(item);
    }

    public async Task<List<GetPriceUnitResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository
            .Get(new PriceUnitsDefaultSpecification())
            .Include(x => x.Price)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetPriceUnitResponse>>(items);
    }

    public async Task<List<GetPriceUnitResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository
            .Get(new PriceUnitsWithoutSpecification())
            .Include(x => x.Price)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetPriceUnitResponse>>(items);
    }

    public async Task<List<GetPriceUnitTitleResponse>> GetTitlesAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository
            .Get(new PriceUnitsDefaultSpecification())
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetPriceUnitTitleResponse>>(items);
    }

    public async Task CreateAsync(CreatePriceUnitRequest request, CancellationToken cancellationToken = default)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var item = PriceUnit.Create(request.Title, request.PriceId.HasValue ? new PriceId(request.PriceId.Value) : null);

        await repository.CreateAsync(item, cancellationToken);

        if (request.IconContent is not null)
            await fileService.SaveLocalFileAsync(webHostEnvironment.GetPriceUnitIconPath(
                item.Id.Value, null), request.IconContent, cancellationToken);
    }

    public async Task UpdateAsync(Guid id, UpdatePriceUnitRequest request, CancellationToken cancellationToken = default)
    {
        await updateValidator.ValidateAndThrowAsync((id, request), cancellationToken);
        var item = await repository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetTitle(request.Title);
        item.SetPriceId(request.PriceId.HasValue ? new PriceId(request.PriceId.Value) : null);

        await repository.UpdateAsync(item, cancellationToken);

        if (request.IconContent is not null)
            await fileService.ReplaceLocalFileAsync(webHostEnvironment.GetPriceUnitIconPath(
                item.Id.Value, null), request.IconContent, cancellationToken);
    }

    public async Task SetStatus(Guid id, UpdatePriceUnitStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetStatus(request.IsActive);

        await repository.UpdateAsync(item, cancellationToken);
    }
}