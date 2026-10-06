using System.ComponentModel.DataAnnotations;

namespace ClaimPhaseService.Api.Models;

public sealed class TokenRequest
{
    [Required]
    public string ClientId { get; set; } = string.Empty;

    [Required]
    public string ClientSecret { get; set; } = string.Empty;
}

public sealed record TokenResponse(string AccessToken, string TokenType, int ExpiresInSeconds);
