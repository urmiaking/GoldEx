using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.StoreAggregate;
using System;

namespace GoldEx.Server.Infrastructure.Specifications.Stores;

public class StoreUserByAccessSpecification : SpecificationBase<StoreUser>
{
    public StoreUserByAccessSpecification(Guid userId, StoreId storeId)
    {
        AddCriteria(x => x.UserId == userId && x.StoreId == storeId);
    }
}
