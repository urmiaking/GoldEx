using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.InvoicePaymentAggregate;
using GoldEx.Server.Domain.LedgerAccountAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.TransactionAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Constants;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Tests.Server.Infrastructure.Repositories;

[TestFixture]
public class DashboardOverviewTester
{
    private static readonly DateOnly Today = new(2026, 11, 16);

    [Test]
    public async Task Overview_SeparatesUnitsAndPeriods_AndMatchesInvoiceBalances()
    {
        var options = new DbContextOptionsBuilder<GoldExDbContext>()
            .UseInMemoryDatabase($"Dashboard-{Guid.NewGuid():N}").Options;
        await VerifyOverviewAsync(options);
    }

    [Test]
    public async Task Overview_ExecutesOnSqlServer()
    {
        var server = Environment.GetEnvironmentVariable("GOLDEX_DASHBOARD_TEST_SQL_SERVER");
        if (string.IsNullOrWhiteSpace(server))
            Assert.Ignore("Set GOLDEX_DASHBOARD_TEST_SQL_SERVER to run against a disposable local SQL Server database.");

        var connection = new SqlConnectionStringBuilder
        {
            DataSource = server,
            InitialCatalog = $"GoldEx_Dashboard_Test_{Guid.NewGuid():N}",
            IntegratedSecurity = true,
            TrustServerCertificate = true
        };
        var options = new DbContextOptionsBuilder<GoldExDbContext>().UseSqlServer(connection.ConnectionString).Options;
        await using var db = new GoldExDbContext(options);
        try
        {
            await db.Database.EnsureCreatedAsync();
            await VerifyOverviewAsync(options);
        }
        finally { await db.Database.EnsureDeletedAsync(); }
    }

    [Test]
    public async Task EmptyStore_DoesNotInventUnits_AndRejectsUnboundedPeriods()
    {
        var options = new DbContextOptionsBuilder<GoldExDbContext>()
            .UseInMemoryDatabase($"Dashboard-{Guid.NewGuid():N}").Options;
        await using var db = new GoldExDbContext(options, new TestStoreContext { StoreId = Guid.NewGuid() });
        var repository = CreateRepository(db);
        var result = await repository.GetOverviewAsync(Today, 90);

        Assert.Multiple(() =>
        {
            Assert.That(result.FromDate, Is.EqualTo(Today.AddDays(-89)));
            Assert.That(result.Units, Is.Empty);
            Assert.That(result.DueInvoices, Is.Empty);
            Assert.That(result.Settlements, Is.Empty);
            Assert.That(result.ActiveCustomerCount, Is.Zero);
        });
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => repository.GetOverviewAsync(Today, 365));
    }

    private static async Task VerifyOverviewAsync(DbContextOptions<GoldExDbContext> options)
    {
        var activeStore = Guid.NewGuid();
        var context = new TestStoreContext { StoreId = activeStore };
        await using var db = new GoldExDbContext(options, context);
        var toman = PriceUnit.Create("تومان", UnitType.TMN);
        var gold = PriceUnit.Create("طلای ۱۸", UnitType.Gold18K);
        var customer = Customer.Create(CustomerType.RetailCustomer, "مشتری آزمایشی", "", null, null, null, null);
        var sell = AddInvoice(1, InvoiceType.Sell, toman, Today.AddDays(-6), Today.AddDays(-2), 200m);
        sell.SetPriceUnit(toman);
        sell.SetDiscounts([InvoiceDiscount.Create(10m, 2m, toman.Id)])
            .SetExtraCosts([InvoiceExtraCost.Create(5m, null, toman.Id)]);
        sell.AddUsedProduct(null, "طلای دست دوم", 1m, 10m, null, 750m, 0m, 1, false,
            ProductType.UsedGold, GoldUnitType.Gram, null);
        AddPayment(sell, Today.AddDays(-1), PaymentSide.Receive, 70m, 2m);
        AddPayment(sell, Today, PaymentSide.Pay, 5m);

        var buy = AddInvoice(2, InvoiceType.Purchase, gold, Today, Today, 6.125m);
        buy.SetPriceUnit(gold);
        buy.AddUsedProduct(null, "طلای دست دوم", 1m, 0.125m, null, 750m, 0m, 1, false,
            ProductType.UsedGold, GoldUnitType.Gram, null);
        AddPayment(buy, Today, PaymentSide.Pay, 1m, type: PaymentType.MoltenGoldInventory);
        AddPayment(buy, Today, PaymentSide.Receive, 0.125m, type: PaymentType.MoltenGoldInventory);

        var previous = AddInvoice(3, InvoiceType.Sell, toman, Today.AddDays(-7), null, 55m);
        _ = AddInvoice(4, InvoiceType.Sell, toman, Today.AddDays(-14), null, 999m);
        var settled = AddInvoice(5, InvoiceType.Sell, toman, Today.AddDays(-2), Today.AddDays(-1), 20m);
        AddPayment(settled, Today, PaymentSide.Receive, 20m);
        _ = AddInvoice(6, InvoiceType.Purchase, gold, Today.AddDays(1), Today.AddDays(1), 999m);
        var old = AddInvoice(7, InvoiceType.Purchase, toman, Today.AddDays(-120), Today.AddDays(-40), 7m);
        AddPayment(old, Today.AddDays(-8), PaymentSide.Pay, 2m);

        var sameName = Customer.Create(CustomerType.RetailCustomer, customer.FullName, "222", null, null, null, null);
        db.AddRange(gold, toman, customer, sameName);
        var receivable = LedgerAccount.CreateSystemAccount(SystemLedgerAccounts.AccountsReceivable, LedgerAccountType.Asset);
        db.Add(receivable);
        AddReceivable(customer, toman, 20m);
        AddReceivable(sameName, toman, 40m);
        AddReceivable(customer, gold, 2.125m);
        await db.SaveChangesAsync();

        // Seed a second tenant with the same invoice number and a much larger balance.
        db.ChangeTracker.Clear();
        context.StoreId = Guid.NewGuid();
        var otherCustomer = Customer.Create(CustomerType.RetailCustomer, "مشتری فروشگاه دیگر", "", null, null, null, null);
        db.Add(otherCustomer);
        var other = AddInvoice(1, InvoiceType.Sell, toman, Today, Today.AddDays(-1), 999m, otherCustomer);
        AddPayment(other, Today, PaymentSide.Receive, 1m);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        context.StoreId = activeStore;

        var repository = CreateRepository(db);
        var result = await repository.GetOverviewAsync(Today, 7);
        var balances = await repository.GetCustomerBalancesSummaryAsync();
        var transactionRepository = (ITransactionRepository)Activator.CreateInstance(
            typeof(GoldExDbContext).Assembly.GetType("GoldEx.Server.Infrastructure.Repositories.TransactionRepository")!, db)!;
        var ranked = await transactionRepository.GetTopCustomersAsync(TransactionType.Debit, toman.Id.Value);
        var monetary = result.Units.Single(u => u.PriceUnitId == toman.Id.Value);
        var weight = result.Units.Single(u => u.PriceUnitId == gold.Id.Value);
        var saleRemaining = sell.TotalUnpaidAmount;
        var purchaseRemaining = buy.TotalUnpaidAmount;
        Assert.Multiple(() =>
        {
            Assert.That(result.FromDate, Is.EqualTo(Today.AddDays(-6)));
            Assert.That(result.SellCount, Is.EqualTo(2));
            Assert.That(result.PurchaseCount, Is.EqualTo(1));
            Assert.That(result.ActiveCustomerCount, Is.EqualTo(1));
            Assert.That(monetary.Points, Has.Count.EqualTo(7));
            Assert.That(monetary.Points[1].SellAmount, Is.Zero);
            Assert.That(monetary.SellAmount, Is.EqualTo(sell.TotalAmountWithDiscountsAndExtraCosts + 20m));
            Assert.That(monetary.PreviousSellAmount, Is.EqualTo(previous.TotalAmountWithDiscountsAndExtraCosts));
            Assert.That(weight.PurchaseAmount, Is.EqualTo(buy.TotalAmountWithDiscountsAndExtraCosts));
            Assert.That(weight.SellAmount, Is.Zero);
            Assert.That(result.OverdueCount, Is.EqualTo(2));
            Assert.That(result.DueTodayCount, Is.EqualTo(1));
            Assert.That(result.DueInvoices.Select(x => x.Number), Is.EqualTo(new long[] { 7, 1, 2 }));
            Assert.That(result.DueInvoices[1].Remaining, Is.EqualTo(saleRemaining));
            Assert.That(result.DueInvoices[2].Remaining, Is.EqualTo(purchaseRemaining));
            Assert.That(result.Settlements, Has.Count.EqualTo(1));
            Assert.That(result.Settlements.Single().Received, Is.EqualTo(90m));
            Assert.That(result.Settlements.Single().Paid, Is.EqualTo(5m));
            Assert.That(balances.Receivables.Single(x => x.PriceUnitId == toman.Id.Value).Count, Is.EqualTo(2));
            Assert.That(balances.Receivables.Single(x => x.PriceUnitId == gold.Id.Value).Amount, Is.EqualTo(2.125m));
            Assert.That(ranked.Select(x => x.Amount), Is.EqualTo(new[] { 40m, 20m }));
        });

        Invoice AddInvoice(long number, InvoiceType type, PriceUnit unit, DateOnly date, DateOnly? due, decimal amount,
            Customer? account = null)
        {
            var invoice = Invoice.Create(number, null, null, type, TradeScale.Retail,
                (account ?? customer).Id, unit.Id, unit.Id, null, date, due);
            invoice.AddCurrencyItem(null, unit.Id, null, 1m, amount, 0m, 0m);
            db.Add(invoice);
            return invoice;
        }

        void AddPayment(Invoice invoice, DateOnly date, PaymentSide side, decimal amount, decimal? exchangeRate = null,
            PaymentType type = PaymentType.InternalCash)
        {
            var payment = InvoicePayment.Create(date.ToDateTime(new TimeOnly(16, 0)), type, side, amount, exchangeRate, null,
                invoice.Id, invoice.PriceUnitId, null, null, null, null, null);
            invoice.SetPayments([.. invoice.InvoicePayments ?? [], payment]);
            db.Add(payment);
        }

        void AddReceivable(Customer account, PriceUnit unit, decimal amount)
        {
            var ledger = LedgerAccount.CreateCustomerAccount($"{account.Id.Value:N}/{unit.Id.Value:N}", account.Id, unit.Id,
                LedgerAccountType.Asset, receivable.Id);
            var counter = LedgerAccount.CreateSystemAccount($"Sales {ledger.Id.Value:N}", LedgerAccountType.Revenue);
            var group = Guid.NewGuid();
            var postingDate = Today.ToDateTime(TimeOnly.MinValue);
            db.AddRange(ledger, counter,
                Transaction.CreateForManualEntry("Test sales", amount, null, group, TransactionType.Debit,
                    ledger.Id, unit.Id, null, null, postingDate),
                Transaction.CreateForManualEntry("Test sales", amount, null, group, TransactionType.Credit,
                    counter.Id, unit.Id, null, null, postingDate));
        }
    }

    private static IDashboardRepository CreateRepository(GoldExDbContext db) => (IDashboardRepository)Activator.CreateInstance(
        typeof(GoldExDbContext).Assembly.GetType("GoldEx.Server.Infrastructure.Repositories.DashboardRepository")!, db)!;

    private sealed class TestStoreContext : IStoreContext
    {
        public Guid? StoreId { get; set; }
        public string? StoreSlug => "dashboard-test";
        public string? CustomDomain => null;
        public bool IsCustomDomain => false;
    }
}
