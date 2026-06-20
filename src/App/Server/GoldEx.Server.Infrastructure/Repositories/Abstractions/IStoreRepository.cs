using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IStoreRepository : IRepository<Store>, ICreateRepository<Store>, IUpdateRepository<Store>, IDeleteRepository<Store>;