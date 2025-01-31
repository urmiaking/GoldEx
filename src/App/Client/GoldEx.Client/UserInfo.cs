using System.Security.Claims;

namespace GoldEx.Client;

// Add properties to this class and update the server and client AuthenticationStateProviders
// to expose more information about the authenticated user to the client.
public class UserInfo
{
    public UserInfo()
    {

    }
    public UserInfo(IEnumerable<Claim> claims)
    {
        UserClaims = claims.Select(x => new UserClaim(x.Type, x.Value)).ToList();
    }

    public List<UserClaim> UserClaims { get; set; } = default!;

    public IEnumerable<Claim> Claims => UserClaims.Select(x => new Claim(x.Type, x.Value));
}

public class UserClaim(string type, string value)
{
    public string Type { get; set; } = type;
    public string Value { get; set; } = value;
}
