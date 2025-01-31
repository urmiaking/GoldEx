using Microsoft.AspNetCore.Identity;

namespace GoldEx.Sdk.Server.Domain.Entities.Identity;

public class AppUser : IdentityUser<Guid>
{
    public string Name { get; private set; } = default!;

    public virtual ICollection<AppUserClaim> Claims { get; set; } = default!;
    public virtual ICollection<AppUserLogin> Logins { get; set; } = default!;
    public virtual ICollection<AppUserToken> Tokens { get; set; } = default!;
    public virtual ICollection<AppUserRole> UserRoles { get; set; } = default!;

    public AppUser(string name, string username, string? email = null, string? phoneNumber = null)
    {
        Id = Guid.NewGuid();
        Name = name;
        UserName = username;
        Email = email;
        PhoneNumber = phoneNumber;
    }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private AppUser()
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    {
    }
}
