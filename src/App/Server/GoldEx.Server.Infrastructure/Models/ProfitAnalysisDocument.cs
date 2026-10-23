using GoldEx.Server.Domain.ProductCategoryAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Models;

internal class ProfitAnalysisDocument
{
    public Guid Id { get; init; }
    public long InvoiceNumber { get; init; }
    public DateOnly InvoiceDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public InvoiceType InvoiceType { get; init; }
    public Guid CustomerId { get; init; }
    public string CustomerName { get; init; } = "";
    public Guid PriceUnitId { get; init; }
    public UnitType? UnitType { get; init; }
    public Guid BasePriceUnitId { get; init; }
    public UnitType? BaseUnitType { get; init; }
    public string BasePriceUnit { get; init; } = "";
    public decimal? ExchangeRate { get; init; }
    public decimal Discount { get; init; }
    public decimal Total { get; init; }
    public List<ProfitAnalysisProductItem> Items { get; init; } = [];
}

internal class ProfitAnalysisProductItem
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public decimal TotalWeight { get; init; }
    public decimal GramPrice { get; init; }
    public decimal ItemRawAmount { get; init; }
    public decimal ItemFinalAmount { get; init; }
    public decimal ItemStoneAmount { get; init; }
    public decimal ItemTaxAmount { get; init; }
    public bool IsInstantProduct { get; init; }
    public string ProductName { get; init; } = "";
    public string Barcode { get; init; } = "";
    public ProductType ProductType { get; init; }
    public ProductCategoryId? CategoryId { get; init; }
}
