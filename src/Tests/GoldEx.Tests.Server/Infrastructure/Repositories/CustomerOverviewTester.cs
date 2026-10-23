using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.DependencyInjections.Extensions;
using GoldEx.Server.Domain.CustomerAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.DTOs.Customers;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Tests.Server.Infrastructure.Repositories;

[TestFixture]
public class CustomerOverviewTester
{
    private GoldExDbContext _dbContext = null!;
    private ServiceProvider _provider = null!;
    private ICustomerRepository _repository = null!;
    private DbContextOptions<GoldExDbContext> _options = null!;
    private Guid _storeId;

    private sealed class TestStoreContext(Guid storeId) : IStoreContext
    {
        public Guid? StoreId { get; set; } = storeId;
        public string? StoreSlug { get; set; } = "test";
        public string? CustomDomain { get; set; }
        public bool IsCustomDomain => false;
    }

    [SetUp]
    public void SetUp()
    {
        _storeId = Guid.NewGuid();
        _options = new DbContextOptionsBuilder<GoldExDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new GoldExDbContext(_options, new TestStoreContext(_storeId));
        var services = new ServiceCollection();
        services.AddSingleton(_dbContext);
        services.DiscoverServices(typeof(GoldExDbContext).Assembly);
        _provider = services.BuildServiceProvider();
        _repository = _provider.GetRequiredService<ICustomerRepository>();
    }

    [TearDown]
    public async Task TearDown()
    {
        await _provider.DisposeAsync();
        await _dbContext.DisposeAsync();
    }

    [Test]
    public async Task Overview_CountsAllThousandCustomers_IndependentlyOfRecentPage()
    {
        AddCustomers(CustomerType.RetailCustomer, 850);
        AddCustomers(CustomerType.Retailer, 25);
        AddCustomers(CustomerType.Wholesaler, 50);
        AddCustomers(CustomerType.MeltedGoldDealer, 25);
        AddCustomers(CustomerType.Workshop, 25);
        AddCustomers(CustomerType.AssayingLab, 25);
        await _dbContext.SaveChangesAsync();

        var recent = await _repository.GetListAsync(new CustomerFilter(null, null, null, null),
            new RequestFilter(0, 5, null, null, GoldEx.Sdk.Common.Definitions.SortDirection.Descending));
        var overview = await _repository.GetOverviewAsync();

        Assert.Multiple(() =>
        {
            Assert.That(recent.Data, Has.Count.EqualTo(5));
            Assert.That(overview.TotalCustomersCount, Is.EqualTo(1000));
            Assert.That(overview.RetailCustomersCount, Is.EqualTo(875));
            Assert.That(overview.WholesaleCustomersCount, Is.EqualTo(125));
            Assert.That(overview.RetailCustomersCount + overview.WholesaleCustomersCount,
                Is.EqualTo(overview.TotalCustomersCount));
        });
    }

    [Test]
    public async Task Overview_ExcludesOtherStoresAndHistoricalDefaultStore()
    {
        AddCustomers(CustomerType.RetailCustomer, 3);
        AddCustomers(CustomerType.Workshop, 2);
        AddCustomers(CustomerType.RetailCustomer, 300, Guid.NewGuid());
        await _dbContext.SaveChangesAsync();
        await using (var defaultContext = new GoldExDbContext(_options, new TestStoreContext(Guid.Empty)))
        {
            for (var i = 0; i < 50; i++)
                defaultContext.Add(Customer.Create(CustomerType.Wholesaler, $"Default customer {i}",
                    Guid.NewGuid().ToString(), null, null, null, null));
            await defaultContext.SaveChangesAsync();
        }

        var overview = await _repository.GetOverviewAsync();

        Assert.Multiple(() =>
        {
            Assert.That(overview.TotalCustomersCount, Is.EqualTo(5));
            Assert.That(overview.RetailCustomersCount, Is.EqualTo(3));
            Assert.That(overview.WholesaleCustomersCount, Is.EqualTo(2));
        });
    }

    [Test]
    public async Task Overview_EmptyStore_ReturnsZeroCounts()
    {
        var overview = await _repository.GetOverviewAsync();

        Assert.Multiple(() =>
        {
            Assert.That(overview.TotalCustomersCount, Is.Zero);
            Assert.That(overview.RetailCustomersCount, Is.Zero);
            Assert.That(overview.WholesaleCustomersCount, Is.Zero);
        });
    }

    private void AddCustomers(CustomerType type, int count, Guid? storeId = null)
    {
        for (var i = 0; i < count; i++)
            _dbContext.Add(Customer.Create(type, $"Customer {type} {i}", Guid.NewGuid().ToString(),
                null, null, null, null, new StoreId(storeId ?? _storeId)));
    }
}
