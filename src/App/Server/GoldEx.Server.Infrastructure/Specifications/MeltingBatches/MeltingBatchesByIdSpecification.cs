using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.MeltingBatchAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.MeltingBatches;

public class MeltingBatchesByIdSpecification : SpecificationBase<MeltingBatch>
{
    public MeltingBatchesByIdSpecification(MeltingBatchId id)
    {
        AddCriteria(x => x.Id == id);
    }
}