using FluentValidation;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Application.Seeders;
using GoldEx.Server.Application.Services;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.FinancialAccountAggregate;
using GoldEx.Server.Domain.InventoryStockAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.MoltenGoldTradeAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.PriceAggregate;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Extensions;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Constants;
using GoldEx.Shared.DTOs.Invoices;
using GoldEx.Shared.DTOs.MoltenGold;
using GoldEx.Shared.DTOs.Products;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GoldEx.Tests.Server.Application.Services;

public sealed class MoltenGoldDeskSqlTester
{
    [Test]
    [Platform("Win")]
    public async Task ReservesStockAndPostsPartialDelivery_AndReusesTransferForBothInvoices()
    {
        await using var app = CreateTestApp();
        await using var scope = app.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var store = services.GetRequiredService<StoreContext>();
        var storeId = Guid.NewGuid();
        store.SetStore(storeId, "desk-test");
        var db = services.GetRequiredService<GoldExDbContext>();
        try
        {
            await VerifyPhysicalDeskAsync(services, store, storeId, db);
        }
        finally
        {
            await db.Database.EnsureDeletedAsync();
        }
    }

    private static WebApplication CreateTestApp()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "AGENTS.md")))
            root = root.Parent;
        Assert.That(root, Is.Not.Null);
        var serverRoot = Path.Combine(root!.FullName, "src/App/Server/GoldEx.Server");
        var builder = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            ApplicationName = "GoldEx.Server",
            EnvironmentName = "Development",
            ContentRootPath = serverRoot,
            WebRootPath = Path.Combine(serverRoot, "wwwroot")
        });
        builder.Logging.ClearProviders();
        var database = "GoldEx_MoltenGoldTest_" + Guid.NewGuid().ToString("N");
        builder.Configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:GoldEx"] = $"Server=(localdb)\\MSSQLLocalDB;Database={database};Integrated Security=True;TrustServerCertificate=True"
        });
        return builder.ConfigureServices();
    }

    private static async Task VerifyPhysicalDeskAsync(IServiceProvider services, StoreContext store, Guid storeId, GoldExDbContext db)
    {
            await db.Database.MigrateAsync();
            var unit = PriceUnit.Create("تومان", UnitType.TMN, true);
            var supplier = Customer.Create(CustomerType.RetailCustomer, "مسعود", "0010000001", null, null, null, null, new(storeId));
            var buyer = Customer.Create(CustomerType.RetailCustomer, "رضا", "0010000002", null, null, null, null, new(storeId));
            db.AddRange(unit, supplier, buyer);
            await db.SaveChangesAsync();
            await ActivatorUtilities.CreateInstance<LedgerAccountDbSeeder>(services).SeedAsync(new DbSeedContext());
            await ActivatorUtilities.CreateInstance<SettingsDbSeeder>(services).SeedAsync(new DbSeedContext());
            var setting = await db.Set<Setting>().SingleAsync();
            setting.SetMoltenGoldCommission(2m);
            var cashLedger = await db.Set<LedgerAccount>().SingleAsync(x => x.Title == SystemLedgerAccounts.InternalCashAccounts);
            var cash = FinancialAccount.CreateSystemAccount(null, null, FinancialAccountType.Cash, unit.Id, cashLedger.Id,
                cashAccount: CashAccount.Create("صندوق آزمایشی", CashAccountType.Internal), storeId: new(storeId));
            db.Add(cash);
            await db.SaveChangesAsync();

            var desk = services.GetRequiredService<IMoltenGoldTradeService>();
            Assert.That((await desk.GetSettingsAsync()).CommissionPercent, Is.EqualTo(2m));
            var purchase = Guid.NewGuid();
            await desk.CreateAsync(new(purchase, InvoiceType.Purchase, supplier.Id.Value, null,
                10, 750, MoltenGoldRateBasis.Gram, 100, 0, null, 0, null, 0, null));
            await desk.DeliverAsync(purchase, new(Guid.NewGuid(), 10, "123"));
            var supplierInvoice = await db.Set<Invoice>().SingleAsync();
            var stock = await db.Set<InventoryStock>().SingleAsync();
            var product = await db.Set<Product>().SingleAsync();
            Assert.That(product.MoltenGold!.AssayNumber, Is.EqualTo("123"));

            var sale = Guid.NewGuid();
            await desk.CreateAsync(new(sale, InvoiceType.Sell, buyer.Id.Value, stock.ProductId!.Value.Value,
                4, 750, MoltenGoldRateBasis.Gram, 100, null, null, 0, null, 0, null));
            Assert.That(await db.Set<Invoice>().CountAsync(), Is.EqualTo(1));
            var inventory = services.GetRequiredService<IInventoryStockRepository>();
            Assert.That(await inventory.GetReservedMoltenGoldAsync(stock.ProductId.Value, default), Is.EqualTo(4));
            Assert.That(await inventory.GetQuantityAsync(stock.ProductId.Value), Is.EqualTo(10));
            Assert.That((await desk.SearchStockAsync(product.Barcode)).Single().Available, Is.EqualTo(6m));

            var attemptedExit = InventoryStock.CreateProduct(stock.ProductId.Value, 7, WarehouseActionType.Out);
            await Assert.ThatAsync(() => inventory.CreateAsync(attemptedExit), Throws.TypeOf<ValidationException>());
            db.Entry(attemptedExit).State = EntityState.Detached;

            await desk.TransferAsync(sale, new(Guid.NewGuid(), Guid.Empty, supplier.Id.Value, supplierInvoice.Id.Value, 150, "حواله آزمایشی"));
            await desk.TransferAsync(sale, new(Guid.NewGuid(), Guid.Empty, supplier.Id.Value, null, 25, "حواله بابت مانده حساب"));
            await desk.PayAsync(sale, new(Guid.NewGuid(), cash.Id.Value, 25, "پیش‌دریافت آزمایشی"));
            var delivery = new DeliverMoltenGold(Guid.NewGuid(), 1.5m);
            await desk.DeliverAsync(sale, delivery);
            await desk.DeliverAsync(sale, delivery);
            Assert.That(await inventory.GetQuantityAsync(stock.ProductId.Value), Is.EqualTo(8.5m));
            Assert.That(await inventory.GetReservedMoltenGoldAsync(stock.ProductId.Value, default), Is.EqualTo(2.5m));
            var row = (await desk.GetListAsync(MoltenGoldTradeFilter.All, 0)).Single(x => x.Id == sale);
            Assert.That(row.Total, Is.EqualTo(408m));
            Assert.That(row.RemainingAmount, Is.EqualTo(208m));
            Assert.That(row.Events.Count(x => x.Kind == MoltenGoldTradeEventKind.Delivery), Is.EqualTo(1));
            Assert.That(row.Events.Single(x => x.Kind == MoltenGoldTradeEventKind.Delivery).InvoiceRemaining, Is.Zero);
            await desk.DeliverAsync(sale, new(Guid.NewGuid(), 2.5m));
            row = (await desk.GetListAsync(MoltenGoldTradeFilter.All, 0)).Single(x => x.Id == sale);
            var saleInvoice = row.Events.Single(x => x.InvoiceRemaining > 0).InvoiceId!.Value;
            await desk.TransferAsync(sale, new(Guid.NewGuid(), saleInvoice, supplier.Id.Value, supplierInvoice.Id.Value, 208, null));
            row = (await desk.GetListAsync(MoltenGoldTradeFilter.All, 0)).Single(x => x.Id == sale);
            Assert.That(row.RemainingAmount, Is.Zero);
            Assert.That(row.RemainingWeight, Is.Zero);
            var supplierRow = (await desk.GetListAsync(MoltenGoldTradeFilter.All, 0)).Single(x => x.Id == purchase);
            Assert.That(supplierRow.RemainingAmount, Is.EqualTo(642m));
            var journal = services.GetRequiredService<ITransactionRepository>();
            var supplierBalance = await journal.GetCustomerRemainingListAsync(supplier.Id, unit.Id);
            Assert.That(supplierBalance.Values.Single(), Is.EqualTo(-617m));
            Assert.That(await db.Set<Transaction>().SumAsync(x => x.TransactionType == TransactionType.Debit ? x.BaseCurrencyAmount : -x.BaseCurrencyAmount), Is.Zero);

            var invoiceService = services.GetRequiredService<IInvoiceService>();
            var normalItem = new InvoiceProductItemDto(null, 100, 0, 0, 300, null, null, null, unit.Id.Value,
                false, 1, 3, 0, WageType.Percent, new ProductRequestDto(null, "آب‌شدهٔ معمولی", null, 3,
                    0, WageType.Percent, ProductType.MoltenGold, 750, GoldUnitType.Gram, null, null, null, null,
                    new MoltenGoldDto("987", null, null)));
            var normalRequest = new InvoiceRequestDto(null, supplierInvoice.InvoiceNumber + 1, DateTime.Today, null,
                InvoiceType.Purchase, TradeScale.Retail, unit.Id.Value, null, null, 1, supplier.Id.Value,
                [normalItem], [], [], [], [], [], []);
            await invoiceService.CreateAsync(normalRequest);
            var normalInvoice = await db.Set<Invoice>().Include(x => x.ProductItems)
                .SingleAsync(x => x.InvoiceType == InvoiceType.Purchase && x.InvoiceNumber == normalRequest.InvoiceNumber);
            var newSale = Guid.NewGuid();
            await desk.CreateAsync(new(newSale, InvoiceType.Sell, buyer.Id.Value, stock.ProductId.Value.Value,
                1, 750, MoltenGoldRateBasis.Gram, 100, 0, null, 0, null, 0, null));
            await desk.TransferAsync(newSale, new(Guid.NewGuid(), Guid.Empty, supplier.Id.Value, normalInvoice.Id.Value, 30, null));
            var savedItem = normalInvoice.ProductItems.Single();
            await invoiceService.UpdateAsync(normalInvoice.Id.Value, normalRequest with
            {
                Id = normalInvoice.Id.Value,
                InvoiceProductItems = [normalItem with { Id = savedItem.Id.Value, Product = normalItem.Product with { Id = savedItem.ProductId.Value } }]
            });
            Assert.That(await db.Set<InvoicePayment>().Where(x => x.InvoiceId == normalInvoice.Id)
                .SumAsync(x => x.Amount), Is.EqualTo(30m));
            await Assert.ThatAsync(() => invoiceService.DeleteAsync(normalInvoice.Id.Value), Throws.TypeOf<ValidationException>());

            store.SetStore(Guid.NewGuid(), "other");
            Assert.That(await db.Set<MoltenGoldTrade>().CountAsync(), Is.Zero);
    }

    [TestCase(false)]
    [TestCase(true)]
    [Platform("Win")]
    public async Task PaperTrade_ExchangesCustomerBalances_WithoutStock_AndSettlesGoldAndMoney(bool baseIsRial)
    {
        await using var app = CreateTestApp();
        await using var scope = app.Services.CreateAsyncScope();
        var services = scope.ServiceProvider;
        var store = services.GetRequiredService<StoreContext>();
        var storeId = Guid.NewGuid();
        store.SetStore(storeId, "paper-test");
        var db = services.GetRequiredService<GoldExDbContext>();
        try
        {
            await db.Database.MigrateAsync();
            var money = PriceUnit.Create("تومان", UnitType.TMN, !baseIsRial);
            var gold = PriceUnit.Create("گرم طلای ۱۸", UnitType.Gold18K);
            var customer = Customer.Create(CustomerType.RetailCustomer, "مسعود", "0010000003", null, null, null, null, new(storeId));
            var supplier = Customer.Create(CustomerType.RetailCustomer, "محمود", "0010000004", null, null, null, null, new(storeId));
            var market = Price.Create(PriceCatalog.Geram18, PriceHistory.Create(250_000_000m, DateTime.Now, "0", "ریال"));
            db.AddRange(money, gold, customer, supplier, market);
            if (baseIsRial)
                db.Add(PriceUnit.Create("ریال", UnitType.IRR, true));
            await db.SaveChangesAsync();
            await ActivatorUtilities.CreateInstance<LedgerAccountDbSeeder>(services).SeedAsync(new DbSeedContext());
            var cashLedger = await db.Set<LedgerAccount>().SingleAsync(x => x.Title == SystemLedgerAccounts.InternalCashAccounts);
            var cash = FinancialAccount.CreateSystemAccount(null, null, FinancialAccountType.Cash, money.Id, cashLedger.Id,
                cashAccount: CashAccount.Create("صندوق آزمایشی", CashAccountType.Internal), storeId: new(storeId));
            db.Add(cash);
            await db.SaveChangesAsync();
            var desk = services.GetRequiredService<IMoltenGoldTradeService>();
            var journal = services.GetRequiredService<ITransactionRepository>();
            Assert.That((await desk.GetSettingsAsync()).DayRate, Is.EqualTo(25_000_000m));
            var sale = Guid.NewGuid();
            var purchase = Guid.NewGuid();
            await desk.CreateAsync(new(sale, InvoiceType.Sell, customer.Id.Value, null, 10, 750,
                MoltenGoldRateBasis.Gram, 25_000_000, 1, null, 0, null, 0, null, IsPaper: true));
            await desk.CreateAsync(new(purchase, InvoiceType.Purchase, supplier.Id.Value, null, 10, 750,
                MoltenGoldRateBasis.Gram, 24_000_000, 0, null, 0, null, 0, null, IsPaper: true));
            Assert.That(await db.Set<InventoryStock>().CountAsync(), Is.Zero);
            Assert.That(await db.Set<Invoice>().CountAsync(), Is.Zero);
            Assert.That(await db.Set<Product>().CountAsync(), Is.Zero);
            var moneyBalance = await journal.GetCustomerRemainingListAsync(customer.Id, money.Id);
            var goldBalance = await journal.GetCustomerRemainingListAsync(customer.Id, gold.Id);
            Assert.That(moneyBalance.Values.Single(), Is.EqualTo(252_500_000m));
            Assert.That(goldBalance.Values.Single(), Is.EqualTo(-10m));

            var settlement = new SettleMoltenGold(Guid.NewGuid(), purchase, 3);
            await desk.SettleGoldAsync(sale, settlement);
            await desk.SettleGoldAsync(sale, settlement);
            var row = (await desk.GetListAsync(MoltenGoldTradeFilter.All, 0)).Single(x => x.Id == sale);
            Assert.That(row.RemainingGoldWeight, Is.EqualTo(7m));
            Assert.That(row.RemainingAmount, Is.EqualTo(252_500_000m));
            await desk.PayAsync(sale, new(Guid.NewGuid(), cash.Id.Value, 50_000_000, null));
            await desk.TransferAsync(sale, new(Guid.NewGuid(), Guid.Empty, supplier.Id.Value, null, 100_000_000, null, purchase));
            var rows = await desk.GetListAsync(MoltenGoldTradeFilter.All, 0);
            Assert.That(rows.Single(x => x.Id == sale).RemainingAmount, Is.EqualTo(102_500_000m));
            Assert.That(rows.Single(x => x.Id == purchase).RemainingAmount, Is.EqualTo(140_000_000m));
            await desk.SettleGoldAsync(sale, new(Guid.NewGuid(), purchase, 7));
            Assert.That((await journal.GetCustomerRemainingListAsync(customer.Id, gold.Id)).Values.Single(), Is.Zero);
            Assert.That((await journal.GetCustomerRemainingListAsync(supplier.Id, gold.Id)).Values.Single(), Is.Zero);
            await desk.PayAsync(sale, new(Guid.NewGuid(), cash.Id.Value, 102_500_000, null));
            await desk.PayAsync(purchase, new(Guid.NewGuid(), cash.Id.Value, 140_000_000, null));
            Assert.That((await journal.GetCustomerRemainingListAsync(customer.Id, money.Id)).Values.Single(), Is.Zero);
            Assert.That((await journal.GetCustomerRemainingListAsync(supplier.Id, money.Id)).Values.Single(), Is.Zero);

            var cancelled = Guid.NewGuid();
            await desk.CreateAsync(new(cancelled, InvoiceType.Sell, customer.Id.Value, null, 2, 750,
                MoltenGoldRateBasis.Gram, 25_000_000, 1, null, 0, null, 0, null, IsPaper: true));
            await desk.CancelAsync(cancelled);
            await desk.CancelAsync(cancelled);
            Assert.That((await journal.GetCustomerRemainingListAsync(customer.Id, money.Id)).Values.Single(), Is.Zero);
            Assert.That((await journal.GetCustomerRemainingListAsync(customer.Id, gold.Id)).Values.Single(), Is.Zero);

            var physicalPurchase = Guid.NewGuid();
            await desk.CreateAsync(new(physicalPurchase, InvoiceType.Purchase, supplier.Id.Value, null, 1, 750,
                MoltenGoldRateBasis.Gram, 23_000_000, 0, null, 0, null, 0, null, IsPaper: true));
            await desk.DeliverAsync(physicalPurchase, new(Guid.NewGuid(), 1, "paper-123"));
            var product = await db.Set<Product>().SingleAsync();
            var physicalSale = Guid.NewGuid();
            await desk.CreateAsync(new(physicalSale, InvoiceType.Sell, customer.Id.Value, null, 1, 750,
                MoltenGoldRateBasis.Gram, 25_000_000, 1, null, 0, null, 0, null, IsPaper: true));
            await desk.DeliverAsync(physicalSale, new(Guid.NewGuid(), 1, ProductId: product.Id.Value));
            Assert.That((await journal.GetCustomerRemainingListAsync(customer.Id, gold.Id)).Values.Single(), Is.Zero);
            Assert.That((await journal.GetCustomerRemainingListAsync(supplier.Id, gold.Id)).Values.Single(), Is.Zero);
            Assert.That(await services.GetRequiredService<IInventoryStockRepository>().GetQuantityAsync(product.Id), Is.Zero);
            rows = await desk.GetListAsync(MoltenGoldTradeFilter.All, 0);
            Assert.That(rows.Single(x => x.Id == physicalSale).RemainingAmount, Is.EqualTo(25_250_000m));
            Assert.That(rows.Single(x => x.Id == physicalSale).RemainingWeight, Is.Zero);
            Assert.That(rows.Single(x => x.Id == physicalSale).Events.Single(x => x.Kind == MoltenGoldTradeEventKind.Delivery).InvoiceRemaining, Is.Zero);

            var fractionalSale = Guid.NewGuid();
            var fractionalPurchase = Guid.NewGuid();
            await desk.CreateAsync(new(fractionalSale, InvoiceType.Sell, customer.Id.Value, null, .007m, 749.123456m,
                MoltenGoldRateBasis.Gram, 12_345_678.123456789m, 2.3456m, null, 0, null, 0, null, IsPaper: true));
            await desk.CreateAsync(new(fractionalPurchase, InvoiceType.Purchase, supplier.Id.Value, null, .012m, 995.123456m,
                MoltenGoldRateBasis.Gram, 12_300_000.123456789m, 0, null, 0, null, 0, null, IsPaper: true));
            await desk.SettleGoldAsync(fractionalSale, new(Guid.NewGuid(), fractionalPurchase, .002m));
            rows = await desk.GetListAsync(MoltenGoldTradeFilter.All, 0);
            var fractionalRemaining = rows.Single(x => x.Id == fractionalSale).RemainingGoldWeight;
            Assert.That((await journal.GetCustomerRemainingListAsync(customer.Id, gold.Id)).Values.Single(), Is.EqualTo(-fractionalRemaining));
            await desk.SettleGoldAsync(fractionalSale, new(Guid.NewGuid(), fractionalPurchase, fractionalRemaining));
            Assert.That((await journal.GetCustomerRemainingListAsync(customer.Id, gold.Id)).Values.Single(), Is.Zero);
            var unbalanced = await db.Set<Transaction>().GroupBy(x => x.GroupId)
                .Select(x => x.Sum(e => e.TransactionType == TransactionType.Debit ? e.BaseCurrencyAmount : -e.BaseCurrencyAmount))
                .Where(x => x != 0).ToListAsync();
            Assert.That(unbalanced, Is.Empty);
            store.SetStore(Guid.NewGuid(), "other");
            Assert.That(await db.Set<MoltenGoldTrade>().CountAsync(), Is.Zero);
        }
        finally { await db.Database.EnsureDeletedAsync(); }
    }
}
