using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.PersonalAccessTokenAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IPersonalAccessTokenRepository : IRepository<PersonalAccessToken>,
    ICreateRepository<PersonalAccessToken>,
    IUpdateRepository<PersonalAccessToken>,
    IDeleteRepository<PersonalAccessToken>;
