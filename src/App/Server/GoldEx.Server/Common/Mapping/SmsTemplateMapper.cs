using GoldEx.Server.Domain.SmsTemplateAggregate;
using GoldEx.Shared.DTOs.SmsTemplates;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class SmsTemplateMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SmsTemplate, SmsTemplateResponse>()
            .Map(dest => dest.Id, src => src.Id.Value);
    }
}