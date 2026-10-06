namespace ClaimPhaseService.Api.Models;

/// <summary>
/// A phase belonging to a validation type. One ValidationTypeId has many phases,
/// but only one of them has IsCurrent = true.
/// </summary>
public sealed record ValidationPhase(
    string ValidationTypeId,
    string PhaseCode,
    string PhaseDescription,
    bool IsCurrent,
    bool ApprovedForSelfServiceDisclosure,
    DateOnly EffectiveDate);
