using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Shared.Settings;
using IPE.SmsIrClient;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoldEx.Server.Infrastructure.Services;

[ScopedService]
public class SmsSender(IOptions<SmsSettings> options, ILogger<SmsSender> logger) : ISmsSender
{
    private readonly SmsSettings _smsSettings = options.Value;

    public async Task<bool> SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
    {
        try
        {
//#if DEBUG
//            logger.LogInformation($"Sms sent to {phoneNumber} with this message : {message}");
//            return true;
//#endif

            var now = DateTime.Now;
            var start = new DateTime(now.Year, now.Month, now.Day, 08, 0, 0);
            var end = start.AddHours(14); // 08:00 of the next day

            if (now <= start || now > end)
            {
                logger.LogInformation("SMS sending is restricted between 22:00 and 08:00.");
                return false;
            }

            var smsIrClient = new SmsIr(_smsSettings.ApiKey);

            long.TryParse(_smsSettings.SenderPhoneNumber, out var lineNumber);

            var result = await smsIrClient.BulkSendAsync(lineNumber, message, [phoneNumber]);

            return result.Status == 1;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error sending sms");
            return false;
        }
    }

    public async Task<decimal> GetBalanceAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var smsIrClient = new SmsIr(_smsSettings.ApiKey);

            var result = await smsIrClient.GetCreditAsync();

            return result.Data;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Error getting credit info");
            throw;
        }
    }
}