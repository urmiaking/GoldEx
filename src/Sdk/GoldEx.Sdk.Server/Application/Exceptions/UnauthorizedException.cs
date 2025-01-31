namespace GoldEx.Sdk.Server.Application.Exceptions;

public class UnauthorizedException(string message) : ApplicationOperationException(message)
{
    public UnauthorizedException() : this("Unauthorized.")
    {
        
    }
}
