namespace GoldEx.Shared.DTOs.SmsTemplates;

public record SmsTemplateRequest(Guid Id, string Body, bool IsActive);