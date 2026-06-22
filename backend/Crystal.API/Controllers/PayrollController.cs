using Crystal.Core.Authorization;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Interfaces.Services;
using Crystal.API.Authorization;
using Crystal.API.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Crystal.API.Controllers;

[ApiController]
[Route("api/payroll")]
[Authorize]
public class PayrollController : ControllerBase
{
    private readonly IPayrollService m_payrollService;

    public PayrollController(IPayrollService p_payrollService)
    {
        m_payrollService = p_payrollService;
    }

    [HttpGet("periods")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Payroll)]
    public async Task<ActionResult<IEnumerable<PayPeriodResponseDto>>> GetPeriods()
    {
        IEnumerable<PayPeriodResponseDto> payPeriods = await m_payrollService.GetAllPayPeriodsAsync();
        return Ok(payPeriods);
    }

    [HttpPost("periods")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Payroll)]
    public async Task<ActionResult<PayPeriodResponseDto>> CreatePeriod([FromBody] CreatePayPeriodRequest p_request)
    {
        PayPeriodResponseDto created = await m_payrollService.CreatePayPeriodAsync(p_request);
        return CreatedAtAction(nameof(GetPeriods), created);
    }

    [HttpPost("generate")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Payroll)]
    public async Task<ActionResult<PayStubResponseDto>> Generate([FromBody] GeneratePayrollRequest p_request)
    {
        PayStubResponseDto payStub = await m_payrollService.GeneratePayStubAsync(
            p_request.PayPeriodId,
            p_request.EmployeeProfileId);

        return Ok(payStub);
    }

    [HttpPost("generate-period")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Payroll)]
    public async Task<ActionResult<GeneratePayrollForPeriodResponseDto>> GenerateForPeriod(
        [FromBody] GeneratePayrollForPeriodRequest p_request)
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        GeneratePayrollForPeriodResponseDto result =
            await m_payrollService.GenerateForPeriodAsync(
                p_request.PayPeriodId,
                userId,
                p_request.LocationId);

        return Ok(result);
    }

    [HttpGet("stubs")]
    [RequirePermission(PermissionActions.Read, PermissionSubjects.Payroll)]
    public async Task<ActionResult<IEnumerable<PayStubResponseDto>>> GetStubs()
    {
        string? userId = this.GetCurrentUserId();
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized();
        }

        IEnumerable<PayStubResponseDto> payStubs = await m_payrollService.GetAllPayStubsAsync(userId);
        return Ok(payStubs);
    }

    [HttpPost("stubs/{p_id:int}/publish")]
    [RequirePermission(PermissionActions.Manage, PermissionSubjects.Payroll)]
    public async Task<ActionResult<PayStubResponseDto>> Publish(int p_id)
    {
        PayStubResponseDto payStub = await m_payrollService.PublishPayStubAsync(p_id);
        return Ok(payStub);
    }
}
