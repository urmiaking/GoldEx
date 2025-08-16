using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Shared.DTOs.Settings;
using Mapster;

namespace GoldEx.Server.Common.Mapping;

public class SettingsMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Setting, GetSettingResponse>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.HasIcon, src => MapContext.Current.GetService<IWebHostEnvironment>().AppIconExists());
    }
}
