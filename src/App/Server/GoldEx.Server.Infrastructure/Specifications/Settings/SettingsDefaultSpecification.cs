using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.SettingAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Settings;

public class SettingsDefaultSpecification : SpecificationBase<Setting>
{
    public SettingsDefaultSpecification()
    {
        ApplyOrderBy(x => x.CreatedAt);
    }
}