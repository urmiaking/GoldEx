using GoldEx.Sdk.Common.Data;
using GoldEx.Sdk.Server.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using GoldEx.Server.Domain.CustomerAggregate;

namespace GoldEx.Server.Application.Services.Abstractions;

public interface IAccountService
{
    Task<bool> AddRoleClaimAsync(string roleName, Claim claim, CancellationToken cancellationToken = default);
    Task<bool> AddUserClaimAsync(string username, Claim claim, CancellationToken cancellationToken = default);
    Task ConfirmPhoneNumberAsync(AppUser user, CancellationToken cancellationToken = default);
    Task<IdentityResult> CreateUserAsync(AppUser user, string? password = null, string[]? roles = null, CancellationToken cancellationToken = default);
    Task<bool> DeleteRoleAsync(string roleName, CancellationToken cancellationToken = default);
    Task EnsureRoleAsync(string role, CancellationToken cancellationToken = default);
    Task<bool> EnsureRoleAsync(string username, string roleName, CancellationToken cancellationToken = default);
    Task EnsureRolesAsync(AppUser user, string[] roles, CancellationToken cancellationToken = default);
    Task<AppUser?> FindUserAsync(string username);
    string GenerateRandomPassword(int length);
    Task<AppUser?> GetAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IList<Claim>> GetRoleClaimsAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppUser>> GetRoleMembersAsync(string roleName, CancellationToken cancellationToken = default);
    Task<IEnumerable<AppRole>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<IList<Claim>> GetUserClaimsAsync(string username, CancellationToken cancellationToken = default);
    Task<IList<string>> GetUserRolesAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> RemoveRoleAsync(string username, string roleName, CancellationToken cancellationToken = default);
    Task<bool> RemoveRoleClaimAsync(string roleName, Claim claim, CancellationToken cancellationToken = default);
    Task<bool> RemoveUserClaimAsync(string username, Claim claim, CancellationToken cancellationToken = default);
    Task<bool> RenameRoleAsync(string roleName, string newName, CancellationToken cancellationToken = default);
    Task SetMobileAsync(AppUser user, string mobile, CancellationToken cancellationToken = default);
    Task<bool> SetPasswordAsync(AppUser user, string password, CancellationToken cancellationToken = default);
    Task<bool> SetPasswordAsync(string username, string password, CancellationToken cancellationToken = default);
}