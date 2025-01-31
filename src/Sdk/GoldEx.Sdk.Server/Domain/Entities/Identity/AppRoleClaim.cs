using Microsoft.AspNetCore.Identity;

namespace GoldEx.Sdk.Server.Domain.Entities.Identity;

public class AppRoleClaim : IdentityRoleClaim<Guid>
{
    public virtual AppRole Role { get; set; } = default!;
}