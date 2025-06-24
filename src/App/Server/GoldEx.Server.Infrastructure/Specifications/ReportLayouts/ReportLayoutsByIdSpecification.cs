using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.ReportLayoutAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.ReportLayouts;

public class ReportLayoutsByIdSpecification : SpecificationBase<ReportLayout>
{
    public ReportLayoutsByIdSpecification(ReportLayoutId id)
    {
        AddCriteria(x => x.Id == id);
    }
}