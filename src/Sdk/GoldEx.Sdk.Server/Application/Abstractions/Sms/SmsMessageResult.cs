namespace GoldEx.Sdk.Server.Application.Abstractions.Sms;

public class SmsMessageResult(Status status, List<RecipientStatus> recipients)
{
    public Status Status { get; } = status;
    public List<RecipientStatus> Recipients { get; } = recipients;
}

public class RecipientStatus(string number, string? refId, bool isSent)
{
    public string Number { get; } = number;
    public string? RefId { get; } = refId;
    public bool IsSent { get; } = isSent;
}