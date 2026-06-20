using GoldEx.Sdk.Server.Domain.Entities;
using GoldEx.Server.Domain.Common;
using GoldEx.Server.Domain.StoreAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Domain.SmsTemplateAggregate;

public readonly record struct SmsTemplateId(Guid Value);
public class SmsTemplate : EntityBase<SmsTemplateId>, IStoreFiltered
{
    public StoreId StoreId { get; private set; }

    public static SmsTemplate Create(SmsTemplateSubject subject, string body, string parameters, StoreId storeId = default)
    {
        return new SmsTemplate
        {
            Id = new SmsTemplateId(Guid.CreateVersion7()),
            Subject = subject,
            Body = body,
            Parameters = parameters,
            IsActive = true,
            StoreId = storeId
        };
    }

#pragma warning disable CS8618 
    private SmsTemplate() { }
#pragma warning restore CS8618

    public SmsTemplateSubject Subject { get; private set; }
    public string Body { get; private set; }
    public string Parameters { get; private set; }
    public bool IsActive { get; private set; }

    public void SetBody(string  body) => Body = body;
    public void SetStatus(bool isActive) => IsActive = isActive;
    public void SetParameters(string parameters) => Parameters = parameters;
}