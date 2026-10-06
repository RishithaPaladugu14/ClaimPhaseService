using ClaimPhaseService.Api.Models;
using ClaimPhaseService.Api.Security;
using Microsoft.AspNetCore.Mvc;

namespace ClaimPhaseService.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public sealed class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService) => _tokenService = tokenService;

    /// <summary>Exchanges client credentials for a short lived JWT bearer token.</summary>
    [HttpPost("token")]
    [ProducesResponseType(typeof(TokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult<TokenResponse> Token([FromBody] TokenRequest request)
    {
        var token = _tokenService.Issue(request.ClientId, request.ClientSecret);
        return token is null ? Unauthorized(new { error = "Invalid client credentials." }) : Ok(token);
    }
}
