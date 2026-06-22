using Crystal.Core.Authorization;
using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Crystal.Infrastructure.Services.Validation;

namespace Crystal.Infrastructure.Services;

public class TimeEntryService : ITimeEntryService
{
    private readonly ITimeEntryRepository m_timeEntryRepository;
    private readonly IEmployeeProfileRepository m_employeeProfileRepository;
    private readonly IScheduledShiftRepository m_scheduledShiftRepository;
    private readonly IEmployeeScopeService m_employeeScopeService;
    private readonly IPunchEligibilityService m_punchEligibilityService;

    public TimeEntryService(
        ITimeEntryRepository p_timeEntryRepository,
        IEmployeeProfileRepository p_employeeProfileRepository,
        IScheduledShiftRepository p_scheduledShiftRepository,
        IEmployeeScopeService p_employeeScopeService,
        IPunchEligibilityService p_punchEligibilityService)
    {
        m_timeEntryRepository = p_timeEntryRepository;
        m_employeeProfileRepository = p_employeeProfileRepository;
        m_scheduledShiftRepository = p_scheduledShiftRepository;
        m_employeeScopeService = p_employeeScopeService;
        m_punchEligibilityService = p_punchEligibilityService;
    }

    public async Task<IEnumerable<TimeEntryResponseDto>> GetAllAsync(string p_userId)
    {
        IEnumerable<TimeEntry> timeEntries;

        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.TimeEntry))
        {
            timeEntries = await m_timeEntryRepository.GetAllAsync();
        }
        else
        {
            int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
            if (!profileId.HasValue)
            {
                return [];
            }

            timeEntries = await m_timeEntryRepository.GetByEmployeeProfileIdAsync(profileId.Value);
        }

        return timeEntries.Select(MapToDto);
    }

    public async Task<TimeEntryResponseDto?> GetByIdAsync(int p_id, string p_userId)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        TimeEntry? timeEntry = await m_timeEntryRepository.GetByIdAsync(p_id);

        if (timeEntry is null)
        {
            return null;
        }

        if (!await CanAccessTimeEntryAsync(timeEntry, p_userId))
        {
            return null;
        }

        return MapToDto(timeEntry);
    }

    public async Task<TimeEntryResponseDto> CreateAsync(CreateTimeEntryRequest p_request, string p_userId)
    {
        if (!await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.TimeEntry))
        {
            int? ownProfileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
            if (!ownProfileId.HasValue)
            {
                throw new InvalidOperationException(ErrorMessages.EmployeeProfile.NotLinkedToAccount);
            }

            p_request.EmployeeProfileId = ownProfileId.Value;
        }

        ValidateEndTime(p_request.StartTime, p_request.EndTime);
        await EnsureEmployeeProfileExistsAsync(p_request.EmployeeProfileId);
        await EnsureScheduledShiftExistsAsync(p_request.ScheduledShiftId);

        TimeEntry timeEntry = new TimeEntry
        {
            EmployeeProfileId = p_request.EmployeeProfileId,
            ScheduledShiftId = p_request.ScheduledShiftId,
            Date = p_request.Date,
            StartTime = p_request.StartTime,
            EndTime = p_request.EndTime,
            IsDeleted = false
        };

        await m_timeEntryRepository.AddAsync(timeEntry);
        await m_timeEntryRepository.SaveChangesAsync();

        TimeEntry? createdEntry = await m_timeEntryRepository.GetByIdAsync(timeEntry.Id);
        if (createdEntry is null)
        {
            throw new InvalidOperationException(ErrorMessages.TimeEntry.CreateRetrievalFailed);
        }

        return MapToDto(createdEntry);
    }

    public async Task<TimeEntryResponseDto> UpdateAsync(int p_id, UpdateTimeEntryRequest p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        ValidateEndTime(p_request.StartTime, p_request.EndTime);
        await EnsureEmployeeProfileExistsAsync(p_request.EmployeeProfileId);
        await EnsureScheduledShiftBelongsToEmployeeAsync(p_request.ScheduledShiftId, p_request.EmployeeProfileId);

        TimeEntry? existingEntry = await m_timeEntryRepository.GetTrackedByIdAsync(p_id);
        if (existingEntry is null)
        {
            throw new KeyNotFoundException(ErrorMessages.TimeEntry.NotFound);
        }

        existingEntry.EmployeeProfileId = p_request.EmployeeProfileId;
        existingEntry.ScheduledShiftId = p_request.ScheduledShiftId;
        existingEntry.Date = p_request.Date;
        existingEntry.StartTime = p_request.StartTime;
        existingEntry.EndTime = p_request.EndTime;

        await m_timeEntryRepository.UpdateAsync(existingEntry);
        await m_timeEntryRepository.SaveChangesAsync();

        TimeEntry? updatedEntry = await m_timeEntryRepository.GetByIdAsync(p_id);
        if (updatedEntry is null)
        {
            throw new InvalidOperationException(ErrorMessages.TimeEntry.UpdateRetrievalFailed);
        }

        return MapToDto(updatedEntry);
    }

    public async Task DeleteAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        TimeEntry? existingEntry = await m_timeEntryRepository.GetTrackedByIdAsync(p_id);
        if (existingEntry is null)
        {
            throw new KeyNotFoundException(ErrorMessages.TimeEntry.NotFound);
        }

        await m_timeEntryRepository.SoftDeleteAsync(existingEntry);
        await m_timeEntryRepository.SaveChangesAsync();
    }

    public async Task<TimeEntryResponseDto?> GetActiveAsync(string p_userId)
    {
        int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
        if (!profileId.HasValue)
        {
            return null;
        }

        TimeEntry? activeEntry = await m_timeEntryRepository.GetActiveOpenByEmployeeProfileIdAsync(profileId.Value);
        if (activeEntry is null)
        {
            return null;
        }

        return MapToDto(activeEntry);
    }

    public async Task<TimeEntryResponseDto> PunchInAsync(string p_userId)
    {
        await m_punchEligibilityService.EnsurePunchInAllowedAsync(p_userId);

        int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
        if (!profileId.HasValue)
        {
            throw new InvalidOperationException(ErrorMessages.EmployeeProfile.NotLinkedToAccount);
        }

        DateOnly today = BusinessClock.Today;
        TimeOnly startTime = BusinessClock.CurrentTime;

        ScheduledShift? todayShift =
            await m_scheduledShiftRepository.GetByEmployeeProfileIdAndDateAsync(profileId.Value, today);

        TimeEntry timeEntry = new TimeEntry
        {
            EmployeeProfileId = profileId.Value,
            ScheduledShiftId = todayShift?.Id,
            Date = today,
            StartTime = startTime,
            EndTime = null,
            IsDeleted = false
        };

        await m_timeEntryRepository.AddAsync(timeEntry);
        await m_timeEntryRepository.SaveChangesAsync();

        TimeEntry? createdEntry = await m_timeEntryRepository.GetByIdAsync(timeEntry.Id);
        if (createdEntry is null)
        {
            throw new InvalidOperationException(ErrorMessages.TimeEntry.CreateRetrievalFailed);
        }

        return MapToDto(createdEntry);
    }

    public async Task<TimeEntryResponseDto> PunchOutAsync(string p_userId)
    {
        int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
        if (!profileId.HasValue)
        {
            throw new InvalidOperationException(ErrorMessages.EmployeeProfile.NotLinkedToAccount);
        }

        TimeEntry? activeEntry =
            await m_timeEntryRepository.GetTrackedActiveOpenByEmployeeProfileIdAsync(profileId.Value);
        if (activeEntry is null)
        {
            throw new InvalidOperationException(ErrorMessages.TimeEntry.NoOpenPunchToClose);
        }

        TimeOnly endTime = BusinessClock.CurrentTime;
        ValidateEndTime(activeEntry.StartTime, endTime);

        activeEntry.EndTime = endTime;

        await m_timeEntryRepository.UpdateAsync(activeEntry);
        await m_timeEntryRepository.SaveChangesAsync();

        TimeEntry? updatedEntry = await m_timeEntryRepository.GetByIdAsync(activeEntry.Id);
        if (updatedEntry is null)
        {
            throw new InvalidOperationException(ErrorMessages.TimeEntry.PunchOutRetrievalFailed);
        }

        return MapToDto(updatedEntry);
    }

    private async Task<bool> CanAccessTimeEntryAsync(TimeEntry p_timeEntry, string p_userId)
    {
        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.TimeEntry))
        {
            return true;
        }

        int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
        return profileId.HasValue && p_timeEntry.EmployeeProfileId == profileId.Value;
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

    private async Task EnsureScheduledShiftExistsAsync(int? p_scheduledShiftId)
    {
        await EnsureScheduledShiftBelongsToEmployeeAsync(p_scheduledShiftId, p_employeeProfileId: null);
    }

    private async Task EnsureScheduledShiftBelongsToEmployeeAsync(int? p_scheduledShiftId, int? p_employeeProfileId)
    {
        if (!p_scheduledShiftId.HasValue)
        {
            return;
        }

        EntityIdentifierValidator.EnsureValid(p_scheduledShiftId.Value);

        ScheduledShift? scheduledShift = await m_scheduledShiftRepository.GetByIdAsync(p_scheduledShiftId.Value);
        if (scheduledShift is null)
        {
            throw new InvalidOperationException(ErrorMessages.TimeEntry.ScheduledShiftNotFound);
        }

        if (!p_employeeProfileId.HasValue)
        {
            return;
        }

        if (!scheduledShift.EmployeeProfileId.HasValue
            || scheduledShift.EmployeeProfileId.Value != p_employeeProfileId.Value)
        {
            throw new InvalidOperationException(ErrorMessages.TimeEntry.ScheduledShiftEmployeeMismatch);
        }
    }

    private static void ValidateEndTime(TimeOnly p_startTime, TimeOnly? p_endTime)
    {
        if (p_endTime.HasValue && p_endTime.Value <= p_startTime)
        {
            throw new ArgumentException(ErrorMessages.TimeEntry.EndTimeBeforeStartTime);
        }
    }

    private static TimeEntryResponseDto MapToDto(TimeEntry p_timeEntry)
    {
        return new TimeEntryResponseDto
        {
            Id = p_timeEntry.Id,
            EmployeeProfileId = p_timeEntry.EmployeeProfileId,
            EmployeeFirstName = p_timeEntry.EmployeeProfile.FirstName,
            EmployeeLastName = p_timeEntry.EmployeeProfile.LastName,
            ScheduledShiftId = p_timeEntry.ScheduledShiftId,
            Date = p_timeEntry.Date,
            StartTime = p_timeEntry.StartTime,
            EndTime = p_timeEntry.EndTime
        };
    }
}
