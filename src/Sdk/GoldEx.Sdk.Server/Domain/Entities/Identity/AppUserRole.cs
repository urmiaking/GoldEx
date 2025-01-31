using Microsoft.AspNetCore.Identity;

namespace GoldEx.Sdk.Server.Domain.Entities.Identity;

public class AppUserRole : IdentityUserRole<Guid>
{
    public virtual AppUser User { get; set; } = default!;
    public virtual AppRole Role { get; set; } = default!;
}
