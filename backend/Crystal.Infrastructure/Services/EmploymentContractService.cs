using Crystal.Core.Authorization;
using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Services.Validation;

namespace Crystal.Infrastructure.Services;

public class EmploymentContractService : IEmploymentContractService
{
    private readonly IEmploymentContractRepository m_employmentContractRepository;
    private readonly IEmployeeProfileRepository m_employeeProfileRepository;
    private readonly IEmployeeScopeService m_employeeScopeService;

    public EmploymentContractService(
        IEmploymentContractRepository p_employmentContractRepository,
        IEmployeeProfileRepository p_employeeProfileRepository,
        IEmployeeScopeService p_employeeScopeService)
    {
        m_employmentContractRepository = p_employmentContractRepository;
        m_employeeProfileRepository = p_employeeProfileRepository;
        m_employeeScopeService = p_employeeScopeService;
    }

    public async Task<IEnumerable<EmploymentContractResponseDto>> GetAllAsync(string p_userId)
    {
        IEnumerable<EmploymentContract> contracts;

        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.EmploymentContract))
        {
            contracts = await m_employmentContractRepository.GetAllAsync();
        }
        else
        {
            int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
            if (!profileId.HasValue)
            {
                return [];
            }

            contracts = await m_employmentContractRepository.GetByEmployeeProfileIdAsync(profileId.Value);
        }

        return contracts.Select(MapToDto);
    }

    public async Task<EmploymentContractResponseDto?> GetByIdAsync(int p_id, string p_userId)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        EmploymentContract? contract = await m_employmentContractRepository.GetByIdAsync(p_id);

        if (contract is null)
        {
            return null;
        }

        if (!await CanAccessContractAsync(contract, p_userId))
        {
            return null;
        }

        return MapToDto(contract);
    }

    public async Task<EmploymentContractResponseDto> CreateAsync(CreateEmploymentContractRequest p_request)
    {
        await EnsureEmployeeProfileExistsAsync(p_request.EmployeeProfileId);
        ValidateDateRange(p_request.StartDate, p_request.EndDate);
        await EnsureNoOverlappingContractsAsync(
            p_request.EmployeeProfileId,
            p_request.StartDate,
            p_request.EndDate,
            null);

        EmploymentContract contract = new EmploymentContract
        {
            EmployeeProfileId = p_request.EmployeeProfileId,
            ContractType = p_request.ContractType,
            WageType = p_request.WageType,
            BaseRate = p_request.BaseRate,
            StartDate = p_request.StartDate,
            EndDate = p_request.EndDate,
            IsDeleted = false
        };

        await m_employmentContractRepository.AddAsync(contract);
        await m_employmentContractRepository.SaveChangesAsync();

        EmploymentContract? createdContract = await m_employmentContractRepository.GetByIdAsync(contract.Id);
        if (createdContract is null)
        {
            throw new InvalidOperationException(ErrorMessages.EmploymentContract.CreateRetrievalFailed);
        }

        return MapToDto(createdContract);
    }

    public async Task<EmploymentContractResponseDto> UpdateAsync(int p_id, UpdateEmploymentContractRequest p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        EmploymentContract? existingContract = await m_employmentContractRepository.GetTrackedByIdAsync(p_id);
        if (existingContract is null)
        {
            throw new KeyNotFoundException(ErrorMessages.EmploymentContract.NotFound);
        }

        await EnsureEmployeeProfileExistsAsync(p_request.EmployeeProfileId);
        ValidateDateRange(p_request.StartDate, p_request.EndDate);
        await EnsureNoOverlappingContractsAsync(
            p_request.EmployeeProfileId,
            p_request.StartDate,
            p_request.EndDate,
            p_id);

        existingContract.EmployeeProfileId = p_request.EmployeeProfileId;
        existingContract.ContractType = p_request.ContractType;
        existingContract.WageType = p_request.WageType;
        existingContract.BaseRate = p_request.BaseRate;
        existingContract.StartDate = p_request.StartDate;
        existingContract.EndDate = p_request.EndDate;

        await m_employmentContractRepository.UpdateAsync(existingContract);
        await m_employmentContractRepository.SaveChangesAsync();

        EmploymentContract? updatedContract = await m_employmentContractRepository.GetByIdAsync(p_id);
        if (updatedContract is null)
        {
            throw new InvalidOperationException(ErrorMessages.EmploymentContract.UpdateRetrievalFailed);
        }

        return MapToDto(updatedContract);
    }

    public async Task DeleteAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        EmploymentContract? existingContract = await m_employmentContractRepository.GetTrackedByIdAsync(p_id);
        if (existingContract is null)
        {
            throw new KeyNotFoundException(ErrorMessages.EmploymentContract.NotFound);
        }

        await m_employmentContractRepository.SoftDeleteAsync(existingContract);
        await m_employmentContractRepository.SaveChangesAsync();
    }

    private async Task EnsureEmployeeProfileExistsAsync(int p_employeeProfileId)
    {
        EntityIdentifierValidator.EnsureValidEmployeeProfileId(p_employeeProfileId);

        EmployeeProfile? employeeProfile = await m_employeeProfileRepository.GetByIdAsync(p_employeeProfileId);
        if (employeeProfile is null)
        {
            throw new InvalidOperationException(ErrorMessages.EmployeeProfile.SpecifiedNotFound);
        }
    }

    private static void ValidateDateRange(DateOnly p_startDate, DateOnly? p_endDate)
    {
        if (p_endDate.HasValue && p_endDate.Value < p_startDate)
        {
            throw new ArgumentException(ErrorMessages.EmploymentContract.EndDateBeforeStartDate);
        }
    }

    private async Task EnsureNoOverlappingContractsAsync(
        int p_employeeProfileId,
        DateOnly p_startDate,
        DateOnly? p_endDate,
        int? p_excludeContractId)
    {
        bool hasOverlap = await m_employmentContractRepository.HasOverlappingContractsAsync(
            p_employeeProfileId,
            p_startDate,
            p_endDate,
            p_excludeContractId);

        if (hasOverlap)
        {
            throw new InvalidOperationException(ErrorMessages.EmploymentContract.ActiveContractAlreadyExists);
        }
    }

    private async Task<bool> CanAccessContractAsync(EmploymentContract p_contract, string p_userId)
    {
        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.EmploymentContract))
        {
            return true;
        }

        int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
        return profileId.HasValue && p_contract.EmployeeProfileId == profileId.Value;
    }

    private static EmploymentContractResponseDto MapToDto(EmploymentContract p_contract)
    {
        return new EmploymentContractResponseDto
        {
            Id = p_contract.Id,
            EmployeeProfileId = p_contract.EmployeeProfileId,
            EmployeeFirstName = p_contract.EmployeeProfile.FirstName,
            EmployeeLastName = p_contract.EmployeeProfile.LastName,
            ContractType = p_contract.ContractType.ToString(),
            WageType = p_contract.WageType.ToString(),
            BaseRate = p_contract.BaseRate,
            StartDate = p_contract.StartDate,
            EndDate = p_contract.EndDate
        };
    }
}
