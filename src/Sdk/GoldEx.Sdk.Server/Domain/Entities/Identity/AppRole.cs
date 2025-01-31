using Microsoft.AspNetCore.Identity;

namespace GoldEx.Sdk.Server.Domain.Entities.Identity;

public class AppRole : IdentityRole<Guid>
{
    public virtual ICollection<AppUserRole>? UserRoles { get; set; }
    public virtual ICollection<AppRoleClaim>? Claims { get; set; }

    public AppRole(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    private AppRole() { }
}
