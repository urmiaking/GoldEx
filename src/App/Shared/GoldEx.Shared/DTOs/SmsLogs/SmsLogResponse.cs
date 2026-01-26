namespace GoldEx.Shared.DTOs.SmsLogs;

public record SmsLogResponse(string Receiver, string Message, bool Delivered, DateTime CreatedAt);