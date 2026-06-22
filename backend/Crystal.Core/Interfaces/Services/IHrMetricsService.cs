using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IHrMetricsService
{
    Task<HrDashboardMetricsDto> GetDashboardMetricsAsync();
}
