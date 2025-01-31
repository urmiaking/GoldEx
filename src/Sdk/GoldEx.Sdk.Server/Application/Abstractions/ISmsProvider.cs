using GoldEx.Sdk.Common.Definitions;
using GoldEx.Sdk.Server.Application.Abstractions.Sms;

namespace GoldEx.Sdk.Server.Application.Abstractions;

public interface ISmsProvider
{
    /// <summary>
    /// Send a message to multiple recipients
    /// </summary>
    /// <param name="recipients">Phone numbers of the recipients</param>
    /// <param name="message">Message text</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of result for each message</returns>
    Task<List<SmsMessageResult>> SendAsync(IEnumerable<string> recipients, string message, CancellationToken cancellationToken = default);

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
