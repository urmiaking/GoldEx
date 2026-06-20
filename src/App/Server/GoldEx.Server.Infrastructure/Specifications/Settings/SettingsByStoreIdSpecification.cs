using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.SettingAggregate;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.Settings;

public class SettingsByStoreIdSpecification : SpecificationBase<Setting>
{
    public SettingsByStoreIdSpecification(StoreId storeId)
    {
        AddCriteria(x => x.StoreId == storeId);
        AddInclude(x => x.BarcodePrintSettings!);
        AddInclude(x => x.BarcodePrintSettings!.PositionItems);
    }
}
