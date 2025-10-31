using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Validators.BarcodeInquiries;
using GoldEx.Server.Domain.BarcodeInquiryAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.BarcodeInquiries;
using GoldEx.Shared.DTOs.BarcodeInquiries;
using GoldEx.Shared.Services.Abstractions;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class BarcodeInquiryService(
    IBarcodeInquiryRepository repository,
    IMapper mapper,
    BarcodeInquiryRequestValidator validator) : IBarcodeInquiryService
{
    public async Task<List<GetBarcodeInquiryResponse>> GetListAsync(string? barcode, CancellationToken cancellationToken = default)
    {
        var items = await repository
            .Get(new BarcodeInquiriesDefaultSpecification(barcode))
            .ToListAsync(cancellationToken);

        return mapper.Map<List<GetBarcodeInquiryResponse>>(items);
    }

    public async Task InquiryAsync(string barcode, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(barcode, cancellationToken);

        var existingBarcode = await repository
            .Get(new BarcodeInquiriesByBarcodeSpecification(barcode))
            .FirstOrDefaultAsync(cancellationToken);

        if (existingBarcode is not null)
        {
            existingBarcode.SetInquiryDate();
            await repository.UpdateAsync(existingBarcode, cancellationToken);
        }
        else
        {
            var newInquiry = BarcodeInquiry.Create(barcode);
            await repository.CreateAsync(newInquiry, cancellationToken);
        }
    }
}