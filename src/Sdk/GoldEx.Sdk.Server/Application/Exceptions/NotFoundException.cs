namespace GoldEx.Sdk.Server.Application.Exceptions;

public class NotFoundException(string message) : ApplicationOperationException(message)
{
    public NotFoundException() : this("Not found.")
    {
        
    }
}