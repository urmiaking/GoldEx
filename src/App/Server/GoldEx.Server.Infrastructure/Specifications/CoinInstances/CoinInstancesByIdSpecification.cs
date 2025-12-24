using GoldEx.Sdk.Server.Infrastructure.Specifications;
using GoldEx.Server.Domain.CoinInstanceAggregate;

namespace GoldEx.Server.Infrastructure.Specifications.CoinInstances;

public class CoinInstancesByIdSpecification(CoinInstanceId id) : SpecificationBase<CoinInstance>(x => x.Id == id);