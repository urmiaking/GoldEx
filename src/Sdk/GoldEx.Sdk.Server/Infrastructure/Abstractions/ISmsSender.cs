namespace GoldEx.Sdk.Server.Infrastructure.Abstractions;

public interface ISmsSender
{
    /// <summary>
    /// Send a sms message.
    /// </summary>
    /// <param name="phoneNumber">Recipient phone number.</param>
    /// <param name="message">Message text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns true in case of successful send.</returns>
    Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the remaining credit of the account
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Remaining credit</returns>
    Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default);
}
