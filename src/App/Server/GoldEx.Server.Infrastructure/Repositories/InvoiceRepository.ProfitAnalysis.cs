using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.Reporting;
using GoldEx.Server.Infrastructure.Models;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure.Repositories;

internal partial class InvoiceRepository
{
    public async Task<ProfitAnalysisData> GetProfitAnalysisDataAsync(ProfitAnalysisRpRequest request,
        CancellationToken cancellationToken = default)
    {
        var from = request.FromDate.HasValue ? DateOnly.FromDateTime(request.FromDate.Value) : DateOnly.MinValue;
        var to = request.ToDate.HasValue ? DateOnly.FromDateTime(request.ToDate.Value) : DateOnly.MaxValue;
        var selectedSales = Query.AsNoTracking().Where(i => i.InvoiceType == InvoiceType.Sell &&
            i.InvoiceDate >= from && i.InvoiceDate <= to);
        if (request.CustomerId.HasValue)
            selectedSales = selectedSales.Where(i => i.CustomerId == new CustomerId(request.CustomerId.Value));

        var selectedProducts = selectedSales.SelectMany(i => i.ProductItems)
            .Where(i => i.Product != null && (i.Product.ProductType == ProductType.Gold || i.Product.ProductType == ProductType.Jewelry));
        if (request.ProductType.HasValue)
            selectedProducts = selectedProducts.Where(i => i.Product!.ProductType == request.ProductType.Value);
        if (request.CategoryId.HasValue)
            selectedProducts = selectedProducts.Where(i => i.Product!.ProductCategoryId == new ProductCategoryId(request.CategoryId.Value));

        var productIds = await selectedProducts.Select(i => i.ProductId).Distinct().ToListAsync(cancellationToken);
        if (productIds.Count == 0) return new([], []);

        // Load complete history for the selected products, including sales before the requested window.
        // All queries retain the active store's global filters; no per-row database calls.
        var documents = await BuildProfitDocumentsQuery(productIds, to).ToListAsync(cancellationToken);

        var lines = documents.SelectMany(d => d.Items.Select(p => new GoldProfitSource
        {
            ItemId = p.Id, ProductId = p.ProductId, InvoiceId = d.Id, InvoiceNumber = d.InvoiceNumber,
            Date = d.InvoiceDate, CreatedAt = d.CreatedAt, InvoiceType = d.InvoiceType,
            CustomerName = d.CustomerName, CustomerId = d.CustomerId, ProductName = p.ProductName, Barcode = p.Barcode,
            CategoryId = p.CategoryId?.Value, ProductType = p.ProductType,
            Weight = p.TotalWeight, GramPrice = p.GramPrice, RawAmount = p.ItemRawAmount,
            Amount = p.ItemFinalAmount + p.ItemStoneAmount, Tax = p.ItemTaxAmount,
            InvoiceAmount = d.Total, InvoiceDiscount = d.Discount,
            PriceUnitId = d.PriceUnitId, UnitType = d.UnitType,
            BasePriceUnitId = d.BasePriceUnitId, BaseUnitType = d.BaseUnitType,
            BasePriceUnit = d.BasePriceUnit, ExchangeRate = d.ExchangeRate, IsInstant = p.IsInstantProduct
        })).ToList();

        var movementsQuery = dbContext.Set<InventoryStock>().AsNoTracking()
            .Where(s => s.ProductId.HasValue && productIds.Contains(s.ProductId.Value) &&
                (!s.InvoiceId.HasValue || s.ReverseInventoryStockId.HasValue));
        if (request.ToDate.HasValue && request.ToDate.Value.Date < DateTime.MaxValue.Date)
        {
            var endExclusive = request.ToDate.Value.Date.AddDays(1);
            movementsQuery = movementsQuery.Where(s => s.PostingDate < endExclusive);
        }
        var otherMovements = await movementsQuery
            .Select(s => s.ProductId!.Value).Distinct().ToListAsync(cancellationToken);
        return new(lines, otherMovements.Select(id => id.Value).ToHashSet());
    }

    private IQueryable<ProfitAnalysisDocument> BuildProfitDocumentsQuery(List<ProductId> productIds, DateOnly to)
        => Query.AsNoTracking().AsSplitQuery()
            .Where(i => i.InvoiceDate <= to && i.ProductItems.Any(p => productIds.Contains(p.ProductId)))
            .Select(i => new ProfitAnalysisDocument
            {
                Id = i.Id.Value, InvoiceNumber = i.InvoiceNumber, InvoiceDate = i.InvoiceDate,
                CreatedAt = i.CreatedAt, InvoiceType = i.InvoiceType,
                CustomerName = i.Customer != null ? i.Customer.FullName : "",
                CustomerId = i.CustomerId.Value,
                PriceUnitId = i.PriceUnitId.Value, UnitType = i.PriceUnit!.UnitType,
                BasePriceUnitId = i.BasePriceUnitId.Value, BaseUnitType = i.BasePriceUnit!.UnitType,
                BasePriceUnit = i.BasePriceUnit.Title, ExchangeRate = i.ExchangeRate,
                Discount = i.Discounts.Sum(d => d.Amount * (d.ExchangeRate ?? 1m)),
                Total = i.ProductItems.Sum(p => p.ItemFinalAmount + p.ItemStoneAmount) +
                    i.CoinItems.Sum(p => p.ItemFinalAmount) + i.CurrencyItems.Sum(p => p.ItemFinalAmount) +
                    (i.InvoiceType == InvoiceType.Purchase ? i.UsedProducts.Sum(p => p.ItemFinalAmount) : 0m),
                Items = i.ProductItems.Where(p => productIds.Contains(p.ProductId)).Select(p => new ProfitAnalysisProductItem
                {
                    Id = p.Id.Value, ProductId = p.ProductId.Value, TotalWeight = p.TotalWeight, GramPrice = p.GramPrice,
                    ItemRawAmount = p.ItemRawAmount, ItemFinalAmount = p.ItemFinalAmount,
                    ItemStoneAmount = p.ItemStoneAmount, ItemTaxAmount = p.ItemTaxAmount, IsInstantProduct = p.IsInstantProduct,
                    ProductName = p.Product!.Name, Barcode = p.Product.Barcode, ProductType = p.Product.ProductType,
                    CategoryId = p.Product.ProductCategoryId
                }).ToList()
            });
}
