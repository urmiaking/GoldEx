using GoldEx.Sdk.Server.Infrastructure.Repositories;
using GoldEx.Server.Domain.SmsTemplateAggregate;

namespace GoldEx.Server.Infrastructure.Repositories.Abstractions;

public interface ISmsTemplateRepository : IRepository<SmsTemplate>,
    ICreateRepository<SmsTemplate>,
    IUpdateRepository<SmsTemplate>;