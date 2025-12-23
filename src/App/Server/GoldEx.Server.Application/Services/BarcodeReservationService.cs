using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.BarcodeReservationAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.BarcodeReservations;
using GoldEx.Shared.DTOs.BarcodeReservations;
using GoldEx.Shared.Enums;
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

    public async Task<IssueNextBarcodeResponse> IssueNextAsync(IssueNextBarcodeRequest request,
        CancellationToken cancellationToken = default)
    {
        string? fullPrefix = null;
        string next;

        switch (request.BarcodeType)
        {
            case BarcodeType.Product:
                {
                    if (!request.ProductType.HasValue)
                        throw new ArgumentException("ProductType is required for Product barcode type.", nameof(request.ProductType));

                    var categoryId = request.ProductCategoryId.HasValue ? new ProductCategoryId(request.ProductCategoryId.Value) : (ProductCategoryId?)null;

                    fullPrefix = await generator.BuildProductPrefixAsync(request.ProductType.Value, categoryId, cancellationToken);
                    next = await generator.GenerateNextAsync(request.BarcodeType, request.ProductType, categoryId, cancellationToken);

                    break;
                }
            case BarcodeType.Coin:
                {
                    next = await generator.GenerateNextAsync(request.BarcodeType, null, null, cancellationToken);
                    break;
                }
            default:
                throw new ArgumentOutOfRangeException();
        }

        var reservation = BarcodeReservation.Create(
            barcodeType: request.BarcodeType,
            prefix: fullPrefix,
            barcode: next,
            ttl: DefaultTtl,
            invoiceId: request.InvoiceId.HasValue ? new InvoiceId(request.InvoiceId.Value) : null);

        await reservationRepository.CreateAsync(reservation, cancellationToken);

        return new IssueNextBarcodeResponse(reservation.Barcode);
    }

    public async Task CommitAsync(BarcodeType barcodeType, string barcode, Guid? invoiceId,
        CancellationToken cancellationToken = default)
    {
        var active = await reservationRepository
            .Get(new BarcodeActiveReservationByBarcodeSpecification(barcodeType, barcode))
            .FirstOrDefaultAsync(cancellationToken);

        if (active is null)
            return;

        active.Commit(invoiceId.HasValue ? new InvoiceId(invoiceId.Value) : null);
        await reservationRepository.UpdateAsync(active, cancellationToken);
    }

    public async Task ReleaseAsync(BarcodeType barcodeType, string barcode, CancellationToken cancellationToken = default)
    {
        var active = await reservationRepository
            .Get(new BarcodeActiveReservationByBarcodeSpecification(barcodeType, barcode))
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