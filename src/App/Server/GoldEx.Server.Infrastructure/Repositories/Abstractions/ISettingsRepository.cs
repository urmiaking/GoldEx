using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.SettingsAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ISettingsRepository : IRepository<Setting>,
    ICreateRepository<Setting>,
    IUpdateRepository<Setting>;