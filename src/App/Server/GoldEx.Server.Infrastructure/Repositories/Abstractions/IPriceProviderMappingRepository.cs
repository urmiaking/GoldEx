using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PriceProviderMappingAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IPriceProviderMappingRepository : IRepository<PriceProviderMapping>,
    ICreateRepository<PriceProviderMapping>,
    IUpdateRepository<PriceProviderMapping>,
    IDeleteRepository<PriceProviderMapping>;