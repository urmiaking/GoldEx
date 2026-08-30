using Microsoft.AspNetCore.Identity;

namespace GoldEx.Sdk.Server.Domain.Entities.Identity;

public class AppUserPasskey : IdentityUserPasskey<Guid>
{
#pragma warning disable CS8618
    public virtual AppUser User { get; set; }
#pragma warning restore CS8618
}