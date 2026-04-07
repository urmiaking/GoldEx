namespace GoldEx.Sdk.Server.Application.Abstractions;

public interface IUserContext
{
    Guid? GetUserId();
    bool IsInRole(string role);
    bool IsAuthenticated();
}