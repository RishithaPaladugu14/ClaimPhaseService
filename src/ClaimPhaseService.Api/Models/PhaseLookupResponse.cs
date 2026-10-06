namespace ClaimPhaseService.Api.Models;

/// <summary>
/// The only shape returned to callers. Internal flags (IsCurrent, disclosure approval)
/// are deliberately not exposed - callers get the minimum data needed for the use case.
/// </summary>
public sealed record PhaseLookupResponse(
    string ControlId,
    string ValidationTypeId,
    string PhaseCode,
    string PhaseDescription,
    DateOnly EffectiveDate);
