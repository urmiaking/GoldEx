namespace GoldEx.Sdk.Server.Application.Exceptions;

public class ForbiddenException(string message) : ApplicationOperationException(message)
{
    public ForbiddenException() : this("Forbidden")
    {
        
    }
}
