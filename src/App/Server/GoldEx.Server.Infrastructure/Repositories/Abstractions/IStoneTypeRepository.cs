using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.StoneTypeAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IStoneTypeRepository : IRepository<StoneType>,
    ICreateRepository<StoneType>,
    IUpdateRepository<StoneType>,
    IDeleteRepository<StoneType>;