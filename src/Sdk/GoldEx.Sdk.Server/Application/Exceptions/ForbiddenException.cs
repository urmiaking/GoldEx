using GoldEx.Sdk.Common.Exceptions;

namespace GoldEx.Sdk.Server.Application.Exceptions;

public class ForbiddenException(string message) : ApplicationOperationException(message)
{
    public ForbiddenException() : this("Forbidden")
    {
        
    }
}
