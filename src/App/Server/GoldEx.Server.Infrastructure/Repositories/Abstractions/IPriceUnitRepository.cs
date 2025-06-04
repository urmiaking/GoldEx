using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PriceUnitAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IPriceUnitRepository : IRepository<PriceUnit>,
    ICreateRepository<PriceUnit>,
    IUpdateRepository<PriceUnit>;