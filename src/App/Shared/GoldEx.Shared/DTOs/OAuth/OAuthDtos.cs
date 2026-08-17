using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GoldEx.Shared.DTOs.OAuth;

public class ClientRegistrationRequest
{
    [JsonPropertyName("client_name")]
    public string? ClientName { get; set; }

    [JsonPropertyName("redirect_uris")]
    public List<string>? RedirectUris { get; set; }

    [JsonPropertyName("grant_types")]
    public List<string>? GrantTypes { get; set; }

    [JsonPropertyName("response_types")]
    public List<string>? ResponseTypes { get; set; }

    [JsonPropertyName("token_endpoint_auth_method")]
    public string? TokenEndpointAuthMethod { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}

public class ClientRegistrationResponse
{
    [JsonPropertyName("client_id")]
    public string ClientId { get; set; } = string.Empty;

    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; set; }

    [JsonPropertyName("client_name")]
    public string? ClientName { get; set; }

    [JsonPropertyName("redirect_uris")]
    public List<string> RedirectUris { get; set; } = [];

    [JsonPropertyName("grant_types")]
    public List<string> GrantTypes { get; set; } = [];

    [JsonPropertyName("response_types")]
    public List<string> ResponseTypes { get; set; } = [];

    [JsonPropertyName("token_endpoint_auth_method")]
    public string TokenEndpointAuthMethod { get; set; } = "client_secret_post";

    [JsonPropertyName("client_id_issued_at")]
    public long ClientIdIssuedAt { get; set; }
}

public class OAuthTokenRequest
{
    [JsonPropertyName("grant_type")]
    public string? GrantType { get; set; }

    [JsonPropertyName("code")]
    public string? Code { get; set; }

    [JsonPropertyName("redirect_uri")]
    public string? RedirectUri { get; set; }

    [JsonPropertyName("client_id")]
    public string? ClientId { get; set; }

    [JsonPropertyName("client_secret")]
    public string? ClientSecret { get; set; }

    [JsonPropertyName("code_verifier")]
    public string? CodeVerifier { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("scope")]
    public string? Scope { get; set; }
}

public class OAuthTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = "Bearer";

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }

    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("scope")]
    public string Scope { get; set; } = "mcp";
}

public class OAuthProtectedResourceMetadata
{
    [JsonPropertyName("resource")]
    public string Resource { get; set; } = string.Empty;

    [JsonPropertyName("authorization_servers")]
    public List<string> AuthorizationServers { get; set; } = [];

    [JsonPropertyName("scopes_supported")]
    public List<string> ScopesSupported { get; set; } = ["mcp"];

    [JsonPropertyName("bearer_methods_supported")]
    public List<string> BearerMethodsSupported { get; set; } = ["header"];
}

public class OAuthAuthorizationServerMetadata
{
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = string.Empty;

    [JsonPropertyName("authorization_endpoint")]
    public string AuthorizationEndpoint { get; set; } = string.Empty;

    [JsonPropertyName("token_endpoint")]
    public string TokenEndpoint { get; set; } = string.Empty;

    [JsonPropertyName("registration_endpoint")]
    public string RegistrationEndpoint { get; set; } = string.Empty;

    [JsonPropertyName("revocation_endpoint")]
    public string RevocationEndpoint { get; set; } = string.Empty;

    [JsonPropertyName("response_types_supported")]
    public List<string> ResponseTypesSupported { get; set; } = ["code"];

    [JsonPropertyName("grant_types_supported")]
    public List<string> GrantTypesSupported { get; set; } = ["authorization_code", "refresh_token"];

    [JsonPropertyName("token_endpoint_auth_methods_supported")]
    public List<string> TokenEndpointAuthMethodsSupported { get; set; } = ["client_secret_post", "client_secret_basic", "none"];

    [JsonPropertyName("response_modes_supported")]
    public List<string> ResponseModesSupported { get; set; } = ["query", "fragment"];

    [JsonPropertyName("code_challenge_methods_supported")]
    public List<string> CodeChallengeMethodsSupported { get; set; } = ["S256", "plain"];

    [JsonPropertyName("scopes_supported")]
    public List<string> ScopesSupported { get; set; } = ["mcp", "read", "write", "openid", "profile"];

    [JsonPropertyName("subject_types_supported")]
    public List<string> SubjectTypesSupported { get; set; } = ["public"];
}
