using System.Net;

namespace ClaimPhaseService.Api.Security;

public sealed class IpAllowListOptions
{
    public const string SectionName = "IpAllowList";

    public bool Enabled { get; set; }

    /// <summary>CIDR ranges (for example 10.20.0.0/16) or single IPs.</summary>
    public List<string> AllowedRanges { get; set; } = new();
}

/// <summary>
/// Rejects callers whose remote IP is outside the configured corporate ranges.
/// Runs before authentication so unauthorised networks never reach the token endpoint.
/// </summary>
public sealed class IpAllowListMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IpAllowListOptions _options;
    private readonly ILogger<IpAllowListMiddleware> _logger;

    public IpAllowListMiddleware(RequestDelegate next, IpAllowListOptions options, ILogger<IpAllowListMiddleware> logger)
    {
        _next = next;
        _options = options;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_options.Enabled)
        {
            await _next(context);
            return;
        }

        var remoteIp = context.Connection.RemoteIpAddress;
        if (remoteIp is null || !_options.AllowedRanges.Any(range => IsInRange(remoteIp, range)))
        {
            _logger.LogWarning("Blocked request from disallowed remote address");
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Caller network is not allowed." });
            return;
        }

        await _next(context);
    }

    public static bool IsInRange(IPAddress remoteIp, string range)
    {
        if (remoteIp.IsIPv4MappedToIPv6)
        {
            remoteIp = remoteIp.MapToIPv4();
        }

        if (!range.Contains('/'))
        {
            return IPAddress.TryParse(range, out var single) && single.Equals(remoteIp);
        }

        var parts = range.Split('/', 2);
        if (!IPAddress.TryParse(parts[0], out var network) || !int.TryParse(parts[1], out var prefix))
        {
            return false;
        }

        if (network.AddressFamily != remoteIp.AddressFamily)
        {
            return false;
        }

        var networkBytes = network.GetAddressBytes();
        var remoteBytes = remoteIp.GetAddressBytes();
        var fullBytes = prefix / 8;
        var remainingBits = prefix % 8;

        for (var i = 0; i < fullBytes; i++)
        {
            if (networkBytes[i] != remoteBytes[i])
            {
                return false;
            }
        }

        if (remainingBits == 0)
        {
            return true;
        }

        var mask = (byte)(0xFF << (8 - remainingBits));
        return (networkBytes[fullBytes] & mask) == (remoteBytes[fullBytes] & mask);
    }
}
