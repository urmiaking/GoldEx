using System.Data;
using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Application.Validators.PriceUnits;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.LedgerAccounts;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class PriceUnitService(
    IPriceUnitRepository repository,
    ILedgerAccountRepository ledgerAccountRepository,
    IFileService fileService,
    IWebHostEnvironment webHostEnvironment,
    IMapper mapper,
    ILogger<PriceUnitService> logger,
    CreatePriceUnitRequestValidator createValidator,
    UpdatePriceUnitRequestValidator updateValidator) : IPriceUnitService
{
    public async Task<GetPriceUnitResponse> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(id)))
            .AsNoTracking()
            .Include(x => x.Price)
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        return mapper.Map<GetPriceUnitResponse>(item);
    }

    public async Task<GetPriceUnitResponse?> GetDefaultAsync(CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .AsNoTracking()
            .Include(x => x.Price)
            .FirstOrDefaultAsync(cancellationToken);

        return item is null ? null : mapper.Map<GetPriceUnitResponse>(item);
    }

    public async Task<List<GetPriceUnitResponse>> GetListAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository
            .Get(new PriceUnitsDefaultSpecification())
            .AsNoTracking()
            .Include(x => x.Price)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetPriceUnitResponse>>(items);
    }

    public async Task<List<GetPriceUnitResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository
            .Get(new PriceUnitsWithoutSpecification())
            .AsNoTracking()
            .Include(x => x.Price)
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetPriceUnitResponse>>(items);
    }

    public async Task<List<GetPriceUnitTitleResponse>> GetTitlesAsync(CancellationToken cancellationToken = default)
    {
        var items = await repository
            .Get(new PriceUnitsDefaultSpecification())
            .AsNoTracking()
            .AsSplitQuery()
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetPriceUnitTitleResponse>>(items);
    }

    public async Task CreateAsync(CreatePriceUnitRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await createValidator.ValidateAndThrowAsync(request, cancellationToken);

                var cashLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByTitleSpecification(SystemLedgerAccounts.CashAccounts))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException("Cash ledger account not found");

                var requestedPriceUnitLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByTitleSpecification(LedgerAccountTitleBuilder.ForCurrencyAccount(request.Title)))
                    .FirstOrDefaultAsync(cancellationToken);

                LedgerAccountId ledgerAccountId;

                if (requestedPriceUnitLedgerAccount is not null)
                {
                    ledgerAccountId = requestedPriceUnitLedgerAccount.Id;
                }
                else
                {
                    var priceUnitLedgerAccount = LedgerAccount.CreateSystemAccount(
                        LedgerAccountTitleBuilder.ForCurrencyAccount(request.Title),
                        LedgerAccountType.Asset,
                        cashLedgerAccount.Id);

                    await ledgerAccountRepository.CreateAsync(priceUnitLedgerAccount, cancellationToken);

                    ledgerAccountId = priceUnitLedgerAccount.Id;
                }

                var item = PriceUnit.Create(request.Title,
                    ledgerAccountId,
                    false,
                    request.PriceId.HasValue
                        ? new PriceId(request.PriceId.Value)
                        : null);

                await repository.CreateAsync(item, cancellationToken);

                if (request.IconContent is not null)
                    await fileService.SaveLocalFileAsync(webHostEnvironment.GetPriceUnitIconPath(
                        item.Id.Value, null), request.IconContent, cancellationToken);

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

    public async Task UpdateAsync(Guid id, UpdatePriceUnitRequest request, CancellationToken cancellationToken = default)
    {
        await using var dbTransaction = await repository.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
        {
            try
            {
                await updateValidator.ValidateAndThrowAsync((id, request), cancellationToken);

                var item = await repository
                    .Get(new PriceUnitsByIdSpecification(new PriceUnitId(id)))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                item.SetTitle(request.Title);
                item.SetPriceId(request.PriceId.HasValue ? new PriceId(request.PriceId.Value) : null);

                await repository.UpdateAsync(item, cancellationToken);

                var priceUnitLedgerAccount = await ledgerAccountRepository
                    .Get(new LedgerAccountsByIdSpecification(item.LedgerAccountId))
                    .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

                priceUnitLedgerAccount.SetTitle(LedgerAccountTitleBuilder.ForCurrencyAccount(item.Title));

                await ledgerAccountRepository.UpdateAsync(priceUnitLedgerAccount, cancellationToken);

                if (request.IconContent is not null)
                    await fileService.ReplaceLocalFileAsync(webHostEnvironment.GetPriceUnitIconPath(
                        item.Id.Value, null), request.IconContent, cancellationToken);

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

    public async Task UpdateStatusAsync(Guid id, UpdatePriceUnitStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        item.SetStatus(request.IsActive);

        await repository.UpdateAsync(item, cancellationToken);
    }

    public async Task SetAsDefaultAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var item = await repository
            .Get(new PriceUnitsByIdSpecification(new PriceUnitId(id)))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new NotFoundException();

        var defaultItems = await repository
            .Get(new PriceUnitsSetAsDefaultSpecification())
            .ToListAsync(cancellationToken);

        foreach (var defaultItem in defaultItems)
            defaultItem.SetDefault(false);

        item.SetDefault(true);

        defaultItems.Add(item);

        await repository.UpdateRangeAsync(defaultItems, cancellationToken);
    }
}