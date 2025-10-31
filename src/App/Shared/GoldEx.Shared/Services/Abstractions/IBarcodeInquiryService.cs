using GoldEx.Shared.DTOs.BarcodeInquiries;

namespace GoldEx.Shared.Services.Abstractions;

public interface IBarcodeInquiryService
{
    Task<List<GetBarcodeInquiryResponse>> GetListAsync(string? barcode, CancellationToken cancellationToken = default);
    Task InquiryAsync(string barcode, CancellationToken cancellationToken = default);
}