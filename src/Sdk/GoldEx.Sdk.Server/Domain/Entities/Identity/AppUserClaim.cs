using Microsoft.AspNetCore.Identity;

namespace GoldEx.Sdk.Server.Domain.Entities.Identity;

public class AppUserClaim : IdentityUserClaim<Guid>
{
    public virtual AppUser User { get; set; } = default!;
}