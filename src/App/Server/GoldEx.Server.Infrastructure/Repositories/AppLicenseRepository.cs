using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.AppLicenseAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal sealed class AppLicenseRepository(GoldExDbContext dbContext) : RepositoryBase<AppLicense>(dbContext), IAppLicenseRepository;