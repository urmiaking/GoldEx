using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.SmsTemplateAggregate;
using GoldEx.Shared.Enums;

namespace GoldEx.Server.Infrastructure.Specifications.SmsTemplates;

public class SmsTemplatesBySubjectSpecification : SpecificationBase<SmsTemplate>
{
    public SmsTemplatesBySubjectSpecification(SmsTemplateSubject subject)
    {
        AddCriteria(x => x.Subject == subject);
    }
}