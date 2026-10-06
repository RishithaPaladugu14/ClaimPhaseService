using System.Net;
using ClaimPhaseService.Api.Security;
using Xunit;

namespace ClaimPhaseService.Tests;

public class IpAllowListMiddlewareTests
{
    [Theory]
    [InlineData("10.20.30.40", "10.0.0.0/8", true)]
    [InlineData("11.20.30.40", "10.0.0.0/8", false)]
    [InlineData("192.168.1.55", "192.168.1.0/24", true)]
    [InlineData("192.168.2.55", "192.168.1.0/24", false)]
    [InlineData("127.0.0.1", "127.0.0.1", true)]
    [InlineData("127.0.0.2", "127.0.0.1", false)]
    [InlineData("10.1.2.3", "not-a-range", false)]
    public void IsInRange_MatchesOnlyAddressesInsideTheRange(string ip, string range, bool expected)
    {
        Assert.Equal(expected, IpAllowListMiddleware.IsInRange(IPAddress.Parse(ip), range));
    }
}
