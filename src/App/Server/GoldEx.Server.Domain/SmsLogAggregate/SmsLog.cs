using GoldEx.Sdk.Server.Domain.Entities;

namespace GoldEx.Server.Domain.SmsLogAggregate;

public readonly record struct SmsLogId(Guid Value);
public class SmsLog : EntityBase<SmsLogId>
{
    public static SmsLog Create(string message, string receiver, bool delivered)
    {
        return new SmsLog
        {
            Id = new SmsLogId(Guid.CreateVersion7()),
            Message = message,
            Receiver = receiver,
            Delivered = delivered
        };
    }

#pragma warning disable CS8618
    private SmsLog() { }
#pragma warning restore CS8618

    public string Message { get; private set; }
    public string Receiver { get; private set; }
    public bool Delivered { get; private set; }
}