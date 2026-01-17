using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.SmsLogAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ISmsLogRepository : IRepository<SmsLog>, ICreateRepository<SmsLog>;