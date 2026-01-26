using GoldEx.Server.Domain.SmsLogAggregate;
using GoldEx.Shared.DTOs.SmsLogs;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

internal class SmsLogMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<SmsLog, SmsLogResponse>();
    }
}