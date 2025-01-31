using Microsoft.AspNetCore.Identity;

namespace GoldEx.Sdk.Server.Domain.Entities.Identity;

public class AppUserLogin : IdentityUserLogin<Guid>
{
    public virtual AppUser User { get; set; } = default!;
}