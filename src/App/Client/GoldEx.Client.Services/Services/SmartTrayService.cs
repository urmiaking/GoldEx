using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Shared.DTOs.SmartTrays;
using GoldEx.Shared.Routings;
using GoldEx.Shared.Services.Abstractions;
using System.Net.Http.Json;
using System.Text.Json;

namespace GoldEx.Client.Services.Services;

[ScopedService]
internal class SmartTrayService(HttpClient client, JsonSerializerOptions jsonOptions) : ISmartTrayService
{
    public async Task<List<SmartTrayDto>> GetActiveTraysAsync(CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.SmartTrays.GetActive(), cancellationToken);
        if (!response.IsSuccessStatusCode)
            await HandleErrorResponseAsync(response, "خطا در دریافت لیست سینی‌های فعال", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<List<SmartTrayDto>>(jsonOptions, cancellationToken);
        return result ?? [];
    }

    public async Task<SmartTrayDto> GetTrayByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await client.GetAsync(ApiUrls.SmartTrays.GetById(id), cancellationToken);
        if (!response.IsSuccessStatusCode)
            await HandleErrorResponseAsync(response, "سینی مورد نظر یافت نشد.", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<SmartTrayDto>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<SmartTrayDto> CreateTrayAsync(CreateSmartTrayRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.SmartTrays.Create(), request, jsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
            await HandleErrorResponseAsync(response, "خطا در ایجاد سینی جدید.", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<SmartTrayDto>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<SmartTrayItemDto> AddItemByBarcodeAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default)
    {
        var request = new AddSmartTrayItemRequest { Barcode = barcode };
        using var response = await client.PostAsJsonAsync(ApiUrls.SmartTrays.AddItem(trayId), request, jsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
            await HandleErrorResponseAsync(response, $"کالایی با بارکد {barcode} یافت نشد.", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<SmartTrayItemDto>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task<SmartTrayItemDto> ReturnItemByBarcodeAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(ApiUrls.SmartTrays.ReturnItem(trayId, barcode), null, cancellationToken);
        if (!response.IsSuccessStatusCode)
            await HandleErrorResponseAsync(response, "خطا در بازگرداندن کالا به ویترین.", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<SmartTrayItemDto>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task RemoveItemAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default)
    {
        using var response = await client.DeleteAsync(ApiUrls.SmartTrays.RemoveItem(trayId, barcode), cancellationToken);
        if (!response.IsSuccessStatusCode)
            await HandleErrorResponseAsync(response, "خطا در حذف کالا از سینی.", cancellationToken);
    }

    public async Task<SmartTrayItemDto> MarkItemSoldAsync(Guid trayId, string barcode, Guid invoiceId, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(ApiUrls.SmartTrays.MarkSold(trayId, barcode, invoiceId), null, cancellationToken);
        if (!response.IsSuccessStatusCode)
            await HandleErrorResponseAsync(response, "خطا در ثبت فروش کالا.", cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<SmartTrayItemDto>(jsonOptions, cancellationToken);
        return result ?? throw new UnexpectedHttpResponseException();
    }

    public async Task CloseTrayAsync(Guid trayId, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(ApiUrls.SmartTrays.Close(trayId), null, cancellationToken);
        if (!response.IsSuccessStatusCode)
            await HandleErrorResponseAsync(response, "خطا در بستن سینی.", cancellationToken);
    }

    public async Task ReportDiscrepancyAsync(Guid trayId, ReportSmartTrayDiscrepancyRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsJsonAsync(ApiUrls.SmartTrays.Discrepancy(trayId), request, jsonOptions, cancellationToken);
        if (!response.IsSuccessStatusCode)
            await HandleErrorResponseAsync(response, "خطا در ثبت مغایرت سینی.", cancellationToken);
    }

    public async Task CancelTrayAsync(Guid trayId, CancellationToken cancellationToken = default)
    {
        using var response = await client.PostAsync(ApiUrls.SmartTrays.Cancel(trayId), null, cancellationToken);
        if (!response.IsSuccessStatusCode)
            await HandleErrorResponseAsync(response, "خطا در لغو سینی.", cancellationToken);
    }

    private static async Task HandleErrorResponseAsync(HttpResponseMessage response, string defaultMessage, CancellationToken cancellationToken)
    {
        string? errorText = null;
        try
        {
            errorText = await response.Content.ReadAsStringAsync(cancellationToken);
        }
        catch { }

        var message = !string.IsNullOrWhiteSpace(errorText) ? errorText : defaultMessage;
        if (message.StartsWith('"') && message.EndsWith('"') && message.Length > 1)
        {
            message = message.Trim('"');
        }

        throw new NotFoundException(message);
    }
}
