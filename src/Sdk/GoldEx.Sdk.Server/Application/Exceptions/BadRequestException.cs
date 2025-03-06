using GoldEx.Sdk.Common.Exceptions;

namespace GoldEx.Sdk.Server.Application.Exceptions;

public class BadRequestException(string message) : ApplicationOperationException(message)
{
    public BadRequestException() : this("Bad request.")
    {

    }
}
