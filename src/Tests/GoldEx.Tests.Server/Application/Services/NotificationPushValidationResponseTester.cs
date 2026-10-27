using System.Net;
using System.Text.Json;
using FluentValidation;
using GoldEx.Server.Common.Middlewares;
using Microsoft.Extensions.Logging.Abstractions;

namespace GoldEx.Tests.Server.Application.Services;

[TestFixture]
public sealed class NotificationPushValidationResponseTester
{
    [Test]
    public void MessageOnlyValidationExceptionsRetainTheirMessageInTheClientErrorContract()
    {
        var middleware = new ExceptionHandlerMiddleware(NullLogger<ExceptionHandlerMiddleware>.Instance, _ => Task.CompletedTask);
        var response = middleware.GetResponse(new ValidationException("اطلاعات دستگاه معتبر نیست."));
        using var body = JsonDocument.Parse(response.message);
        Assert.That(response.code, Is.EqualTo(HttpStatusCode.BadRequest));
        Assert.That(body.RootElement.GetProperty("errors").GetProperty("")[0].GetString(), Is.EqualTo("اطلاعات دستگاه معتبر نیست."));
    }
}
