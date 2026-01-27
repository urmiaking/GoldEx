namespace GoldEx.Shared.DTOs.Backups;

public record RestoreDatabaseRequest(Stream BackupStream, string FileName);