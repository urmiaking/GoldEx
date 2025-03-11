using FluentValidation;
using GoldEx.Shared.Application.Services.Abstractions;
using GoldEx.Shared.Application.Validators.Settings;
using GoldEx.Shared.Domain.Aggregates.SettingsAggregate;
using GoldEx.Shared.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Shared.Application.Services;

public class SettingsService<T>(
    ISettingsRepository<T> repository,
    SettingsValidator<T> validator)
    : ISettingsService<T> where T : SettingsBase
{
    public async Task CreateAsync(T settings, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(settings, cancellationToken);
        await repository.CreateAsync(settings, cancellationToken);
    }

    public async Task UpdateAsync(T settings, CancellationToken cancellationToken = default)
    {
        await validator.ValidateAndThrowAsync(settings, cancellationToken);
        await repository.UpdateAsync(settings, cancellationToken);
    }

    public Task<T?> GetAsync(SettingsId id, CancellationToken cancellationToken = default) 
        => repository.GetAsync(id, cancellationToken);

    public Task<T?> GetAsync(CancellationToken cancellationToken = default) 
        => repository.GetAllAsync(cancellationToken);

    public Task<T?> GetUpdateAsync(DateTime checkpointDate, CancellationToken cancellationToken = default) 
        => repository.GetUpdateAsync(checkpointDate, cancellationToken);
}