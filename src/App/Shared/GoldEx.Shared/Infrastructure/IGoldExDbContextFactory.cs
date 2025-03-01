using Microsoft.EntityFrameworkCore;

namespace GoldEx.Shared.Infrastructure;

public interface IGoldExDbContextFactory
{
    Task<DbContext> CreateDbContextAsync();
}