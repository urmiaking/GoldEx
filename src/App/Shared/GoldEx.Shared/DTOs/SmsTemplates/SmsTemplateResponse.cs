using GoldEx.Shared.Enums;

namespace GoldEx.Shared.DTOs.SmsTemplates;

public record SmsTemplateResponse(Guid Id, SmsTemplateSubject Subject, string Body, string Parameters, bool IsActive);
