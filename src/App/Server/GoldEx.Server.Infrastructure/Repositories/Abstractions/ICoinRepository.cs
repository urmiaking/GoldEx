using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.CoinAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ICoinRepository : IRepository<Coin>,
    ICreateRepository<Coin>,
    IUpdateRepository<Coin>,
    IDeleteRepository<Coin>;