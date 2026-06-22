using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IPunchEligibilityService
{
    Task<PunchEligibilityDto> EvaluateAsync(string p_userId);

    Task EnsurePunchInAllowedAsync(string p_userId);
}
