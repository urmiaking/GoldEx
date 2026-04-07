using GoldEx.Sdk.Server.Domain.Entities.Identity;
using GoldEx.Shared.DTOs.UserAccounts;
using Mapster;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;

namespace GoldEx.Server.Common.Mapping;

internal class UserAccountMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<AppUser, GetUserAccountResponse>()
            .Map(dest => dest.Username, src => src.UserName)
            .Map(dest => dest.FullName, src => src.Name)
            .Map(dest => dest.Role, src => src.UserRoles.First().Role.Name);

        config.NewConfig<UserPasskeyInfo, GetPasskeyResponse>()
            .Map(dest => dest.CredentialId, 
                src => WebEncoders.Base64UrlEncode(src.CredentialId));
    }
}