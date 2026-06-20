using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.StoreAggregate;
using System;

namespace GoldEx.Server.Infrastructure.Specifications.Stores;

public class StoreUserByUserIdSpecification : SpecificationBase<StoreUser>
{
    public StoreUserByUserIdSpecification(Guid userId)
    {
        AddCriteria(x => x.UserId == userId);
    }
}
