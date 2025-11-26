using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Server.Infrastructure.Models.Spreadsheets;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Services.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.PriceUnits;
using GoldEx.Shared.DTOs.InventoryEntries;
using GoldEx.Shared.DTOs.PriceUnits;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Application.Services;

[ScopedService]
internal class ExcelProductProcessor(
    IServerProductCategoryService productCategoryService,
    IBarcodeGeneratorService barcodeService,
    IServerCustomerService customerService,
    IServerPriceUnitService priceUnitService,
    IPriceUnitRepository priceUnitRepository,
    IPriceService priceService,
    ICategoryPredictor categoryPredictor)
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

        var gramPriceUnit = await priceUnitRepository
            .Get(new PriceUnitsByUnitTypeSpecification(UnitType.Gold18K))
            .FirstOrDefaultAsync(cancellationToken) ?? throw new InvalidOperationException("واحد قیمت گرم 18 عیار یافت نشد");

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
                else
                {
                    var output = categoryPredictor.Predict(dto.ToAiInput());

                    category = await productCategoryService
                        .GetOrCreateAsync(output, cancellationToken);
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
                GetPriceUnitTitleResponse? wagePriceUnit = null;
                decimal? wagePriceUnitExchangeRate = null;
                if (!string.IsNullOrWhiteSpace(dto.WagePriceUnit))
                {
                    try
                    {
                        wagePriceUnit = await priceUnitService.GetAsync(dto.WagePriceUnit, cancellationToken);

                        var exchangeRateResponse = await priceService.GetExchangeRateAsync(
                            wagePriceUnit.Id, gramPriceUnit.Id.Value, cancellationToken);

                        wagePriceUnitExchangeRate = exchangeRateResponse.ExchangeRate;

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
                    WagePriceUnitId: wagePriceUnit?.Id,
                    WagePriceUnitTitle: wagePriceUnit?.Title,
                    DateTime: DateTime.Now,
                    GoldUnitType: GoldUnitType.Gram,
                    StonePriceUnit: null,
                    GemStones: null,
                    MoltenGold: molten
                );

                resultItems.Add(new GetProductItemResponse(product, dto.Quantity, wagePriceUnitExchangeRate, gramPriceUnit.Id.Value));
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