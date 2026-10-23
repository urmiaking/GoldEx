using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Reporting;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Tests.Server.Infrastructure.Repositories;

[TestFixture]
public class ProfitAnalysisRepositoryTester
{
    [Test]
    public void HistoricalProjection_TranslatesToSqlServerWithoutConnecting()
    {
        var options = new DbContextOptionsBuilder<GoldExDbContext>()
            .UseSqlServer("Server=localhost;Database=profit-analysis-translation-only;Integrated Security=true;TrustServerCertificate=true").Options;
        using var db = new GoldExDbContext(options, new StoreContext { StoreId = Guid.NewGuid() });
        var repositoryType = typeof(GoldExDbContext).Assembly.GetType("GoldEx.Server.Infrastructure.Repositories.InvoiceRepository")!;
        var repository = Activator.CreateInstance(repositoryType, db)!;
        var method = repositoryType.GetMethod("BuildProfitDocumentsQuery", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!;
        var query = (IQueryable)method.Invoke(repository, [new List<ProductId> { new(Guid.NewGuid()) }, new DateOnly(2026, 10, 31)])!;
        var sql = query.ToQueryString();
        Assert.That(sql, Does.Contain("StoreId"));
        Assert.That(sql, Does.Contain("InvoiceDate"));
        Assert.That(sql, Does.Contain("InvoiceDiscounts"));
    }

    private class StoreContext : IStoreContext
    {
        public Guid? StoreId { get; set; }
        public string? StoreSlug { get; set; }
        public string? CustomDomain { get; set; }
        public bool IsCustomDomain => false;
    }

    [TestCase("bounded")]
    [TestCase("all")]
    [TestCase("from")]
    [TestCase("until")]
    [TestCase("max")]
    public async Task ReportQuery_ProjectsHistoricalAmountsAndHonorsTenantFilter(string range)
    {
        var context = new StoreContext { StoreId = Guid.NewGuid() };
        var firstStore = context.StoreId;
        var options = new DbContextOptionsBuilder<GoldExDbContext>()
            .UseInMemoryDatabase($"profit-analysis-{Guid.NewGuid()}").Options;
        await using var db = new GoldExDbContext(options, context);
        var currency = PriceUnit.Create("تومان", UnitType.TMN);
        db.Add(currency);
        var first = await Seed(db, currency, "فروشگاه اول");
        context.StoreId = Guid.NewGuid();
        await Seed(db, currency, "فروشگاه دوم");
        context.StoreId = firstStore;
        db.ChangeTracker.Clear();

        var repository = (IInvoiceRepository)Activator.CreateInstance(
            typeof(GoldExDbContext).Assembly.GetType("GoldEx.Server.Infrastructure.Repositories.InvoiceRepository")!, db)!;
        var request = range switch
        {
            "all" => new ProfitAnalysisRpRequest(),
            "from" => new ProfitAnalysisRpRequest(FromDate: new(2026, 10, 1)),
            "until" => new ProfitAnalysisRpRequest(ToDate: new(2026, 10, 31)),
            "max" => new ProfitAnalysisRpRequest(ToDate: DateTime.MaxValue),
            _ => new ProfitAnalysisRpRequest(new(2026, 10, 1), new(2026, 10, 31))
        };
        var data = await repository.GetProfitAnalysisDataAsync(request);
        Assert.That(data.Lines, Has.Count.EqualTo(2));
        Assert.That(data.Lines.All(x => x.ProductName == "فروشگاه اول"), Is.True);
        Assert.That(data.Lines.Single(x => x.InvoiceType == InvoiceType.Sell).RawAmount, Is.EqualTo(70000000m).Within(0.000001m));
        var report = ProfitAnalysisBuilder.Build(data.Lines, data.ProductsWithOtherMovements, request);
        Assert.That(report.CalculatedCount, Is.EqualTo(1));
        Assert.That(report.Summaries.Single().PurchaseCost, Is.EqualTo(62000000m));
        Assert.That(report.Summaries.Single().GoldPriceEffect, Is.EqualTo(10000000m).Within(0.000001m));

        var filtered = await repository.GetProfitAnalysisDataAsync(request with { CustomerId = Guid.NewGuid() });
        Assert.That(filtered.Lines, Is.Empty);

        // Editing today's product profile must not recalculate the historical raw gold component.
        var edited = await db.Set<Product>().SingleAsync(p => p.Id == first.Id);
        edited.SetFineness(900m).SetWeight(3m);
        await db.SaveChangesAsync();
        var afterProfileEdit = await repository.GetProfitAnalysisDataAsync(request);
        Assert.That(afterProfileEdit.Lines.Single(x => x.InvoiceType == InvoiceType.Sell).RawAmount,
            Is.EqualTo(data.Lines.Single(x => x.InvoiceType == InvoiceType.Sell).RawAmount));

        db.Add(InventoryStock.CreateProduct(first.Id, 1m, WarehouseActionType.In,
            postingDate: new DateTime(2026, 10, 2)));
        await db.SaveChangesAsync();
        var withManualEntry = await repository.GetProfitAnalysisDataAsync(request);
        Assert.That(withManualEntry.ProductsWithOtherMovements, Does.Contain(first.Id.Value));
        Assert.That(ProfitAnalysisBuilder.Build(withManualEntry.Lines, withManualEntry.ProductsWithOtherMovements, request).CalculatedCount, Is.Zero);
    }

    private static async Task<Product> Seed(GoldExDbContext db, PriceUnit currency, string name)
    {
        var customer = Customer.Create(CustomerType.RetailCustomer, name, "", null, null, null, null);
        var product = Product.Create(name, 10m, 0m, ProductType.Gold, 750m, GoldUnitType.Gram,
            WageType.Fixed, currency.Id, null, null).SetBarcode(Guid.NewGuid().ToString("N"));
        var buy = Invoice.Create(1, null, null, InvoiceType.Purchase, TradeScale.Retail,
            customer.Id, currency.Id, currency.Id, null, new(2026, 10, 1), null);
        buy.AddPurchaseProductItem(null, 6000000m, 0m, 0m, 1, 10m, 62000000m, null, null,
            currency.Id, false, 0m, WageType.Fixed, currency.Id, null, product);
        var sale = Invoice.Create(2, null, null, InvoiceType.Sell, TradeScale.Retail,
            customer.Id, currency.Id, currency.Id, null, new(2026, 10, 20), null);
        sale.AddSaleProductItem(null, 7000000m, 0m, 0m, 1, 10m, null, null, null, false,
            400000m, WageType.Fixed, currency.Id, null, null, product);
        db.AddRange(customer, product, buy, sale);
        await db.SaveChangesAsync();
        return product;
    }
}
