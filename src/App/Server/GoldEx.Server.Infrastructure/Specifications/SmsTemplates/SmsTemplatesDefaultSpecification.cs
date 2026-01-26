using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.SmsTemplateAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.SmsTemplates;

public class SmsTemplatesDefaultSpecification : SpecificationBase<SmsTemplate>
{
    public SmsTemplatesDefaultSpecification()
    {
        ApplyOrderBy(x => x.Subject);
    }
}