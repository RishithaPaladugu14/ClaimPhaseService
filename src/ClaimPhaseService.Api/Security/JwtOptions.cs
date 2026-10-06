namespace ClaimPhaseService.Api.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 30;
}

public sealed class ApiClientOptions
{
    public const string SectionName = "ApiClients";

    public List<ApiClient> Clients { get; set; } = new();
}

public sealed class ApiClient
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public List<string> Scopes { get; set; } = new();
}
