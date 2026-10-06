using ClaimPhaseService.Api.Data;
using ClaimPhaseService.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ClaimPhaseService.Tests;

public class PhaseLookupServiceTests
{
    private static PhaseLookupService CreateSut() =>
        new(new StaticClaimDataStore(), NullLogger<PhaseLookupService>.Instance);

    [Theory]
    [InlineData("CTRL-2024-0000001", "VT-CLM-001", "OPEN")]
    [InlineData("CTRL-2024-0000002", "VT-CLM-002", "APPEAL")]
    [InlineData("CTRL-2024-0000003", "VT-CLM-003", "UPHELD")]
    [InlineData("CTRL-2024-0000005", "VT-CLM-005", "CLOSED")]
    public void GetCurrentPhase_ReturnsCurrentDisclosablePhase(string controlId, string validationTypeId, string phaseCode)
    {
        var result = CreateSut().GetCurrentPhase(controlId);

        Assert.Equal(LookupStatus.Success, result.Status);
        Assert.NotNull(result.Data);
        Assert.Equal(controlId, result.Data!.ControlId);
        Assert.Equal(validationTypeId, result.Data.ValidationTypeId);
        Assert.Equal(phaseCode, result.Data.PhaseCode);
    }

    [Fact]
    public void GetCurrentPhase_IsCaseInsensitiveAndTrimsInput()
    {
        var result = CreateSut().GetCurrentPhase("  ctrl-2024-0000001 ");

        Assert.Equal(LookupStatus.Success, result.Status);
        Assert.Equal("OPEN", result.Data!.PhaseCode);
    }

    [Fact]
    public void GetCurrentPhase_UnknownControl_ReturnsControlNotFound()
    {
        var result = CreateSut().GetCurrentPhase("CTRL-DOES-NOT-EXIST");

        Assert.Equal(LookupStatus.ControlNotFound, result.Status);
        Assert.Null(result.Data);
    }

    [Fact]
    public void GetCurrentPhase_CurrentPhaseNotDisclosable_ReturnsNotApproved()
    {
        var result = CreateSut().GetCurrentPhase("CTRL-2024-0000004");

        Assert.Equal(LookupStatus.NotApprovedForDisclosure, result.Status);
        Assert.Null(result.Data);
    }
}

public class StaticClaimDataStoreTests
{
    [Fact]
    public void EveryValidationType_HasExactlyOneCurrentPhase()
    {
        var store = new StaticClaimDataStore();
        var validationTypeIds = new[] { "VT-CLM-001", "VT-CLM-002", "VT-CLM-003", "VT-CLM-004", "VT-CLM-005" };

        foreach (var id in validationTypeIds)
        {
            Assert.Single(store.GetPhases(id), p => p.IsCurrent);
        }
    }

    [Fact]
    public void ControlIdsRespectColumnLengths()
    {
        var store = new StaticClaimDataStore();
        var control = store.FindControl("CTRL-2024-0000001");

        Assert.NotNull(control);
        Assert.True(control!.ControlId.Length <= 20);
        Assert.True(control.ValidationTypeId.Length <= 10);
    }
}
