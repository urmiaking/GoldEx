using System.Security.Claims;
using GoldEx.Server.Application.Services;
using GoldEx.Server.Domain.ProductAggregate;
using GoldEx.Server.Infrastructure;
using GoldEx.Shared.Constants;
using GoldEx.Shared.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public class DemoAccessTester
{
    [TestCase("۰۹۱۲۳۴۵۶۷۸۹", "09123456789")]
    [TestCase("+98 912 345 6789", "09123456789")]
    [TestCase("00989123456789", "09123456789")]
    [TestCase("0912", null)]
    [TestCase("abc09123456789", null)]
    [TestCase("02112345678", null)]
    public void NormalizeMobile_AcceptsIranianMobileOnly(string input, string? expected)
        => Assert.That(DemoAccessService.NormalizeMobile(input), Is.EqualTo(expected));

    [Test]
    public async Task DemoCannotPersistChanges_RegularUserCan()
    {
        var options = new DbContextOptionsBuilder<GoldExDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var http = new HttpContextAccessor { HttpContext = new DefaultHttpContext() };
        try
        {
            using var db = new GoldExDbContext(options, httpContextAccessor: http);
            await db.Database.EnsureCreatedAsync();
            var item = Product.Create("Sample", 10.5m, 1.5m, ProductType.Gold, 750m, GoldUnitType.Gram, WageType.Percent, null, null, null).SetBarcode("demo-test");
            db.Set<Product>().Add(item);

            http.HttpContext!.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Role, DemoAccess.Role)], "test"));
            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await db.SaveChangesAsync());
            Assert.Throws<UnauthorizedAccessException>(() => db.SaveChanges(false));

            http.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim(ClaimTypes.Role, "Owners")], "test"));
            await db.SaveChangesAsync();
            Assert.That(await db.Set<Product>().CountAsync(), Is.EqualTo(1));
        }
        finally { http.HttpContext = null; }
    }

    [Test]
    public void MissingRole_DemoMarkerStillIdentifiesRestrictedAccount()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(DemoAccess.ExpiryClaim, "invalid")], "test"));
        Assert.That(DemoAccess.IsDemo(user), Is.True);
        Assert.That(DemoAccess.GetExpiry(user), Is.Null);
    }
}
