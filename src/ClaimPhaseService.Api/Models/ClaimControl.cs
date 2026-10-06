namespace ClaimPhaseService.Api.Models;

/// <summary>
/// A claim control record. The control number (ControlId) is what the caller supplies;
/// it maps to exactly one ValidationTypeId.
/// </summary>
public sealed record ClaimControl(string ControlId, string ValidationTypeId);
