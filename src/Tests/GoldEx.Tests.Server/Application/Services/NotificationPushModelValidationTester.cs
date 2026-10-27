using GoldEx.Shared.DTOs.Notifications;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public sealed class NotificationPushModelValidationTester
{
    [Test]
    public void MvcValidatesRegistrationRecordsWithoutPropertyMetadataExceptions()
    {
        using var services = new ServiceCollection().AddLogging().AddMvcCore().AddDataAnnotations().Services.BuildServiceProvider();
        var controller = new ValidationController
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { RequestServices = services } }
        };
        Assert.That(controller.TryValidateModel(new RegisterPushRequest("https://fcm.googleapis.com/test", "curve", "auth")), Is.True);
        controller.ModelState.Clear();
        Assert.That(controller.TryValidateModel(new RegisterPushRequest("", "curve", new string('x', 65))), Is.False);
        Assert.That(controller.ModelState.ContainsKey("Endpoint"), Is.True);
        Assert.That(controller.ModelState.ContainsKey("Auth"), Is.True);
        controller.ModelState.Clear();
        Assert.That(controller.TryValidateModel(new RevokePushRequest("https://fcm.googleapis.com/test")), Is.True);
        controller.ModelState.Clear();
        Assert.That(controller.TryValidateModel(new RevokePushRequest("")), Is.False);
    }

    private sealed class ValidationController : Controller;
}
