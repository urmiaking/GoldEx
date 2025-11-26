using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.BarcodeReservationAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.BarcodeReservations;
using GoldEx.Shared.DTOs.BarcodeReservations;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal sealed class BarcodeReservationService(
    IBarcodeReservationRepository reservationRepository,
    IBarcodeGeneratorService generator
) : IBarcodeReservationService, IServerBarcodeReservationService
{
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(5);

    private const int MaxRetryAttempts = 5;

    public async Task<IssueNextBarcodeResponse> IssueNextAsync(IssueNextBarcodeRequest request,
        CancellationToken cancellationToken = default)
    {
        var categoryId = request.ProductCategoryId.HasValue ? new ProductCategoryId(request.ProductCategoryId.Value) : (ProductCategoryId?)null;

        var fullPrefix = await generator.BuildFullPrefixAsync(request.ProductType, categoryId, cancellationToken);
        DbUpdateException? lastException = null;

        for (var attempt = 1; attempt <= MaxRetryAttempts; attempt++)
        {
            var next = await generator.GenerateNextAsync(request.ProductType, categoryId, cancellationToken);

            var reservation = BarcodeReservation.Create(
                prefix: fullPrefix,
                barcode: next,
                ttl: DefaultTtl,
                invoiceId: request.InvoiceId.HasValue ? new InvoiceId(request.InvoiceId.Value) : null);

            try
            {
                await reservationRepository.CreateAsync(reservation, cancellationToken);
                return new IssueNextBarcodeResponse(fullPrefix, reservation.Barcode, reservation.ExpiresAt);
            }
            catch (DbUpdateException ex)
            {
                // Race condition detected: another request reserved the same barcode.
                lastException = ex;
                // Continue to retry with a new barcode.
            }
        }

        // If all retries fail, throw an exception with the last database error as inner exception
        throw new InvalidOperationException("امکان رزرو بارکد پس از چندین تلاش وجود ندارد. لطفاً دوباره تلاش کنید.", lastException);
    }

    public async Task CommitAsync(string barcode, Guid? invoiceId, CancellationToken cancellationToken = default)
    {
        var active = await reservationRepository
            .Get(new BarcodeActiveReservationByBarcodeSpecification(barcode))
            .FirstOrDefaultAsync(cancellationToken);

        if (active is null)
            return;

        active.Commit(invoiceId.HasValue ? new InvoiceId(invoiceId.Value) : null);
        await reservationRepository.UpdateAsync(active, cancellationToken);
    }

    public async Task ReleaseAsync(string barcode, CancellationToken cancellationToken = default)
    {
        var active = await reservationRepository
            .Get(new BarcodeActiveReservationByBarcodeSpecification(barcode))
            .FirstOrDefaultAsync(cancellationToken);

        if (active is null)
            return;

        active.Release();
        await reservationRepository.UpdateAsync(active, cancellationToken);
    }

    public async Task<int> CleanupExpiredAsync(CancellationToken cancellationToken = default)
    {
        var expired = await reservationRepository
            .Get(new BarcodeExpiredReservationsSpecification())
            .ToListAsync(cancellationToken);

        foreach (var r in expired)
        {
            r.Expire();
        }

        if (expired.Count > 0)
            await reservationRepository.UpdateRangeAsync(expired, cancellationToken);

        return expired.Count;
    }
}