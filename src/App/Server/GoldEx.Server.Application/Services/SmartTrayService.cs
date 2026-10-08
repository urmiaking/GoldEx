using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.CoinInstanceAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.SmartTrayAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.SmartTrays;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Services;

[ScopedService]
public class SmartTrayService(
    ISmartTrayRepository repository,
    IProductService productService,
    ICoinInstanceService coinInstanceService,
    ISmartTrayNotificationPublisher notificationPublisher,
    ILogger<SmartTrayService> logger) : ISmartTrayService
{
    public async Task<List<SmartTrayDto>> GetActiveTraysAsync(CancellationToken cancellationToken = default)
    {
        var trays = await repository.GetActiveTraysAsync(cancellationToken);
        return trays.Select(MapToDto).ToList();
    }

    public async Task<SmartTrayDto> GetTrayByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tray = await repository.GetWithItemsAsync(id, cancellationToken);
        if (tray is null)
            throw new NotFoundException($"سینی هوشمند با شناسه {id} یافت نشد.");

        return MapToDto(tray);
    }

    public async Task<SmartTrayDto> CreateTrayAsync(CreateSmartTrayRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new InvalidOperationException("عنوان سینی نمی‌تواند خالی باشد.");

        var trayNumber = request.TrayNumber.HasValue && request.TrayNumber.Value > 0
            ? request.TrayNumber.Value
            : await repository.GetNextTrayNumberAsync(cancellationToken);

        var tray = SmartTray.Create(
            trayNumber,
            request.Title,
            null,
            request.ClerkName ?? string.Empty);

        await repository.CreateAsync(tray, cancellationToken);

        var dto = MapToDto(tray);
        await notificationPublisher.TrayUpdatedAsync(dto, cancellationToken);
        logger.LogInformation("Smart tray #{TrayNumber} created successfully.", trayNumber);
        return dto;
    }

    public async Task<SmartTrayItemDto> AddItemByBarcodeAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default)
    {
        var normalizedBarcode = NormalizeBarcode(barcode);
        if (string.IsNullOrWhiteSpace(normalizedBarcode))
            throw new InvalidOperationException("بارکد وارد شده معتبر نیست.");

        var tray = await repository.GetWithItemsAsync(trayId, cancellationToken)
            ?? throw new NotFoundException($"سینی با شناسه {trayId} یافت نشد.");

        // First attempt: Product lookup
        var product = await productService.GetAsync(normalizedBarcode, cancellationToken);
        if (product is null && normalizedBarcode.All(char.IsDigit))
        {
            if (normalizedBarcode.Length < 8)
                product = await productService.GetAsync(normalizedBarcode.PadLeft(8, '0'), cancellationToken);
            if (product is null && normalizedBarcode.StartsWith('0'))
                product = await productService.GetAsync(normalizedBarcode.TrimStart('0'), cancellationToken);
        }

        SmartTrayItem item;

        if (product is not null)
        {
            if (product.Weight == 0)
                throw new InvalidOperationException($"کالای «{product.Name}» با بارکد {product.Barcode} قبلاً فروخته شده است و موجودی ندارد.");

            var canonicalBarcode = product.Barcode;
            var isAlreadyInOtherTray = await repository.IsItemInAnyActiveTrayAsync(canonicalBarcode, excludeTrayId: trayId, cancellationToken);
            if (isAlreadyInOtherTray)
                throw new InvalidOperationException($"کالایی با بارکد {canonicalBarcode} در حال حاضر در سینی دیگری روی پیش‌خوان است.");

            item = tray.AddItem(
                ItemType.Product,
                canonicalBarcode,
                product.Name,
                product.Weight,
                product.Fineness,
                product.Wage,
                product.WageType,
                product.ProductCategoryTitle,
                product.Images?.FirstOrDefault()?.Url,
                new ProductId(product.Id),
                null);
        }
        else
        {
            // Second attempt: CoinInstance lookup
            var coinInstance = await coinInstanceService.GetAsync(normalizedBarcode, cancellationToken);
            if (coinInstance is null && normalizedBarcode.All(char.IsDigit))
            {
                if (normalizedBarcode.Length < 8)
                    coinInstance = await coinInstanceService.GetAsync(normalizedBarcode.PadLeft(8, '0'), cancellationToken);
                if (coinInstance is null && normalizedBarcode.StartsWith('0'))
                    coinInstance = await coinInstanceService.GetAsync(normalizedBarcode.TrimStart('0'), cancellationToken);
            }

            if (coinInstance is not null)
            {
                var canonicalBarcode = coinInstance.Barcode;
                var isAlreadyInOtherTray = await repository.IsItemInAnyActiveTrayAsync(canonicalBarcode, excludeTrayId: trayId, cancellationToken);
                if (isAlreadyInOtherTray)
                    throw new InvalidOperationException($"سکه‌ای با بارکد {canonicalBarcode} در حال حاضر در سینی دیگری روی پیش‌خوان است.");

                item = tray.AddItem(
                    ItemType.Coin,
                    canonicalBarcode,
                    coinInstance.Coin?.Title ?? "سکه",
                    coinInstance.Weight,
                    coinInstance.Fineness,
                    null,
                    null,
                    "سکه",
                    null,
                    null,
                    new CoinInstanceId(coinInstance.Id));
            }
            else
            {
                throw new NotFoundException($"کالایی با بارکد {normalizedBarcode} در موجودی اجناس یا سکه‌ها یافت نشد.");
            }
        }

        await repository.UpdateAsync(tray, cancellationToken);

        var itemDto = MapItemToDto(item);
        var trayDto = MapToDto(tray);

        await notificationPublisher.ItemAddedAsync(trayId, itemDto, cancellationToken);
        await notificationPublisher.TrayUpdatedAsync(trayDto, cancellationToken);

        return itemDto;
    }

    public async Task<SmartTrayItemDto> ReturnItemByBarcodeAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default)
    {
        var normalizedBarcode = NormalizeBarcode(barcode);
        var tray = await repository.GetWithItemsAsync(trayId, cancellationToken)
            ?? throw new NotFoundException($"سینی با شناسه {trayId} یافت نشد.");

        var item = tray.MarkItemReturned(normalizedBarcode);
        await repository.UpdateAsync(tray, cancellationToken);

        var itemDto = MapItemToDto(item);
        var trayDto = MapToDto(tray);

        await notificationPublisher.ItemReturnedAsync(trayId, itemDto, cancellationToken);
        await notificationPublisher.TrayUpdatedAsync(trayDto, cancellationToken);

        return itemDto;
    }

    public async Task RemoveItemAsync(Guid trayId, string barcode, CancellationToken cancellationToken = default)
    {
        var normalizedBarcode = NormalizeBarcode(barcode);
        var tray = await repository.GetWithItemsAsync(trayId, cancellationToken)
            ?? throw new NotFoundException($"سینی با شناسه {trayId} یافت نشد.");

        tray.RemoveItem(normalizedBarcode);
        await repository.UpdateAsync(tray, cancellationToken);

        var trayDto = MapToDto(tray);
        await notificationPublisher.ItemRemovedAsync(trayId, normalizedBarcode, cancellationToken);
        await notificationPublisher.TrayUpdatedAsync(trayDto, cancellationToken);
    }

    public async Task<SmartTrayItemDto> MarkItemSoldAsync(Guid trayId, string barcode, Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var normalizedBarcode = NormalizeBarcode(barcode);
        var tray = await repository.GetWithItemsAsync(trayId, cancellationToken)
            ?? throw new NotFoundException($"سینی با شناسه {trayId} یافت نشد.");

        var item = tray.MarkItemSold(normalizedBarcode, new InvoiceId(invoiceId));
        await repository.UpdateAsync(tray, cancellationToken);

        var itemDto = MapItemToDto(item);
        var trayDto = MapToDto(tray);

        await notificationPublisher.ItemSoldAsync(trayId, itemDto, cancellationToken);
        await notificationPublisher.TrayUpdatedAsync(trayDto, cancellationToken);

        return itemDto;
    }

    public async Task CloseTrayAsync(Guid trayId, CancellationToken cancellationToken = default)
    {
        var tray = await repository.GetWithItemsAsync(trayId, cancellationToken)
            ?? throw new NotFoundException($"سینی با شناسه {trayId} یافت نشد.");

        tray.CloseTray(Guid.Empty);
        await repository.UpdateAsync(tray, cancellationToken);

        var trayDto = MapToDto(tray);
        await notificationPublisher.TrayClosedAsync(trayId, cancellationToken);
        await notificationPublisher.TrayUpdatedAsync(trayDto, cancellationToken);
    }

    public async Task ReportDiscrepancyAsync(Guid trayId, ReportSmartTrayDiscrepancyRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var tray = await repository.GetWithItemsAsync(trayId, cancellationToken)
            ?? throw new NotFoundException($"سینی با شناسه {trayId} یافت نشد.");

        tray.ReportDiscrepancy(request.Notes, Guid.Empty);
        await repository.UpdateAsync(tray, cancellationToken);

        var trayDto = MapToDto(tray);
        await notificationPublisher.DiscrepancyReportedAsync(trayId, request.Notes, cancellationToken);
        await notificationPublisher.TrayUpdatedAsync(trayDto, cancellationToken);
    }

    public async Task CancelTrayAsync(Guid trayId, CancellationToken cancellationToken = default)
    {
        var tray = await repository.GetWithItemsAsync(trayId, cancellationToken)
            ?? throw new NotFoundException($"سینی با شناسه {trayId} یافت نشد.");

        tray.CancelTray();
        await repository.UpdateAsync(tray, cancellationToken);

        var trayDto = MapToDto(tray);
        await notificationPublisher.TrayUpdatedAsync(trayDto, cancellationToken);
    }

    private static string NormalizeBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return string.Empty;

        return barcode.Trim()
            .Replace('۰', '0').Replace('۱', '1').Replace('۲', '2').Replace('۳', '3').Replace('۴', '4')
            .Replace('۵', '5').Replace('۶', '6').Replace('۷', '7').Replace('۸', '8').Replace('۹', '9')
            .Replace('٠', '0').Replace('١', '1').Replace('٢', '2').Replace('٣', '3').Replace('٤', '4')
            .Replace('٥', '5').Replace('٦', '6').Replace('٧', '7').Replace('٨', '8').Replace('٩', '9');
    }

    private static SmartTrayDto MapToDto(SmartTray tray)
    {
        return new SmartTrayDto
        {
            Id = tray.Id.Value,
            TrayNumber = tray.TrayNumber,
            Title = tray.Title,
            ClerkId = tray.ClerkId,
            ClerkName = tray.ClerkName,
            Status = tray.Status,
            StatusTitle = tray.Status switch
            {
                SmartTrayStatus.Active => "فعال روی پیش‌خوان",
                SmartTrayStatus.Auditing => "در حال تطبیق با ویترین",
                SmartTrayStatus.Completed => "تکمیل و بسته شده",
                SmartTrayStatus.HasDiscrepancy => "دارای کسری و مغایرت",
                SmartTrayStatus.Cancelled => "لغو شده",
                _ => tray.Status.ToString()
            },
            CreatedAt = tray.CreatedAt,
            ClosedAt = tray.ClosedAt,
            DiscrepancyNotes = tray.DiscrepancyNotes,
            DiscrepancyApprovedBy = tray.DiscrepancyApprovedBy,
            Items = tray.Items.Select(MapItemToDto).ToList()
        };
    }

    private static SmartTrayItemDto MapItemToDto(SmartTrayItem item)
    {
        return new SmartTrayItemDto
        {
            Id = item.Id.Value,
            SmartTrayId = item.SmartTrayId.Value,
            ItemType = item.ItemType,
            ProductId = item.ProductId?.Value,
            CoinInstanceId = item.CoinInstanceId?.Value,
            Barcode = item.Barcode,
            Title = item.Title,
            Weight = item.Weight,
            Fineness = item.Fineness,
            Wage = item.Wage,
            WageType = item.WageType,
            CategoryTitle = item.CategoryTitle,
            ImageUrl = item.ImageUrl,
            Status = item.Status,
            StatusTitle = item.Status switch
            {
                SmartTrayItemStatus.InTray => "روی پیش‌خوان",
                SmartTrayItemStatus.Sold => "فروخته شده",
                SmartTrayItemStatus.Returned => "بازگشت به ویترین",
                SmartTrayItemStatus.Missing => "مفقود شده",
                _ => item.Status.ToString()
            },
            AddedAt = item.AddedAt,
            ReturnedAt = item.ReturnedAt,
            SoldAt = item.SoldAt,
            InvoiceId = item.InvoiceId?.Value,
            DurationSeconds = item.Duration.TotalSeconds
        };
    }
}
