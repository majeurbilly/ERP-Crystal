using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/hr/metrics")]
[Authorize]
[RequirePermission(PermissionActions.Read, PermissionSubjects.HrDashboard)]
public class HrMetricsController : ControllerBase
{
    private readonly IHrMetricsService m_hrMetricsService;

    public HrMetricsController(IHrMetricsService p_hrMetricsService)
    {
        m_hrMetricsService = p_hrMetricsService;
    }

    [HttpGet]
    public async Task<ActionResult<HrDashboardMetricsDto>> GetDashboardMetrics()
    {
        HrDashboardMetricsDto metrics = await m_hrMetricsService.GetDashboardMetricsAsync();
        return Ok(metrics);
    }
}
