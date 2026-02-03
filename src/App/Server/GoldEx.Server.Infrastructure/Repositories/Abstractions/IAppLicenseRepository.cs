using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.AppLicenseAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IAppLicenseRepository : IRepository<AppLicense>,
    ICreateRepository<AppLicense>,
    IUpdateRepository<AppLicense>,
    IDeleteRepository<AppLicense>;