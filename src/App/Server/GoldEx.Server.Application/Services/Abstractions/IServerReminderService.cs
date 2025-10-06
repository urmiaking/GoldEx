namespace GoldEx.Server.Application.Services.Abstractions;

public interface IServerReminderService
{
    Task SendReminderAsync(Guid invoiceId, CancellationToken cancellationToken = default);
}