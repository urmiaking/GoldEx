using System.ComponentModel.Design;
using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Infrastructure.Repositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Tests.Server.Infrastructure.Repositories;

[TestFixture]
public class ProductRepositoryTester
{
    private GoldExDbContext _dbContext;
    private ProductRepository<Product> _productRepository;

    [SetUp]
    public void Setup()
    {
        // Use an in-memory database for testing
        var options = new DbContextOptionsBuilder<GoldExDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new GoldExDbContext(options, new Mediator(new ServiceContainer()));
        _dbContext.Database.EnsureDeleted(); // Ensure a clean database for each test
        _dbContext.Database.EnsureCreated();

        _productRepository = new ProductRepository<Product>(new GoldExDbContextFactory(null));

        // Seed some test data
        SeedData();
    }

    [TearDown]
    public void TearDown()
    {
        _productRepository.Dispose();
        _dbContext.Dispose();
    }

    private void SeedData()
    {
        var appUser = new AppUser("test", "test");

        var products = new List<Product>
        {
            new("Product A", "123", 1, null, ProductType.MoltenGold, null, CaratType.Eighteen, appUser.Id),
            new("Product B", "456", 1, null, ProductType.UsedGold, null, CaratType.Eighteen, appUser.Id),
            new("Product C", "789", 1, 12, ProductType.Gold, WageType.Percent, CaratType.Eighteen, appUser.Id),
            new("Product D", "101", 1, 16, ProductType.Jewelry, WageType.Dollar, CaratType.Eighteen, appUser.Id)
        };

        foreach (var product in products)
        {
            _productRepository.CreateAsync(product).Wait();
            Task.Delay(100); // Delay to ensure CreatedAt is different for each product
        }
    }

    [Test]
    public async Task GetListAsync_NoFilter_ReturnsDefaultPagedList()
    {
        var filter = new RequestFilter();
        var result = await _productRepository.GetListAsync(filter);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Total, Is.EqualTo(4));
        Assert.That(result.Data.Count, Is.EqualTo(4));
        Assert.That(result.Data[0].Name, Is.EqualTo("Product D")); // Default sorting by CreatedAt descending
    }

    [Test]
    public async Task GetListAsync_WithSearchFilter_ReturnsFilteredList()
    {
        var filter = new RequestFilter(Search: "Product B");
        var result = await _productRepository.GetListAsync(filter);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Total, Is.EqualTo(1));
        Assert.That(result.Data.Count, Is.EqualTo(1));
        Assert.That(result.Data[0].Name, Is.EqualTo("Product B"));
    }

    [Test]
    public async Task GetListAsync_WithSkipAndTake_ReturnsPagedData()
    {
        var filter = new RequestFilter(Skip: 1, Take: 2);
        var result = await _productRepository.GetListAsync(filter);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Total, Is.EqualTo(4));
        Assert.That(result.Data.Count, Is.EqualTo(2));
        Assert.That(result.Data[0].Name, Is.EqualTo("Product C"));
        Assert.That(result.Data[1].Name, Is.EqualTo("Product B"));
    }

    [Test]
    public async Task GetListAsync_WithAscendingSort_ReturnsSortedList()
    {
        var filter = new RequestFilter(SortLabel: "Name", SortDirection: SortDirection.Ascending);
        var result = await _productRepository.GetListAsync(filter);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Total, Is.EqualTo(4));
        Assert.That(result.Data.Count, Is.EqualTo(4));
        Assert.That(result.Data[0].Name, Is.EqualTo("Product A"));
        Assert.That(result.Data[1].Name, Is.EqualTo("Product B"));
        Assert.That(result.Data[2].Name, Is.EqualTo("Product C"));
        Assert.That(result.Data[3].Name, Is.EqualTo("Product D"));
    }

    [Test]
    public async Task GetListAsync_WithDescendingSort_ReturnsSortedList()
    {
        var filter = new RequestFilter(SortLabel: "Name", SortDirection: SortDirection.Descending);
        var result = await _productRepository.GetListAsync(filter);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Total, Is.EqualTo(4));
        Assert.That(result.Data.Count, Is.EqualTo(4));
        Assert.That(result.Data[0].Name, Is.EqualTo("Product D"));
        Assert.That(result.Data[1].Name, Is.EqualTo("Product C"));
        Assert.That(result.Data[2].Name, Is.EqualTo("Product B"));
        Assert.That(result.Data[3].Name, Is.EqualTo("Product A"));
    }

    [Test]
    public async Task GetListAsync_WithInvalidSortLabel_ReturnsDefaultSortedList()
    {
        var filter = new RequestFilter(SortLabel: "InvalidProperty", SortDirection: SortDirection.Ascending);
        var result = await _productRepository.GetListAsync(filter);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Total, Is.EqualTo(4));
        Assert.That(result.Data.Count, Is.EqualTo(4));
        Assert.That(result.Data[0].Name, Is.EqualTo("Product A")); // Defaults to CreatedAt
    }
}