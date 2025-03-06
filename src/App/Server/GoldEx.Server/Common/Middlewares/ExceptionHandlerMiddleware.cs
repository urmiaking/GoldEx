using System.Net;
using System.Text.Json;
using FluentValidation;
using GoldEx.Sdk.Common.Exceptions;
using GoldEx.Sdk.Server.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GoldEx.Server.Common.Middlewares;

public class ExceptionHandlerMiddleware(ILogger<ExceptionHandlerMiddleware> logger, RequestDelegate next)
    : ExceptionHandlerMiddlewareBase(logger, next)
{
    public override (HttpStatusCode code, string message) GetResponse(Exception exception)
    {
        HttpStatusCode code;
        var message = exception.Message;
        switch (exception)
        {
            case NotFoundException
                or FileNotFoundException:
                code = HttpStatusCode.NotFound;
                break;

            case UnauthorizedException
                or UnauthorizedAccessException:
                code = HttpStatusCode.Unauthorized;
                break;

            case ForbiddenException:
                code = HttpStatusCode.Forbidden;
                break;

            case BadRequestException:
                code = HttpStatusCode.BadRequest;
                break;

            case ValidationException validationException:
                code = HttpStatusCode.BadRequest;
                message = GetMessage(validationException);
                break;

            default:
                code = HttpStatusCode.InternalServerError;
                break;
        }
        return (code, message);
    }

    private string GetMessage(ValidationException exception)
    {
        var errors = new Dictionary<string, string[]>();

        foreach (var errorGroup in exception.Errors.GroupBy(x => x.PropertyName))
        {
            var errorMessages = errorGroup.Select(x => x.ErrorMessage).ToArray();
            errors.Add(errorGroup.Key, errorMessages);
        }

        var problemDetails = new ValidationProblemDetails(errors);
        return JsonSerializer.Serialize(problemDetails);
    }
}