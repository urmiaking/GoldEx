using System.Security.Cryptography;
using GoldEx.Sdk.Server.Application.Abstractions;
using GoldEx.Server.Application.Services;
using GoldEx.Server.Application.Services.Abstractions;
using GoldEx.Server.Domain.InvoiceAggregate;
using GoldEx.Server.Domain.NotificationAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Notifications;
using Mapster;
using MapsterMapper;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public sealed class NotificationPushTester
{
    private GoldExDbContext _db = null!;
    private StoreContext _store = null!;
    private Guid _userId;
    private Mock<IUserContext> _user = null!;
    private Mock<IWebPushSender> _sender = null!;
    private TestRepository _repository = null!;
    private NotificationPushService _service = null!;
    private RegisterPushRequest _request = null!;
    [SetUp]
    public void Setup()
    {
        _store = new StoreContext(); _store.SetStore(Guid.NewGuid(), "test"); _userId = Guid.NewGuid();
        _db = new GoldExDbContext(new DbContextOptionsBuilder<GoldExDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options, _store);
        _repository = new TestRepository(_db);
        _user = new Mock<IUserContext>(); _user.Setup(x => x.GetUserId()).Returns(() => _userId);
        _sender = new Mock<IWebPushSender>(); _sender.SetupGet(x => x.IsConfigured).Returns(true);
        _sender.SetupGet(x => x.PublicKey).Returns("public");
        _sender.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<WebPushMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPushResult("Accepted", 201));
        _service = new NotificationPushService(_repository, _sender.Object, _user.Object);
        using var curve = ECDiffieHellman.Create(ECCurve.NamedCurves.nistP256);
        var key = curve.ExportParameters(false);
        _request = new("https://fcm.googleapis.com/test-endpoint", Encode([4, ..key.Q.X!, ..key.Q.Y!]), Encode(RandomNumberGenerator.GetBytes(16)));
    }
    [TearDown] public void Cleanup() => _db.Dispose();
    private static string Encode(byte[] bytes) => Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    [TestCase("http://fcm.googleapis.com/send")]
    [TestCase("https://localhost/push")]
    [TestCase("https://127.0.0.1/push")]
    [TestCase("https://fcm.googleapis.com.evil.example/send")]
    [TestCase("https://fcm.googleapis.com:8443/send")]
    [TestCase("https://user:pass@fcm.googleapis.com/send")]
    [TestCase("https://jmt17.google.com.evil.example/fcm/send/token")]
    [TestCase("https://jmt17.google.com/log")]
    [TestCase("https://jmt17.google.com:8443/fcm/send/token")]
    public void UnsafeEndpointsAreRejected(string endpoint) => Assert.That(PushSubscriptionValidator.Valid(_request with { Endpoint = endpoint }), Is.False);
    [Test]
    public void ValidatesCurveAndAuthenticationKeys()
    {
        Assert.That(PushSubscriptionValidator.Valid(_request), Is.True);
        Assert.That(PushSubscriptionValidator.Valid(_request with { P256dh = Encode(new byte[65]) }), Is.False);
        Assert.That(PushSubscriptionValidator.Valid(_request with { Auth = "broken" }), Is.False);
    }
    [Test]
    public void ChromiumLegacyGoogleSubscriptionsAreAcceptedWithoutRewritingTheEndpoint()
    {
        var request = _request with { Endpoint = "https://jmt17.google.com/fcm/send/browser-token" };
        Assert.That(PushSubscriptionValidator.Valid(request), Is.True);
        Assert.That(PushSubscriptionValidator.GetErrors(request), Is.Empty);
        Assert.That(PushSubscriptionValidator.GetErrors(request with { Endpoint = "https://evil.example/" })[0].PropertyName, Is.EqualTo("Endpoint"));
    }
    [Test]
    public async Task RegistrationIsIdempotentAndMovesTheBrowserToTheCurrentUserAndStore()
    {
        await _service.RegisterAsync(_request); await _service.RegisterAsync(_request);
        Assert.That(await _db.Set<NotificationPushSubscription>().CountAsync(), Is.EqualTo(1));
        var old = await _db.Set<NotificationPushSubscription>().SingleAsync();
        _userId = Guid.NewGuid(); _store.SetStore(Guid.NewGuid(), "second");
        await _service.RegisterAsync(_request);
        var current = await _db.Set<NotificationPushSubscription>().IgnoreQueryFilters().SingleAsync();
        Assert.That(current.UserId, Is.EqualTo(_userId)); Assert.That(current.StoreId.Value, Is.EqualTo(_store.StoreId));
        Assert.That(current.Id, Is.Not.EqualTo(old.Id));
    }
    [Test]
    public async Task AnotherUserCannotRevokeYourDevice()
    {
        await _service.RegisterAsync(_request); _userId = Guid.NewGuid();
        await _service.RevokeAsync(new(_request.Endpoint));
        Assert.That((await _db.Set<NotificationPushSubscription>().SingleAsync()).Enabled, Is.True);
    }
    [TestCase("Accepted", "Accepted", true)]
    [TestCase("Gone", "Gone", false)]
    [TestCase("Retry", "Pending", true)]
    [TestCase("Rejected", "Rejected", true)]
    public async Task DeliveryPersistsOutcomeAndDisablesExpiredDevices(string outcome, string expected, bool enabled)
    {
        _sender.Setup(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<WebPushMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WebPushResult(outcome, outcome == "Gone" ? 410 : 503));
        await _service.RegisterAsync(_request);
        var result = await _service.SendTestAsync(new(_request.Endpoint));
        var delivery = await _db.Set<NotificationDelivery>().SingleAsync();
        Assert.That(result.Status, Is.EqualTo(expected)); Assert.That(delivery.Attempts, Is.EqualTo(1));
        Assert.That((await _db.Set<NotificationPushSubscription>().SingleAsync()).Enabled, Is.EqualTo(enabled));
        if (outcome == "Retry") Assert.That(delivery.NextAttemptAtUtc, Is.GreaterThan(DateTime.UtcNow));
    }
    [Test]
    public async Task DoesNotSendTwiceAndDoesNotSendForAnotherStore()
    {
        var subscription = NotificationPushSubscription.Create(new(_store.StoreId!.Value), _userId, "hash", _request.Endpoint, _request.P256dh, _request.Auth, DateTime.UtcNow.AddHours(-1));
        _db.Add(subscription);
        _db.Add(Notification.CreateInvoiceNotification("due", "message", [], new(Guid.NewGuid()), subscription.StoreId));
        _db.Add(Notification.CreateInvoiceNotification("other store", "private", [], new(Guid.NewGuid()), new(Guid.NewGuid())));
        await _db.SaveChangesAsync();
        await _service.ProcessAsync(default); await _service.ProcessAsync(default);
        _sender.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<WebPushMessage>(), It.IsAny<CancellationToken>()), Times.Once);
        Assert.That(await _db.Set<NotificationDelivery>().CountAsync(), Is.EqualTo(1));
    }
    [Test]
    public async Task ReadingAndDismissingArePerUserAndNeverDeleteTheInvoiceReminder()
    {
        var notification = Notification.CreateInvoiceNotification("due", "message", [], new(Guid.NewGuid()), new(_store.StoreId!.Value));
        _db.Add(notification); await _db.SaveChangesAsync();
        var mapping = new TypeAdapterConfig();
        mapping.NewConfig<Notification, GetNotificationResponse>().Map(x => x.Id, x => x.Id.Value)
            .Map(x => x.InvoiceId, x => x.InvoiceId.HasValue ? x.InvoiceId.Value.Value : (Guid?)null);
        var service = new NotificationService(Mock.Of<INotificationRepository>(), _repository, _user.Object, new Mapper(mapping));
        await service.MarkAsReadAsync(notification.Id.Value);
        Assert.That((await service.GetListAsync(null)).Single().IsRead, Is.True);
        var firstUser = _userId; _userId = Guid.NewGuid();
        Assert.That((await service.GetListAsync(null)).Single().IsRead, Is.False);
        _userId = firstUser; await service.DeleteAsync(notification.Id.Value);
        Assert.That(await service.GetListAsync(null), Is.Empty);
        Assert.That(await _db.Set<Notification>().CountAsync(), Is.EqualTo(1));
    }
    [Test]
    public async Task AccessRevocationAndSettledInvoicesCancelQueuedPushes()
    {
        var subscription = NotificationPushSubscription.Create(new(_store.StoreId!.Value), _userId, "hash", _request.Endpoint, _request.P256dh, _request.Auth, DateTime.UtcNow.AddHours(-1));
        var notification = Notification.CreateInvoiceNotification("due", "message", [], new(Guid.NewGuid()), subscription.StoreId);
        _db.Add(subscription); _db.Add(notification); await _db.SaveChangesAsync();
        _repository.Overdue = false;
        await _service.ProcessAsync(default);
        Assert.That((await _db.Set<NotificationDelivery>().SingleAsync()).Status, Is.EqualTo("Canceled"));
        _sender.Verify(x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<WebPushMessage>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // InMemory cannot execute relational bulk updates. This adapter provides a lease for processor tests;
    // the production atomic SQL lease is verified separately against LocalDB.
    private sealed class TestRepository(GoldExDbContext db) : INotificationPushRepository
    {
        private readonly NotificationPushRepository _real = new(db);
        public StoreId CurrentStoreId => _real.CurrentStoreId;
        public bool Overdue { get; set; } = true;
        public IQueryable<NotificationPushSubscription> Subscriptions => _real.Subscriptions;
        public IQueryable<NotificationDelivery> Deliveries => _real.Deliveries;
        public IQueryable<NotificationUserState> UserStates => _real.UserStates;
        public IQueryable<Notification> Notifications => _real.Notifications;
        public IQueryable<Store> ActiveStores => _real.ActiveStores;
        public Task<NotificationPushSubscription?> FindBrowserSubscriptionAsync(string hash, CancellationToken ct) => _real.FindBrowserSubscriptionAsync(hash, ct);
        public Task<bool> CanNotifyAsync(Guid user, StoreId store, CancellationToken ct) => Task.FromResult(true);
        public Task<bool> IsOverdueAsync(InvoiceId id, CancellationToken ct) => Task.FromResult(Overdue);
        public void Add<T>(T entity) where T : class => _real.Add(entity);
        public void Remove<T>(T entity) where T : class => _real.Remove(entity);
        public Task SaveAsync(CancellationToken ct) => _real.SaveAsync(ct);
        public Task RevokeUserAsync(Guid id, CancellationToken ct) => _real.RevokeUserAsync(id, ct);
        public async Task<bool> ClaimAsync(Guid id, DateTime now, CancellationToken ct)
        {
            var item = await db.Set<NotificationDelivery>().SingleAsync(x => x.Id == id, ct);
            if (item.NextAttemptAtUtc > now || item.Status is not ("Pending" or "Sending")) return false;
            db.Entry(item).Property(x => x.Attempts).CurrentValue++;
            db.Entry(item).Property(x => x.Status).CurrentValue = "Sending";
            db.Entry(item).Property(x => x.NextAttemptAtUtc).CurrentValue = now.AddMinutes(2);
            await db.SaveChangesAsync(ct); db.Entry(item).State = EntityState.Detached;
            return true;
        }
    }
}
