using System.Security.Cryptography;
using System.Text.Json;
using GoldEx.Sdk.Common;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Application.Services;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.NotificationAggregate;
using GoldEx.Server.Domain.PriceUnitAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Enums;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public sealed class NotificationPushSqlTester
{
    [Test]
    [Platform("Win")]
    public async Task SqlLeaseIsExclusiveAndRecoversAfterExpiry_AndTenantAndSettlementFiltersWork()
    {
        var databaseName = "GoldEx_NotificationTest_" + Guid.NewGuid().ToString("N");
        var options = new DbContextOptionsBuilder<GoldExDbContext>().UseSqlServer(
            $"Server=(localdb)\\mssqllocaldb;Database={databaseName};Trusted_Connection=True;TrustServerCertificate=True").Options;
        var store = new StoreContext(); store.SetStore(Guid.NewGuid(), "test");
        await using var db = new GoldExDbContext(options, store);
        try
        {
            await db.Database.EnsureCreatedAsync();
            var repo = new NotificationPushRepository(db);
            var user = new AppUser("test", "test"); var role = new AppRole(BuiltinRoles.Owners);
            db.Add(user); db.Add(role); db.Add(new AppUserRole { UserId = user.Id, RoleId = role.Id });
            db.Add(StoreUser.Create(new(store.StoreId!.Value), user.Id));
            var subscription = NotificationPushSubscription.Create(new(store.StoreId.Value), user.Id, "test", "https://fcm.googleapis.com/test", "key", "auth", DateTime.UtcNow);
            var delivery = NotificationDelivery.Create(subscription, null, DateTime.UtcNow.AddSeconds(-1));
            db.Add(subscription); db.Add(delivery); await db.SaveChangesAsync();
            Assert.That(await repo.CanNotifyAsync(user.Id, new(store.StoreId.Value), default), Is.True);
            Assert.That(await repo.CanNotifyAsync(user.Id, new(Guid.NewGuid()), default), Is.False);
            Assert.That(await repo.ClaimAsync(delivery.Id, DateTime.UtcNow, default), Is.True);
            Assert.That(await repo.ClaimAsync(delivery.Id, DateTime.UtcNow, default), Is.False);
            Assert.That(await repo.ClaimAsync(delivery.Id, DateTime.UtcNow.AddMinutes(3), default), Is.True);
            var claimed = await repo.Deliveries.AsNoTracking().SingleAsync();
            Assert.That(claimed.Attempts, Is.EqualTo(2));
            var originalStore = store.StoreId.Value; store.SetStore(Guid.NewGuid(), "other");
            Assert.That(await repo.Deliveries.CountAsync(), Is.Zero);
            Assert.That(await repo.ClaimAsync(delivery.Id, DateTime.UtcNow.AddMinutes(6), default), Is.False);
            store.SetStore(originalStore, "test");
            var unit = PriceUnit.Create("test", null);
            var customer = Customer.Create(CustomerType.RetailCustomer, "test", "", null, null, null, null, new(originalStore));
            db.Add(unit); db.Add(customer);
            Invoice MakeInvoice(long number, int offset, decimal amount)
            {
                var invoice = Invoice.Create(number, null, null, InvoiceType.Sell, default, customer.Id, unit.Id, unit.Id, null,
                    DateOnly.FromDateTime(DateTime.Now), DateOnly.FromDateTime(DateTime.Now).AddDays(offset), new(originalStore));
                if (amount > 0) invoice.AddCurrencyItem(null, unit.Id, null, 1, amount, 0, 0);
                return invoice;
            }
            var overdue = MakeInvoice(1, -1, 100);
            var empty = MakeInvoice(2, -1, 0);
            var future = MakeInvoice(3, 1, 100);
            db.AddRange(overdue, empty, future); await db.SaveChangesAsync();
            var repositoryType = typeof(GoldExDbContext).Assembly.GetType("GoldEx.Server.Infrastructure.Repositories.InvoiceRepository")!;
            var invoices = (IInvoiceRepository)Activator.CreateInstance(repositoryType, db)!;
            var reminders = await invoices.GetOverdueInvoicesAsync();
            Assert.That(reminders.Select(x => x.Id), Is.EqualTo(new[] { overdue.Id }));
            Assert.That(await repo.IsOverdueAsync(empty.Id, default), Is.False);
            Assert.That(await repo.IsOverdueAsync(future.Id, default), Is.False);
            await repo.RevokeUserAsync(user.Id, default);
            Assert.That((await repo.Subscriptions.SingleAsync()).Enabled, Is.False);
        }
        finally { await db.Database.EnsureDeletedAsync(); }
    }
    [Test]
    public void ConfiguredVapidKeysFormAMatchingP256Pair()
    {
        var root = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (root is not null && !File.Exists(Path.Combine(root.FullName, "AGENTS.md"))) root = root.Parent;
        Assert.That(root, Is.Not.Null);
        using var settings = JsonDocument.Parse(File.ReadAllText(Path.Combine(root!.FullName, "src/App/Server/GoldEx.Server/appsettings.json")));
        var push = settings.RootElement.GetProperty("Notifications").GetProperty("Push");
        byte[] Decode(string key) => Convert.FromBase64String(key.Replace('-', '+').Replace('_', '/').PadRight((key.Length + 3) / 4 * 4, '='));
        var pub = Decode(push.GetProperty("PublicKey").GetString()!);
        var priv = Decode(push.GetProperty("PrivateKey").GetString()!);
        using var signer = ECDsa.Create(new ECParameters { Curve = ECCurve.NamedCurves.nistP256, D = priv,
            Q = new ECPoint { X = pub[1..33], Y = pub[33..65] } });
        using var verifier = ECDsa.Create(new ECParameters { Curve = ECCurve.NamedCurves.nistP256,
            Q = new ECPoint { X = pub[1..33], Y = pub[33..65] } });
        var message = RandomNumberGenerator.GetBytes(32);
        Assert.That(verifier.VerifyData(message, signer.SignData(message, HashAlgorithmName.SHA256), HashAlgorithmName.SHA256), Is.True);
    }
}
