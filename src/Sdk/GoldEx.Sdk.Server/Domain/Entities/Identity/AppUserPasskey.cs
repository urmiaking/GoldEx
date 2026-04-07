using Microsoft.AspNetCore.Identity;

namespace GoldEx.Sdk.Server.Domain.Entities.Identity;

public class AppUserPasskey : IdentityUserPasskey<Guid>
{
    public virtual AppUser User { get; set; }
}