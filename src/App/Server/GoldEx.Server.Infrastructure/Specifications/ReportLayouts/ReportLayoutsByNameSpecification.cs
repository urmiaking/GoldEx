using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ReportLayoutAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.ReportLayouts;

public class ReportLayoutsByNameSpecification : SpecificationBase<ReportLayout>
{
    public ReportLayoutsByNameSpecification(string name)
    {
        AddCriteria(x => x.Name == name);
    }
}