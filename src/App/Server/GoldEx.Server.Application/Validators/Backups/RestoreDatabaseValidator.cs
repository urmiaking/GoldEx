using FluentValidation;
using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;

namespace GoldEx.Server.Application.Validators.Backups;

[ScopedService]
internal class RestoreDatabaseValidator : AbstractValidator<string>
{
    private readonly IBackupRepository _repository;
    public RestoreDatabaseValidator(IBackupRepository repository)
    {
        _repository = repository;

        RuleFor(x => x)
            .MustAsync(BeValidBackupAsync)
            .WithMessage("فایل پشتیبان معتبر نمی باشد");
    }

    private async Task<bool> BeValidBackupAsync(string backupPath, CancellationToken cancellationToken = default)
    {
        try
        {
            await _repository.ValidateBackupAsync(backupPath, cancellationToken);
            return true;
        }
        catch
        {
            return false;
        }
    }
}