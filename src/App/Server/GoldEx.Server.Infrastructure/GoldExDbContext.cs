using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Sdk.Server.Infrastructure;
using GoldEx.Server.Infrastructure.Configurations;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Server.Infrastructure;

public class GoldExDbContext(
    DbContextOptions<GoldExDbContext> options)
    : GoldExDbContextBase<AppUser, AppRole, Guid, AppUserClaim, AppUserRole, AppUserLogin, AppRoleClaim, AppUserToken>(
        options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(CustomerConfiguration).Assembly);

        base.OnModelCreating(builder);
    }
}