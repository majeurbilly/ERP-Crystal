using Crystal.Core.Authorization;
using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Services.Validation;

namespace Crystal.Infrastructure.Services;

public class ScheduledShiftService : IScheduledShiftService
{
    private readonly IScheduledShiftRepository m_scheduledShiftRepository;
    private readonly IEmployeeProfileRepository m_employeeProfileRepository;
    private readonly IJobPositionRepository m_jobPositionRepository;
    private readonly ILocationRepository m_locationRepository;
    private readonly IEmployeeScopeService m_employeeScopeService;

    public ScheduledShiftService(
        IScheduledShiftRepository p_scheduledShiftRepository,
        IEmployeeProfileRepository p_employeeProfileRepository,
        IJobPositionRepository p_jobPositionRepository,
        ILocationRepository p_locationRepository,
        IEmployeeScopeService p_employeeScopeService)
    {
        m_scheduledShiftRepository = p_scheduledShiftRepository;
        m_employeeProfileRepository = p_employeeProfileRepository;
        m_jobPositionRepository = p_jobPositionRepository;
        m_locationRepository = p_locationRepository;
        m_employeeScopeService = p_employeeScopeService;
    }

    public async Task<IEnumerable<ScheduledShiftResponseDto>> GetAllAsync(string p_userId)
    {
        IEnumerable<ScheduledShift> scheduledShifts;

        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.ScheduledShift))
        {
            scheduledShifts = await m_scheduledShiftRepository.GetAllAsync();
        }
        else
        {
            int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
            if (!profileId.HasValue)
            {
                return [];
            }

            scheduledShifts = await m_scheduledShiftRepository.GetByEmployeeProfileIdAsync(profileId.Value);
        }

        return scheduledShifts.Select(MapToDto);
    }

    public async Task<IEnumerable<ScheduledShiftResponseDto>> GetTeamScheduleAsync(string p_userId)
    {
        if (!await m_employeeScopeService.CanReadAsync(p_userId, PermissionSubjects.ScheduledShift))
        {
            throw new UnauthorizedAccessException(ErrorMessages.AccessDenied);
        }

        IEnumerable<ScheduledShift> scheduledShifts = await m_scheduledShiftRepository.GetAllAsync();
        if (!await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.ScheduledShift))
        {
            int? locationId = await m_employeeScopeService.GetEmployeeLocationIdAsync(p_userId);
            if (!locationId.HasValue)
            {
                return [];
            }

            scheduledShifts = scheduledShifts.Where(p_shift =>
                p_shift.LocationId == locationId.Value);
        }

        return scheduledShifts.Select(MapToDto);
    }

    public async Task<ScheduledShiftResponseDto?> GetByIdAsync(int p_id, string p_userId)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        ScheduledShift? scheduledShift = await m_scheduledShiftRepository.GetByIdAsync(p_id);

        if (scheduledShift is null)
        {
            return null;
        }

        if (!await CanAccessShiftAsync(scheduledShift, p_userId))
        {
            return null;
        }

        return MapToDto(scheduledShift);
    }

    public async Task<ScheduledShiftResponseDto> CreateAsync(CreateScheduledShiftRequest p_request)
    {
        ValidateShiftTimes(p_request.StartTime, p_request.EndTime);
        await EnsureJobPositionExistsAsync(p_request.JobPositionId);
        await EnsureLocationExistsAsync(p_request.LocationId);
        EmployeeProfile? employeeProfile = await GetEmployeeProfileWhenProvidedAsync(
            p_request.EmployeeProfileId);
        EnsureEmployeeBelongsToLocation(employeeProfile, p_request.LocationId);

        ScheduledShift scheduledShift = new ScheduledShift
        {
            EmployeeProfileId = p_request.EmployeeProfileId,
            LocationId = p_request.LocationId,
            JobPositionId = p_request.JobPositionId,
            Date = p_request.Date,
            StartTime = p_request.StartTime,
            EndTime = p_request.EndTime,
            IsDeleted = false
        };

        await m_scheduledShiftRepository.AddAsync(scheduledShift);
        await m_scheduledShiftRepository.SaveChangesAsync();

        ScheduledShift? createdShift = await m_scheduledShiftRepository.GetByIdAsync(scheduledShift.Id);
        if (createdShift is null)
        {
            throw new InvalidOperationException(ErrorMessages.ScheduledShift.CreateRetrievalFailed);
        }

        return MapToDto(createdShift);
    }

    public async Task<ScheduledShiftResponseDto> UpdateAsync(int p_id, UpdateScheduledShiftRequest p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        ValidateShiftTimes(p_request.StartTime, p_request.EndTime);
        await EnsureJobPositionExistsAsync(p_request.JobPositionId);
        await EnsureLocationExistsAsync(p_request.LocationId);
        EmployeeProfile? employeeProfile = await GetEmployeeProfileWhenProvidedAsync(
            p_request.EmployeeProfileId);
        EnsureEmployeeBelongsToLocation(employeeProfile, p_request.LocationId);

        ScheduledShift? existingShift = await m_scheduledShiftRepository.GetTrackedByIdAsync(p_id);
        if (existingShift is null)
        {
            throw new KeyNotFoundException(ErrorMessages.ScheduledShift.NotFound);
        }

        existingShift.EmployeeProfileId = p_request.EmployeeProfileId;
        existingShift.LocationId = p_request.LocationId;
        existingShift.JobPositionId = p_request.JobPositionId;
        existingShift.Date = p_request.Date;
        existingShift.StartTime = p_request.StartTime;
        existingShift.EndTime = p_request.EndTime;

        await m_scheduledShiftRepository.UpdateAsync(existingShift);
        await m_scheduledShiftRepository.SaveChangesAsync();

        ScheduledShift? updatedShift = await m_scheduledShiftRepository.GetByIdAsync(p_id);
        if (updatedShift is null)
        {
            throw new InvalidOperationException(ErrorMessages.ScheduledShift.UpdateRetrievalFailed);
        }

        return MapToDto(updatedShift);
    }

    public async Task DeleteAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        ScheduledShift? existingShift = await m_scheduledShiftRepository.GetTrackedByIdAsync(p_id);
        if (existingShift is null)
        {
            throw new KeyNotFoundException(ErrorMessages.ScheduledShift.NotFound);
        }

        await m_scheduledShiftRepository.SoftDeleteAsync(existingShift);
        await m_scheduledShiftRepository.SaveChangesAsync();
    }

    private async Task<EmployeeProfile?> GetEmployeeProfileWhenProvidedAsync(int? p_employeeProfileId)
    {
        if (!p_employeeProfileId.HasValue)
        {
            return null;
        }

        EntityIdentifierValidator.EnsureValidEmployeeProfileId(p_employeeProfileId.Value);

        EmployeeProfile? employeeProfile = await m_employeeProfileRepository.GetByIdAsync(p_employeeProfileId.Value);
        if (employeeProfile is null)
        {
            throw new InvalidOperationException(ErrorMessages.EmployeeProfile.SpecifiedNotFound);
        }

        return employeeProfile;
    }

    private async Task EnsureJobPositionExistsAsync(int p_jobPositionId)
    {
        EntityIdentifierValidator.EnsureValid(p_jobPositionId);

        JobPosition? jobPosition = await m_jobPositionRepository.GetByIdAsync(p_jobPositionId);
        if (jobPosition is null)
        {
            throw new InvalidOperationException(ErrorMessages.ScheduledShift.JobPositionNotFound);
        }
    }

    private async Task EnsureLocationExistsAsync(int p_locationId)
    {
        EntityIdentifierValidator.EnsureValid(p_locationId);

        Location? location = await m_locationRepository.GetByIdAsync(p_locationId);
        if (location is null)
        {
            throw new InvalidOperationException(ErrorMessages.ScheduledShift.LocationNotFound);
        }
    }

    private static void EnsureEmployeeBelongsToLocation(
        EmployeeProfile? p_employeeProfile,
        int p_locationId)
    {
        if (p_employeeProfile is not null && p_employeeProfile.LocationId != p_locationId)
        {
            throw new InvalidOperationException(ErrorMessages.ScheduledShift.EmployeeNotInShiftLocation);
        }
    }

    private async Task<bool> CanAccessShiftAsync(ScheduledShift p_shift, string p_userId)
    {
        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.ScheduledShift))
        {
            return true;
        }

        int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
        return profileId.HasValue
            && p_shift.EmployeeProfileId.HasValue
            && p_shift.EmployeeProfileId.Value == profileId.Value;
    }

    private static void ValidateShiftTimes(TimeOnly p_startTime, TimeOnly p_endTime)
    {
        if (p_endTime <= p_startTime)
        {
            throw new ArgumentException(ErrorMessages.ScheduledShift.EndTimeBeforeStartTime);
        }
    }

    private static ScheduledShiftResponseDto MapToDto(ScheduledShift p_scheduledShift)
    {
        return new ScheduledShiftResponseDto
        {
            Id = p_scheduledShift.Id,
            EmployeeProfileId = p_scheduledShift.EmployeeProfileId,
            EmployeeFirstName = p_scheduledShift.EmployeeProfile?.FirstName,
            EmployeeLastName = p_scheduledShift.EmployeeProfile?.LastName,
            JobPositionId = p_scheduledShift.JobPositionId,
            JobPositionName = p_scheduledShift.JobPosition.Name,
            JobPositionColor = p_scheduledShift.JobPosition.Color,
            LocationId = p_scheduledShift.LocationId,
            LocationTitle = p_scheduledShift.Location?.Title,
            Date = p_scheduledShift.Date,
            StartTime = p_scheduledShift.StartTime,
            EndTime = p_scheduledShift.EndTime
        };
    }
}
