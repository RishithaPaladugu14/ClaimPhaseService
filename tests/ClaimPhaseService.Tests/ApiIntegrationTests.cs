using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ClaimPhaseService.Api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace ClaimPhaseService.Tests;

public class ApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public ApiIntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/token",
            new TokenRequest { ClientId = "self-service-portal", ClientSecret = "portal-dev-secret" });
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenResponse>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token!.AccessToken);
        return client;
    }

    [Fact]
    public async Task Token_WithBadSecret_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/token",
            new TokenRequest { ClientId = "self-service-portal", ClientSecret = "wrong" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CurrentPhase_WithoutToken_Returns401()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/v1/claims/CTRL-2024-0000001/current-phase");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CurrentPhase_WithToken_ReturnsCurrentPhase()
    {
        var client = await CreateAuthenticatedClientAsync();

        var payload = await client.GetFromJsonAsync<PhaseLookupResponse>("/api/v1/claims/CTRL-2024-0000002/current-phase");

        Assert.NotNull(payload);
        Assert.Equal("APPEAL", payload!.medicalRecordStatus);
        Assert.Equal("VT-CLM-002", payload.ValidationTypeId);
    }

    [Fact]
    public async Task CurrentPhase_NonDisclosablePhase_Returns404()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetAsync("/api/v1/claims/CTRL-2024-0000004/current-phase");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CurrentPhase_ResponseContainsOnlyDisclosableFields()
    {
        var client = await CreateAuthenticatedClientAsync();

        var json = await client.GetStringAsync("/api/v1/claims/CTRL-2024-0000001/current-phase");

        Assert.DoesNotContain("isCurrent", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("approvedForSelfServiceDisclosure", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Health_IsAnonymous()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
