using ClaimPhaseService.Api.Models;

namespace ClaimPhaseService.Api.Data;

/// <summary>
/// In-memory stand-in for the future SQL tables. Swap this class for a repository
/// backed by EF Core / Dapper later - the service only depends on IClaimDataStore.
/// </summary>
public interface IClaimDataStore
{
    ClaimControl? FindControl(string controlId);
    IReadOnlyList<ValidationPhase> GetPhases(string validationTypeId);
}

public sealed class StaticClaimDataStore : IClaimDataStore
{
    // Table 1: CLAIM_CONTROL (ControlId varchar(20) PK, ValidationTypeId varchar(10))
    private static readonly IReadOnlyList<ClaimControl> Controls = new List<ClaimControl>
    {
        new("CTRL-2024-0000001", "VT-CLM-001"),
        new("CTRL-2024-0000002", "VT-CLM-002"),
        new("CTRL-2024-0000003", "VT-CLM-003"),
        new("CTRL-2024-0000004", "VT-CLM-004"),
        new("CTRL-2024-0000005", "VT-CLM-005"),
        new("CTRL-2024-0000006", "VT-CLM-006"),
        new("CTRL-2024-0000007", "VT-CLM-007"),
        new("CTRL-2024-0000008", "VT-CLM-008"),
        new("CTRL-2024-0000009", "VT-CLM-009"),
        new("CTRL-2024-0000010", "VT-CLM-010"),
        new("CTRL-2024-0000011", "VT-CLM-011"),
        new("CTRL-2024-0000012", "VT-CLM-012")


    };

    // Table 2: VALIDATION_PHASE (ValidationTypeId varchar(10), PhaseCode, IsCurrent bit, ...)
    private static readonly IReadOnlyList<ValidationPhase> Phases = new List<ValidationPhase>
    {
        // VT-CLM-001 - claim received and still open
        new("VT-CLM-001", "OPEN", "Claim received and under review", true, true, new DateOnly(2024, 3, 1)),
        new("VT-CLM-001", "INTAKE", "Claim intake completed", false, true, new DateOnly(2024, 2, 20)),

        // VT-CLM-002 - denied then appealed, appeal is current
        new("VT-CLM-002", "INTAKE", "Claim intake completed", false, true, new DateOnly(2024, 1, 5)),
        new("VT-CLM-002", "DENIED", "Claim denied by payer", false, true, new DateOnly(2024, 1, 25)),
        new("VT-CLM-002", "APPEAL", "Appeal submitted and under review", true, true, new DateOnly(2024, 2, 10)),

        // VT-CLM-003 - appeal decided, denial upheld
        new("VT-CLM-003", "APPEAL", "Appeal submitted and under review", false, true, new DateOnly(2024, 1, 15)),
        new("VT-CLM-003", "UPHELD", "Original determination upheld on appeal", true, true, new DateOnly(2024, 4, 2)),

        // VT-CLM-004 - current phase is an internal-only investigation, not disclosable
        new("VT-CLM-004", "OPEN", "Claim received and under review", false, true, new DateOnly(2024, 5, 1)),
        new("VT-CLM-004", "SIU_REVIEW", "Special investigation unit review", true, false, new DateOnly(2024, 6, 11)),

        // VT-CLM-005 - closed and paid
        new("VT-CLM-005", "OPEN", "Claim received and under review", false, true, new DateOnly(2024, 2, 1)),
        new("VT-CLM-005", "OVERTURNED", "Determination overturned in member favour", false, true, new DateOnly(2024, 3, 12)),
        new("VT-CLM-005", "CLOSED", "Claim closed and paid", true, true, new DateOnly(2024, 3, 30)),
        // VT-CLM-006 - closed and paid
        new("VT-CLM-006", "OPEN", "Claim received and under review", false, true, new DateOnly(2024, 2, 1)),
        new("VT-CLM-006", "OVERTURNED", "Determination overturned in member favour", false, true, new DateOnly(2024, 3, 12)),
        new("VT-CLM-006", "CLOSED", "Claim closed and paid", false, true, new DateOnly(2024, 3, 30)),
        new("VT-CLM-006", "DENIED", "Claim received and under review", false, true, new DateOnly(2025, 2, 1)),
        new("VT-CLM-006", "UPHELD", "Determination overturned in member favour", false, true, new DateOnly(2024, 7, 12)),
        new("VT-CLM-006", "APPEAL", "Claim closed and paid", true, true, new DateOnly(2024, 8, 30)),

        // VT-CLM-007 - claim received and still open
        new("VT-CLM-007", "OPEN", "Claim received and under review", false, true, new DateOnly(2024, 3, 1)),
        new("VT-CLM-007", "INTAKE", "Claim intake completed", false, true, new DateOnly(2024, 2, 20)),
        new("VT-CLM-007", "CLOSE", "Claim intake completed", true, true, new DateOnly(2024, 2, 20)),

        // VT-CLM-008 - denied then appealed, appeal is current
        new("VT-CLM-008", "INTAKE", "Claim intake completed", false, true, new DateOnly(2024, 1, 5)),
        new("VT-CLM-008", "DENIED", "Claim denied by payer", false, true, new DateOnly(2024, 1, 25)),
        new("VT-CLM-008", "APPEAL", "Appeal submitted and under review", true, true, new DateOnly(2024, 2, 10)),

        // VT-CLM-009 - appeal decided, denial upheld
        new("VT-CLM-009", "APPEAL", "Appeal submitted and under review", false, true, new DateOnly(2024, 1, 15)),
        new("VT-CLM-009", "UPHELD", "Original determination upheld on appeal", true, true, new DateOnly(2024, 4, 2)),

        // VT-CLM-010 - current phase is an internal-only investigation, not disclosable
        new("VT-CLM-010", "OPEN", "Claim received and under review", false, true, new DateOnly(2024, 5, 1)),
        new("VT-CLM-010", "SIU_REVIEW", "Special investigation unit review", true, false, new DateOnly(2024, 6, 11)),

        // VT-CLM-011 - closed and paid
        new("VT-CLM-011", "OPEN", "Claim received and under review", false, true, new DateOnly(2024, 2, 1)),
        new("VT-CLM-011", "OVERTURNED", "Determination overturned in member favour", false, true, new DateOnly(2024, 3, 12)),
        new("VT-CLM-011", "CLOSED", "Claim closed and paid", true, true, new DateOnly(2024, 3, 30)),
        // VT-CLM-012 - closed and paid
        new("VT-CLM-012", "OPEN", "Claim received and under review", false, true, new DateOnly(2024, 2, 1)),
        new("VT-CLM-012", "OVERTURNED", "Determination overturned in member favour", false, true, new DateOnly(2024, 3, 12)),
        new("VT-CLM-012", "CLOSED", "Claim closed and paid", false, true, new DateOnly(2024, 3, 30)),
        new("VT-CLM-012", "DENIED", "Claim received and under review", false, true, new DateOnly(2025, 2, 1)),
        new("VT-CLM-012", "UPHELD", "Determination overturned in member favour", false, true, new DateOnly(2024, 7, 12)),
        new("VT-CLM-012", "APPEAL", "Claim closed and paid", false , true, new DateOnly(2024, 8, 30))
    };

    public ClaimControl? FindControl(string controlId) =>
        Controls.FirstOrDefault(c => string.Equals(c.ControlId, controlId, StringComparison.OrdinalIgnoreCase));

    public IReadOnlyList<ValidationPhase> GetPhases(string validationTypeId) =>
        Phases.Where(p => string.Equals(p.ValidationTypeId, validationTypeId, StringComparison.OrdinalIgnoreCase)).ToList();
}
