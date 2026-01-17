using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.SmsTemplateAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class SmsTemplateRepository(GoldExDbContext dbContext) : RepositoryBase<SmsTemplate>(dbContext), ISmsTemplateRepository;