using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.ReportLayoutAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface IReportLayoutRepository : IRepository<ReportLayout>,
    ICreateRepository<ReportLayout>,
    IUpdateRepository<ReportLayout>,
    IDeleteRepository<ReportLayout>;