namespace GoldEx.Sdk.Common.Exceptions;

public class NotFoundException(string message) : ApplicationOperationException(message)
{
    public NotFoundException() : this("Not found.")
    {
        
    }
}