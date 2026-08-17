using GoldEx.Server.Domain.PersonalAccessTokenAggregate;
using GoldEx.Shared.DTOs.PersonalAccessTokens;
using Mapster;
using System;

namespace GoldEx.Server.Common.Mapping;

internal class PersonalAccessTokenMapper : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PersonalAccessTokenId, Guid>()
            .MapWith(src => src.Value);

        config.NewConfig<PersonalAccessToken, PersonalAccessTokenDto>()
            .Map(dest => dest.Id, src => src.Id.Value)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.TokenPrefix, src => src.TokenPrefix)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.ExpiresAt, src => src.ExpiresAt)
            .Map(dest => dest.LastUsedAt, src => src.LastUsedAt)
            .Map(dest => dest.IsRevoked, src => src.IsRevoked);
    }
}
