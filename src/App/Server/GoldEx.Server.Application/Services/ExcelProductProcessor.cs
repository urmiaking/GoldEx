using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Server.Infrastructure.Models.Spreadsheets;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ExcelProductProcessor(
    IServerProductCategoryService productCategoryService,
    IBarcodeGeneratorService barcodeService,
    IServerCustomerService customerService,
    IServerPriceUnitService priceUnitService)
    : IExcelProductProcessor
{
    public async Task<ProcessExcelResponse> ProcessAsync(
        ParseResult<ExcelProductItem> parsedData,
        CancellationToken cancellationToken = default)
    {
        var resultItems = new List<GetProductItemResponse>();
        var skipped = new List<SkippedRowResponse>();

        foreach (var s in parsedData.SkippedRowDetails)
        {
            skipped.Add(new SkippedRowResponse(
                s.RowIndex,
                s.RowValues,
                s.Reason
            ));
        }

        // جلوگیری از بارکدهای تکراری داخل فایل
        var barcodeSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var rowCounter = 1; 

        foreach (var dto in parsedData.Items)
        {
            var rowIndex = rowCounter++;
            var rowValues = new List<string?>();

            try
            {
                // ====== نام کالا اجباری ======
                if (string.IsNullOrWhiteSpace(dto.Name))
                {
                    skipped.Add(new SkippedRowResponse(rowIndex, rowValues, $"عنوان جنس با بارکد '{dto.Barcode}' خالی است"));
                    continue;
                }

                // ====== دسته‌بندی ======
                ProductCategory? category = null;
                if (!string.IsNullOrWhiteSpace(dto.ProductCategory))
                {
                    category = await productCategoryService
                        .GetOrCreateAsync(dto.ProductCategory!.Trim(), cancellationToken);
                }

                // ====== بارکد ======
                string barcode;
                if (!string.IsNullOrWhiteSpace(dto.Barcode))
                {
                    barcode = dto.Barcode!.Trim();
                    if (!barcodeSet.Add(barcode))
                    {
                        skipped.Add(new SkippedRowResponse(rowIndex, rowValues, $"بارکد '{barcode}' تکراری است"));
                        continue;
                    }

                    await barcodeService.ValidateUniquenessAsync(barcode, cancellationToken);
                }
                else
                {
                    barcode = await barcodeService.GenerateNextAsync(dto.ProductType, category?.Id, cancellationToken);
                    barcodeSet.Add(barcode);
                }

                // ====== واحد اجرت ======
                GetPriceUnitTitleResponse? priceUnit = null;
                if (!string.IsNullOrWhiteSpace(dto.WagePriceUnit))
                {
                    try
                    {
                        priceUnit = await priceUnitService.GetAsync(dto.WagePriceUnit.Trim(), cancellationToken);
                    }
                    catch
                    {
                        skipped.Add(new SkippedRowResponse(rowIndex, rowValues, $"واحد اجرت '{dto.WagePriceUnit}' نامعتبر است"));
                        continue;
                    }
                }

                // ====== طلای آبشده ======
                GetMoltenGoldResponse? molten = null;
                if (dto.ProductType == ProductType.MoltenGold)
                {
                    var (lab, num) = ParseMoltenGold(dto.Name);
                    if (!string.IsNullOrWhiteSpace(lab))
                    {
                        var customer = await customerService.GetOrCreateAsync(lab, CustomerType.AssayingLab, cancellationToken);
                        molten = new GetMoltenGoldResponse(num, customer, null);
                    }
                }

                // ====== ساخت خروجی ======
                var product = new GetProductResponse(
                    Id: Guid.Empty,
                    Name: dto.Name,
                    Barcode: barcode,
                    Weight: dto.Weight,
                    Wage: dto.Wage,
                    ProductType: dto.ProductType,
                    WageType: dto.WageType,
                    Fineness: dto.Fineness,
                    ProductCategoryId: category?.Id.Value,
                    ProductCategoryTitle: category?.Title,
                    WagePriceUnitId: priceUnit?.Id,
                    WagePriceUnitTitle: priceUnit?.Title,
                    DateTime: DateTime.UtcNow,
                    GoldUnitType: GoldUnitType.Gram,
                    StonePriceUnit: null,
                    GemStones: null,
                    MoltenGold: molten
                );

                resultItems.Add(new GetProductItemResponse(product, dto.Quantity));
            }
            catch (Exception ex)
            {
                skipped.Add(new SkippedRowResponse(rowIndex, rowValues, ex.Message));
            }
        }

        return new ProcessExcelResponse(
            TotalRows: parsedData.TotalRows,
            MappedRows: parsedData.MappedRows,
            SkippedRows: parsedData.SkippedRows + skipped.Count,
            SkippedRowDetails: skipped,
            Items: resultItems
        );
    }

    private static (string? Name, string? Number) ParseMoltenGold(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return (null, null);

        var parts = name.Split('-', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2) return (parts[0].Trim(), null);

        return (parts[0].Trim(), parts[1].Trim());
    }
}