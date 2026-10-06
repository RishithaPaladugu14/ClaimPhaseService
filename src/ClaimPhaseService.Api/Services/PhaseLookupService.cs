using ClaimPhaseService.Api.Data;
using ClaimPhaseService.Api.Models;

namespace ClaimPhaseService.Api.Services;

public enum LookupStatus
{
    Success,
    ControlNotFound,
    NoCurrentPhase,
    NotApprovedForDisclosure
}

public sealed record LookupResult(LookupStatus Status, PhaseLookupResponse? Data)
{
    public static LookupResult Ok(PhaseLookupResponse data) => new(LookupStatus.Success, data);
    public static LookupResult Fail(LookupStatus status) => new(status, null);
}

public interface IPhaseLookupService
{
    LookupResult GetCurrentPhase(string controlId);
}

public sealed class PhaseLookupService : IPhaseLookupService
{
    private readonly IClaimDataStore _store;
    private readonly ILogger<PhaseLookupService> _logger;

    public PhaseLookupService(IClaimDataStore store, ILogger<PhaseLookupService> logger)
    {
        _store = store;
        _logger = logger;
    }

    public LookupResult GetCurrentPhase(string controlId)
    {
        var control = _store.FindControl(controlId.Trim());
        if (control is null)
        {
            _logger.LogInformation("Control number not found");
            return LookupResult.Fail(LookupStatus.ControlNotFound);
        }

        var current = _store.GetPhases(control.ValidationTypeId).SingleOrDefault(p => p.IsCurrent);
        if (current is null)
        {
            return LookupResult.Fail(LookupStatus.NoCurrentPhase);
        }

        if (!current.ApprovedForSelfServiceDisclosure)
        {
            return LookupResult.Fail(LookupStatus.NotApprovedForDisclosure);
        }

        return LookupResult.Ok(new PhaseLookupResponse(
            control.ControlId,
            control.ValidationTypeId,
            current.PhaseCode,
            current.PhaseDescription,
            current.EffectiveDate));
    }
}
