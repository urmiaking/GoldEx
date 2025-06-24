using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.ReportLayoutAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class ReportLayoutRepository(GoldExDbContext dbContext) : RepositoryBase<ReportLayout>(dbContext), IReportLayoutRepository;