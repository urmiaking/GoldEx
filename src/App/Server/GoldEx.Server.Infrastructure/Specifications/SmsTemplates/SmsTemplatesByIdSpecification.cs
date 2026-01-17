using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.SmsTemplateAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.SmsTemplates;

public class SmsTemplatesByIdSpecification(SmsTemplateId id) : SpecificationBase<SmsTemplate>(x => x.Id == id);