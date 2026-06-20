using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.StoreAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IStoreUserRepository : IRepository<StoreUser>, ICreateRepository<StoreUser>, IUpdateRepository<StoreUser>, IDeleteRepository<StoreUser>;
