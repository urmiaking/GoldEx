using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PriceAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IPriceRepository : IRepository<Price>,
    ICreateRepository<Price>,
    IUpdateRepository<Price>;