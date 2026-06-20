using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.SmsTemplateAggregate;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.SmsTemplates;

public class SmsTemplatesByStoreIdSpecification : SpecificationBase<SmsTemplate>
{
    public SmsTemplatesByStoreIdSpecification(StoreId storeId)
    {
        AddCriteria(x => x.StoreId == storeId);
    }
}
