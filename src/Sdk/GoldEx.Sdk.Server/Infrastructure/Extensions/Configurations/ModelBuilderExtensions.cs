using GoldEx.Sdk.Server.Domain.Entities.Identity;
using Microsoft.EntityFrameworkCore;

namespace GoldEx.Sdk.Server.Infrastructure.Extensions.Configurations;

internal static class ModelBuilderExtensions
{
    internal static void SetupIdentityTables(this ModelBuilder builder)
    {
        builder.Entity<AppUser>(b =>
        {
            b.ToTable("Users");

            // Each User can have many UserClaims
            b.HasMany(e => e.Claims)
                .WithOne()
                .HasForeignKey(uc => uc.UserId)
                .IsRequired();

            // Each User can have many UserLogins
            b.HasMany(e => e.Logins)
                .WithOne()
                .HasForeignKey(ul => ul.UserId)
                .IsRequired();

            // Each User can have many UserTokens
            b.HasMany(e => e.Tokens)
                .WithOne()
                .HasForeignKey(ut => ut.UserId)
                .IsRequired();

            // Each User can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne()
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });
        builder.Entity<AppRole>(b =>
        {
            b.ToTable("Roles");

            // Each Role can have many entries in the UserRole join table
            b.HasMany(e => e.UserRoles)
                .WithOne(e => e.Role)
                .HasForeignKey(ur => ur.RoleId)
                .IsRequired();
        });
        builder.Entity<AppRoleClaim>(b =>
        {
            b.ToTable("RoleClaims");

            b.HasOne(e => e.Role)
                .WithMany(e => e.Claims)
                .HasForeignKey(rc => rc.RoleId)
                .IsRequired();

        });
        builder.Entity<AppUserClaim>(b =>
        {
            b.ToTable("UserClaims");

            b.HasOne(e => e.User)
                .WithMany(e => e.Claims)
                .HasForeignKey(uc => uc.UserId)
                .IsRequired();
        });
        builder.Entity<AppUserLogin>(b =>
        {
            b.HasKey(e => new { e.LoginProvider, e.ProviderKey });

            b.ToTable("UserLogins");

            b.HasOne(e => e.User)
                .WithMany(e => e.Logins)
                .HasForeignKey(ul => ul.UserId)
                .IsRequired();
        });
        builder.Entity<AppUserRole>(b =>
        {
            b.HasKey(e => new { e.UserId, e.RoleId });

            b.ToTable("UserRoles");

            b.HasOne(e => e.User)
                .WithMany(e => e.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .IsRequired();
        });
        builder.Entity<AppUserToken>(b =>
        {
            b.HasKey(e => new { e.UserId, e.LoginProvider, e.Name });


            b.ToTable("UserTokens");

            b.HasOne(e => e.User)
                .WithMany(e => e.Tokens)
                .HasForeignKey(ut => ut.UserId)
                .IsRequired();
        });
    }
}