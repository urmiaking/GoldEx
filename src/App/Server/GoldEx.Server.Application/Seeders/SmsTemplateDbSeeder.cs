using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Sdk.Server.Infrastructure.Abstractions;
using GoldEx.Sdk.Server.Infrastructure.Common;
using GoldEx.Server.Application.Utilities;
using GoldEx.Server.Domain.SmsTemplateAggregate;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Server.Infrastructure.Specifications.SmsTemplates;
using GoldEx.Shared.Enums;
using Microsoft.Extensions.Logging;

namespace GoldEx.Server.Application.Seeders;

[ScopedService]
internal sealed class SmsTemplateDbSeeder(ISmsTemplateRepository repository, ILogger<SmsTemplateDbSeeder> logger) : IDbSeeder
{
    public int Order => 90;
    public async Task SeedAsync(DbSeedContext context, CancellationToken cancellationToken = default)
    {
        var exists = await repository.ExistsAsync(new SmsTemplatesDefaultSpecification(), cancellationToken);

        if (exists)
            return;

        var invoiceDueTemplate = SmsTemplate.Create(SmsTemplateSubject.DueInvoice,
            SmsTemplateBuilder.BuildForInvoice(),
            SmsParameterBuilder.BuildForInvoice());

        await repository.CreateAsync(invoiceDueTemplate, cancellationToken);
        logger.LogInformation($"{nameof(SmsTemplateDbSeeder)}: Seeded 1 sms template.");
    }
}