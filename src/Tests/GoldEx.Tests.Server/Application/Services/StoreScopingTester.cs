using System;
using System.Linq;
using System.Threading.Tasks;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Shared.Enums;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public class StoreScopingTester
{
    private class TestStoreContext : IStoreContext
    {
        public Guid? StoreId { get; set; }
        public string? StoreSlug { get; set; }
    }

    [Test]
    public async Task QueryFilters_And_AutoAssignment_ScopeToActiveStore()
    {
        // 1. Arrange: Setup DbContext with a test store context
        var storeContext = new TestStoreContext();
        var options = new DbContextOptionsBuilder<GoldExDbContext>()
            .UseInMemoryDatabase(databaseName: "GoldEx_IntegrationTest")
            .Options;

        var storeId1 = Guid.CreateVersion7();
        var storeId2 = Guid.CreateVersion7();

        // 2. Act & Assert: Seed data for Store 1
        storeContext.StoreId = storeId1;
        storeContext.StoreSlug = "store1";

        using (var dbContext = new GoldExDbContext(options, storeContext))
        {
            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var product1 = Product.Create("Product Store 1", 10.5m, 1.5m, ProductType.Gold, 750m, GoldUnitType.Gram, WageType.Percent, null, null, null).SetBarcode("barcode1");
            dbContext.Set<Product>().Add(product1);
            await dbContext.SaveChangesAsync();

            // StoreId should be automatically assigned from storeContext
            Assert.That(product1.StoreId.Value, Is.EqualTo(storeId1));
        }

        // Seed data for Store 2
        storeContext.StoreId = storeId2;
        storeContext.StoreSlug = "store2";

        using (var dbContext = new GoldExDbContext(options, storeContext))
        {
            var product2 = Product.Create("Product Store 2", 20.0m, 2.0m, ProductType.Gold, 750m, GoldUnitType.Gram, WageType.Percent, null, null, null).SetBarcode("barcode2");
            dbContext.Set<Product>().Add(product2);
            await dbContext.SaveChangesAsync();

            Assert.That(product2.StoreId.Value, Is.EqualTo(storeId2));
        }

        // Verify isolation: Query under Store 1 context
        storeContext.StoreId = storeId1;
        storeContext.StoreSlug = "store1";
        using (var dbContext = new GoldExDbContext(options, storeContext))
        {
            var products = await dbContext.Set<Product>().ToListAsync();
            Assert.That(products.Count, Is.EqualTo(1));
            Assert.That(products[0].Name, Is.EqualTo("Product Store 1"));
        }

        // Verify isolation: Query under Store 2 context
        storeContext.StoreId = storeId2;
        storeContext.StoreSlug = "store2";
        using (var dbContext = new GoldExDbContext(options, storeContext))
        {
            var products = await dbContext.Set<Product>().ToListAsync();
            Assert.That(products.Count, Is.EqualTo(1));
            Assert.That(products[0].Name, Is.EqualTo("Product Store 2"));
        }
    }
}
