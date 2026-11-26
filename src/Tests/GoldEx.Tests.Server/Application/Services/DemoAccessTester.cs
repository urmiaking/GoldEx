using System.Security.Claims;
using GoldEx.Server.Application.Services;
using GoldEx.Server.Common.Middlewares;
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
    [Test]
    public void LocalCaptcha_IsRasterAndSessionBound()
    {
        using var service = new DemoCaptchaService();
        var challenge = service.Issue("session-one", "register");
        Assert.That(challenge.Id.Length, Is.EqualTo(48));
        Assert.That(challenge.Image, Does.StartWith("data:image/png;base64,"));
        var image = Convert.FromBase64String(challenge.Image.Split(',')[1]);
        Assert.That(image.Take(8), Is.EqualTo(new byte[] {137,80,78,71,13,10,26,10}));
        Assert.That(service.Verify(challenge.Id, "123456", "session-two", "register"), Is.False);
        Assert.That(service.Verify(challenge.Id, "123456", "session-one", "login"), Is.False);
        Assert.That(service.Verify(challenge.Id, "not-an-answer", "session-one", "register"), Is.False);
        Assert.That(service.Verify(challenge.Id, "123456", "session-one", "register"), Is.False);
    }
    [TestCase("POST", "/hubs/prices/negotiate", true)]
    [TestCase("GET", "/hubs/prices", true)]
    [TestCase("POST", "/hubs/prices", true)]
    [TestCase("DELETE", "/hubs/prices", true)]
    [TestCase("POST", "/hubs/smart-trays/negotiate", false)]
    [TestCase("POST", "/api/invoices", false)]
    [TestCase("POST", "/hubs/prices/anything-else", false)]
    public void PriceTransport_IsRestrictedToPublicPriceHub(string method, string path, bool expected)
    {
        var http = new DefaultHttpContext();
        http.Request.Method = method;
        http.Request.Path = path;
        Assert.That(DemoAccessMiddleware.IsPriceTransport(http), Is.EqualTo(expected));
    }

    [TestCase("GET", "/api/Account/Logout", true)]
    [TestCase("GET", "/Account/Login", true)]
    [TestCase("POST", "/Account/Login", true)]
    [TestCase("POST", "/api/Account/Logout", false)]
    [TestCase("POST", "/api/Account/Login", false)]
    [TestCase("POST", "/Account/Demo", false)]
    [TestCase("POST", "/api/invoices", false)]
    public void SessionExit_DoesNotPermitBusinessWrites(string method, string path, bool expected)
    {
        var http = new DefaultHttpContext();
        http.Request.Method = method;
        http.Request.Path = path;
        Assert.That(DemoAccessMiddleware.IsDemoSessionExit(http), Is.EqualTo(expected));
    }
}