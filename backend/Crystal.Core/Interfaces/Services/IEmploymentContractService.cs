using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IEmploymentContractService
{
    Task<IEnumerable<EmploymentContractResponseDto>> GetAllAsync(string p_userId);
    Task<EmploymentContractResponseDto?> GetByIdAsync(int p_id, string p_userId);
    Task<EmploymentContractResponseDto> CreateAsync(CreateEmploymentContractRequest p_request);
    Task<EmploymentContractResponseDto> UpdateAsync(int p_id, UpdateEmploymentContractRequest p_request);
    Task DeleteAsync(int p_id);
}
