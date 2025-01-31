using GoldEx.Sdk.Common.Definitions;

namespace GoldEx.Sdk.Server.Application.Abstractions;

public interface ISmsService
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
    /// Send a message to multiple recipients
    /// </summary>
    /// <param name="phoneNumbers">Phone numbers of the recipients</param>
    /// <param name="message">Message text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Returns true in case of successful send.</returns>
    Task<bool> SendAsync(IEnumerable<string> phoneNumbers, string message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the delivery status of a message.
    /// </summary>
    /// <param name="refId">Message reference id</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Delivery status</returns>
    Task<DeliveryStatus> GetDeliveryAsync(string refId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns the remaining credit of the account
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Remaining credit</returns>
    Task<double> GetBalanceAsync(CancellationToken cancellationToken = default);
}
