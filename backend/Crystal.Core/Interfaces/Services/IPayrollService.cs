using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IPayrollService
{
    Task<IEnumerable<PayStubResponseDto>> GetAllPayStubsAsync(string p_userId);
    Task<IEnumerable<PayPeriodResponseDto>> GetAllPayPeriodsAsync();
    Task<PayPeriodResponseDto> CreatePayPeriodAsync(CreatePayPeriodRequest p_request);
    Task<PayStubResponseDto> GeneratePayStubAsync(int p_payPeriodId, int p_employeeProfileId);
    Task<GeneratePayrollForPeriodResponseDto> GenerateForPeriodAsync(
        int p_payPeriodId,
        string p_userId,
        int? p_locationId);
    Task<PayStubResponseDto> PublishPayStubAsync(int p_payStubId);
}
