using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Server.Infrastructure.Repositories.Abstractions;
using GoldEx.Shared.Settings;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace GoldEx.Server.Infrastructure.Repositories;

[ScopedService]
internal class BackupRepository(IConfiguration config, IOptions<BackupSettings> options) : IBackupRepository
{
    private readonly string _maintenanceConnection = config
        .GetConnectionString("GoldEx") ?? throw new InvalidOperationException();

    private readonly string _databaseName = options.Value.DbPrefix.Replace("_", "");

    public async Task ValidateBackupAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_maintenanceConnection);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();

        command.CommandText =
            "RESTORE FILELISTONLY FROM DISK = @BackupPath";

        command.Parameters.Add(
            new SqlParameter("@BackupPath", backupFilePath));

        await using var reader =
            await command.ExecuteReaderAsync(cancellationToken);

        if (!await reader.ReadAsync(cancellationToken))
            throw new InvalidOperationException();

        var logicalName = reader["LogicalName"].ToString();

        if (logicalName != _databaseName)
            throw new InvalidOperationException();
    }

    public async Task RestoreAsync(string backupFilePath, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqlConnection(_maintenanceConnection);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandTimeout = 0;

        command.CommandText = $@"
            USE master
            ALTER DATABASE [{_databaseName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;

            RESTORE DATABASE [{_databaseName}]
            FROM DISK = @BackupPath
            WITH REPLACE, RECOVERY;

            ALTER DATABASE [{_databaseName}] SET MULTI_USER;
        ";

        command.Parameters.Add(
            new SqlParameter("@BackupPath", backupFilePath));

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}