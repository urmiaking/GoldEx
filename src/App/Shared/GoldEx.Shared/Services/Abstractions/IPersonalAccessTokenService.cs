using GoldEx.Shared.DTOs.PersonalAccessTokens;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GoldEx.Shared.Services.Abstractions;

public interface IPersonalAccessTokenService
{
    Task<List<PersonalAccessTokenDto>> GetListAsync(CancellationToken cancellationToken = default);
    Task<CreatePersonalAccessTokenResponse> CreateAsync(CreatePersonalAccessTokenRequest request, CancellationToken cancellationToken = default);
    Task RevokeAsync(Guid id, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
