using Microsoft.AspNetCore.Identity;

namespace GoldEx.Sdk.Server.Domain.Entities.Identity;

public class AppUserToken : IdentityUserToken<Guid>
{
    public virtual AppUser User { get; set; } = default!;
}