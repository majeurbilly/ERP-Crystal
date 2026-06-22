using Crystal.Core.Authorization;
using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Enums;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Services.Validation;

namespace Crystal.Infrastructure.Services;

public class LeaveRequestService : ILeaveRequestService
{
    private readonly ILeaveRequestRepository m_leaveRequestRepository;
    private readonly IEmployeeProfileRepository m_employeeProfileRepository;
    private readonly IEmployeeScopeService m_employeeScopeService;

    public LeaveRequestService(
        ILeaveRequestRepository p_leaveRequestRepository,
        IEmployeeProfileRepository p_employeeProfileRepository,
        IEmployeeScopeService p_employeeScopeService)
    {
        m_leaveRequestRepository = p_leaveRequestRepository;
        m_employeeProfileRepository = p_employeeProfileRepository;
        m_employeeScopeService = p_employeeScopeService;
    }

    public async Task<IEnumerable<LeaveRequestResponseDto>> GetAllAsync(string p_userId)
    {
        await m_leaveRequestRepository.SoftDeleteExpiredAsync(DateOnly.FromDateTime(DateTime.UtcNow));

        IEnumerable<LeaveRequest> leaveRequests;

        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.LeaveRequest))
        {
            leaveRequests = await m_leaveRequestRepository.GetAllAsync();
        }
        else
        {
            int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
            if (!profileId.HasValue)
            {
                return [];
            }

            leaveRequests = await m_leaveRequestRepository.GetByEmployeeProfileIdAsync(profileId.Value);
        }

        return leaveRequests.Select(MapToDto);
    }

    public async Task<LeaveRequestResponseDto?> GetByIdAsync(int p_id, string p_userId)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        LeaveRequest? leaveRequest = await m_leaveRequestRepository.GetByIdAsync(p_id);

        if (leaveRequest is null)
        {
            return null;
        }

        if (!await CanAccessLeaveRequestAsync(leaveRequest, p_userId))
        {
            return null;
        }

        return MapToDto(leaveRequest);
    }

    public async Task<LeaveRequestResponseDto> CreateAsync(CreateLeaveRequestDto p_request, string p_userId)
    {
        if (!await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.LeaveRequest))
        {
            int? ownProfileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
            if (!ownProfileId.HasValue)
            {
                throw new InvalidOperationException(ErrorMessages.EmployeeProfile.NotLinkedToAccount);
            }

            p_request.EmployeeProfileId = ownProfileId.Value;
        }

        await EnsureEmployeeProfileExistsAsync(p_request.EmployeeProfileId);
        ValidateDateRange(p_request.StartDate, p_request.EndDate);
        await EnsureNoOverlappingLeaveAsync(
            p_request.EmployeeProfileId,
            p_request.StartDate,
            p_request.EndDate,
            null);

        LeaveRequest leaveRequest = new LeaveRequest
        {
            EmployeeProfileId = p_request.EmployeeProfileId,
            LeaveType = p_request.LeaveType,
            Status = LeaveRequestStatus.Pending,
            StartDate = p_request.StartDate,
            EndDate = p_request.EndDate,
            Reason = p_request.Reason,
            IsDeleted = false
        };

        await m_leaveRequestRepository.AddAsync(leaveRequest);
        await m_leaveRequestRepository.SaveChangesAsync();

        LeaveRequest? createdLeaveRequest = await m_leaveRequestRepository.GetByIdAsync(leaveRequest.Id);
        if (createdLeaveRequest is null)
        {
            throw new InvalidOperationException(ErrorMessages.LeaveRequest.CreateRetrievalFailed);
        }

        return MapToDto(createdLeaveRequest);
    }

    public async Task<LeaveRequestResponseDto> UpdateStatusAsync(int p_id, UpdateLeaveRequestStatusDto p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        LeaveRequest? existingLeaveRequest = await m_leaveRequestRepository.GetTrackedByIdAsync(p_id);
        if (existingLeaveRequest is null)
        {
            throw new KeyNotFoundException(ErrorMessages.LeaveRequest.NotFound);
        }

        ValidateStatusTransition(existingLeaveRequest.Status, p_request.Status);

        existingLeaveRequest.Status = p_request.Status;

        await m_leaveRequestRepository.UpdateAsync(existingLeaveRequest);
        await m_leaveRequestRepository.SaveChangesAsync();

        LeaveRequest? updatedLeaveRequest = await m_leaveRequestRepository.GetByIdAsync(p_id);
        if (updatedLeaveRequest is null)
        {
            throw new InvalidOperationException(ErrorMessages.LeaveRequest.StatusUpdateRetrievalFailed);
        }

        return MapToDto(updatedLeaveRequest);
    }

    public async Task DeleteAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        LeaveRequest? existingLeaveRequest = await m_leaveRequestRepository.GetTrackedByIdAsync(p_id);
        if (existingLeaveRequest is null)
        {
            throw new KeyNotFoundException(ErrorMessages.LeaveRequest.NotFound);
        }

        await m_leaveRequestRepository.SoftDeleteAsync(existingLeaveRequest);
        await m_leaveRequestRepository.SaveChangesAsync();
    }

    private async Task<bool> CanAccessLeaveRequestAsync(LeaveRequest p_leaveRequest, string p_userId)
    {
        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.LeaveRequest))
        {
            return true;
        }

        int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
        return profileId.HasValue && p_leaveRequest.EmployeeProfileId == profileId.Value;
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

    private static void ValidateDateRange(DateOnly p_startDate, DateOnly p_endDate)
    {
        if (p_endDate < p_startDate)
        {
            throw new ArgumentException(ErrorMessages.LeaveRequest.EndDateBeforeStartDate);
        }
    }

    private async Task EnsureNoOverlappingLeaveAsync(
        int p_employeeProfileId,
        DateOnly p_startDate,
        DateOnly p_endDate,
        int? p_excludeRequestId)
    {
        bool hasOverlap = await m_leaveRequestRepository.HasOverlappingLeaveAsync(
            p_employeeProfileId,
            p_startDate,
            p_endDate,
            p_excludeRequestId);

        if (hasOverlap)
        {
            throw new InvalidOperationException(ErrorMessages.LeaveRequest.OverlappingPeriod);
        }
    }

    private static void ValidateStatusTransition(LeaveRequestStatus p_currentStatus, LeaveRequestStatus p_newStatus)
    {
        if (p_currentStatus == p_newStatus)
        {
            return;
        }

        if (p_currentStatus != LeaveRequestStatus.Pending)
        {
            throw new InvalidOperationException(ErrorMessages.LeaveRequest.OnlyPendingCanBeApprovedOrRejected);
        }

        if (p_newStatus != LeaveRequestStatus.Approved && p_newStatus != LeaveRequestStatus.Rejected)
        {
            throw new InvalidOperationException(ErrorMessages.LeaveRequest.InvalidPendingStatusTransition);
        }
    }

    private static LeaveRequestResponseDto MapToDto(LeaveRequest p_leaveRequest)
    {
        return new LeaveRequestResponseDto
        {
            Id = p_leaveRequest.Id,
            EmployeeProfileId = p_leaveRequest.EmployeeProfileId,
            EmployeeFirstName = p_leaveRequest.EmployeeProfile.FirstName,
            EmployeeLastName = p_leaveRequest.EmployeeProfile.LastName,
            LeaveType = p_leaveRequest.LeaveType.ToString(),
            Status = p_leaveRequest.Status.ToString(),
            StartDate = p_leaveRequest.StartDate,
            EndDate = p_leaveRequest.EndDate,
            Reason = p_leaveRequest.Reason
        };
    }
}
