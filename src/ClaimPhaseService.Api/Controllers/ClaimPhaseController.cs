using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClaimPhaseService.Api.Models;
using ClaimPhaseService.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClaimPhaseService.Api.Controllers;

[ApiController]
[Route("api/v1/claims")]
[Authorize(Policy = "PhaseRead")]
[Produces("application/json")]
public sealed class ClaimPhaseController : ControllerBase
{
    private readonly IPhaseLookupService _lookup;

    // Temporary development key.
    // 32 bytes = AES-256
    private static readonly byte[] EncryptionKey =
        Encoding.UTF8.GetBytes("12345678901234567890123456789012");

    public ClaimPhaseController(IPhaseLookupService lookup)
        => _lookup = lookup;

    /// <summary>
    /// Returns the current phase for a control number, provided the phase is approved
    /// for self-service disclosure.
    /// </summary>
    [HttpGet("{controlId}/current-phase")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult GetCurrentPhase(
        [FromRoute][Required][StringLength(20, MinimumLength = 1)] string controlId)
    {
        var result = _lookup.GetCurrentPhase(controlId);

        return result.Status switch
        {
            LookupStatus.Success => Ok(new
            {
                data = EncryptResponse(result.Data)
            }),

            // Both "unknown control" and "not disclosable" return 404 so the API never
            // reveals the existence of a claim the caller may not see.
            _ => NotFound(new
            {
                error = "No disclosable phase found for the supplied control number."
            })
        };
    }

    /// <summary>
    /// Encrypts the API response using AES-256.
    /// </summary>
    private static string EncryptResponse(object data)
    {
        // Convert response object into JSON
        var json = JsonSerializer.Serialize(data);

        using var aes = Aes.Create();

        // AES-256 requires a 32-byte key
        aes.Key = EncryptionKey;

        // Generate a unique IV for every request
        aes.GenerateIV();

        var plainBytes = Encoding.UTF8.GetBytes(json);

        using var memoryStream = new MemoryStream();

        // Store IV at the beginning of the encrypted payload.
        // The IV is not secret; it is required for decryption.
        memoryStream.Write(aes.IV, 0, aes.IV.Length);

        using (var cryptoStream = new CryptoStream(
            memoryStream,
            aes.CreateEncryptor(),
            CryptoStreamMode.Write))
        {
            cryptoStream.Write(
                plainBytes,
                0,
                plainBytes.Length);

            cryptoStream.FlushFinalBlock();
        }

        // Return encrypted bytes as Base64
        return Convert.ToBase64String(
            memoryStream.ToArray());
    }
}