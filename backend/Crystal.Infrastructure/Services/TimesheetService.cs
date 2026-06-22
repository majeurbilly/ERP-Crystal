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

public class TimesheetService : ITimesheetService
{
    private readonly ITimesheetRepository m_timesheetRepository;
    private readonly IEmployeeProfileRepository m_employeeProfileRepository;
    private readonly ILocationRepository m_locationRepository;
    private readonly ITimeEntryRepository m_timeEntryRepository;
    private readonly IScheduledShiftRepository m_scheduledShiftRepository;
    private readonly IEmployeeScopeService m_employeeScopeService;

    public TimesheetService(
        ITimesheetRepository p_timesheetRepository,
        IEmployeeProfileRepository p_employeeProfileRepository,
        ILocationRepository p_locationRepository,
        ITimeEntryRepository p_timeEntryRepository,
        IScheduledShiftRepository p_scheduledShiftRepository,
        IEmployeeScopeService p_employeeScopeService)
    {
        m_timesheetRepository = p_timesheetRepository;
        m_employeeProfileRepository = p_employeeProfileRepository;
        m_locationRepository = p_locationRepository;
        m_timeEntryRepository = p_timeEntryRepository;
        m_scheduledShiftRepository = p_scheduledShiftRepository;
        m_employeeScopeService = p_employeeScopeService;
    }

    public async Task<IEnumerable<TimesheetResponseDto>> GetAllAsync(string p_userId)
    {
        IEnumerable<Timesheet> timesheets;

        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.Timesheet))
        {
            timesheets = await m_timesheetRepository.GetAllAsync();
        }
        else if (await m_employeeScopeService.CanSubmitAsync(p_userId, PermissionSubjects.Timesheet))
        {
            int? locationId = await m_employeeScopeService.GetEmployeeLocationIdAsync(p_userId);
            if (!locationId.HasValue)
            {
                return [];
            }

            IEnumerable<Timesheet> allTimesheets = await m_timesheetRepository.GetAllAsync();
            timesheets = allTimesheets.Where(p_timesheet => p_timesheet.EmployeeProfile.LocationId == locationId.Value);
        }
        else
        {
            int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
            if (!profileId.HasValue)
            {
                return [];
            }

            timesheets = await m_timesheetRepository.GetByEmployeeProfileIdAsync(profileId.Value);
        }

        return timesheets.Select(MapToDto);
    }

    public async Task<TimesheetResponseDto?> GetByIdAsync(int p_id, string p_userId)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Timesheet? timesheet = await m_timesheetRepository.GetByIdAsync(p_id);

        if (timesheet is null)
        {
            return null;
        }

        if (!await CanAccessTimesheetAsync(timesheet, p_userId))
        {
            return null;
        }

        return MapToDto(timesheet);
    }

    public async Task<TimesheetResponseDto> CreateAsync(CreateTimesheetRequest p_request)
    {
        ValidatePeriod(p_request.PeriodStart, p_request.PeriodEnd);
        await EnsureEmployeeProfileExistsAsync(p_request.EmployeeProfileId);

        Timesheet timesheet = new Timesheet
        {
            EmployeeProfileId = p_request.EmployeeProfileId,
            PeriodStart = p_request.PeriodStart,
            PeriodEnd = p_request.PeriodEnd,
            Status = TimesheetStatus.Draft,
            IsDeleted = false
        };

        await m_timesheetRepository.AddAsync(timesheet);
        await m_timesheetRepository.SaveChangesAsync();

        await LinkTimeEntriesAsync(timesheet.Id, p_request.EmployeeProfileId, p_request.PeriodStart, p_request.PeriodEnd, p_request.TimeEntryIds);

        Timesheet? createdTimesheet = await m_timesheetRepository.GetByIdAsync(timesheet.Id);
        if (createdTimesheet is null)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.CreateRetrievalFailed);
        }

        return MapToDto(createdTimesheet);
    }

    public async Task<GenerateWeeklyTimesheetsResponseDto> GenerateWeeklyAsync(
        GenerateWeeklyTimesheetsRequest p_request,
        string p_userId)
    {
        ValidatePeriodStart(p_request.PeriodStart);
        ValidateGeneratedWeekStartsOnMonday(p_request.PeriodStart);
        ValidateGeneratedWeekIsComplete(p_request.PeriodStart);
        int? locationId = await ResolveGenerationLocationIdAsync(p_request.LocationId, p_userId);
        ValidateLocationId(locationId);
        await EnsureLocationExistsAsync(locationId);

        DateOnly periodStart = p_request.PeriodStart;
        DateOnly periodEnd = periodStart.AddDays(6);

        IEnumerable<EmployeeProfile> employeeProfiles = await m_employeeProfileRepository.GetAllAsync();
        List<EmployeeProfile> activeEmployeeProfiles = employeeProfiles
            .Where(p_profile => string.Equals(p_profile.Status, "Active", StringComparison.OrdinalIgnoreCase))
            .Where(p_profile => !locationId.HasValue || p_profile.LocationId == locationId.Value)
            .ToList();

        HashSet<int> employeeProfileIds = activeEmployeeProfiles
            .Select(p_profile => p_profile.Id)
            .ToHashSet();

        IList<Timesheet> existingTimesheets = await m_timesheetRepository.GetByPeriodAsync(periodStart, periodEnd);
        List<Timesheet> relevantExistingTimesheets = existingTimesheets
            .Where(p_timesheet => employeeProfileIds.Contains(p_timesheet.EmployeeProfileId))
            .ToList();

        Dictionary<int, Timesheet> timesheetsByEmployeeId = relevantExistingTimesheets
            .GroupBy(p_timesheet => p_timesheet.EmployeeProfileId)
            .ToDictionary(p_group => p_group.Key, p_group => p_group.First());

        List<Timesheet> newTimesheets = activeEmployeeProfiles
            .Where(p_profile => !timesheetsByEmployeeId.ContainsKey(p_profile.Id))
            .Select(p_profile => new Timesheet
            {
                EmployeeProfileId = p_profile.Id,
                PeriodStart = periodStart,
                PeriodEnd = periodEnd,
                Status = TimesheetStatus.Draft,
                IsDeleted = false
            })
            .ToList();

        if (newTimesheets.Count > 0)
        {
            await m_timesheetRepository.AddRangeAsync(newTimesheets);
            await m_timesheetRepository.SaveChangesAsync();

            foreach (Timesheet timesheet in newTimesheets)
            {
                timesheetsByEmployeeId[timesheet.EmployeeProfileId] = timesheet;
            }
        }

        int linkedTimeEntryCount = await LinkUnlinkedWeeklyTimeEntriesAsync(periodStart, periodEnd, timesheetsByEmployeeId);
        IList<Timesheet> generatedTimesheets = await m_timesheetRepository.GetByPeriodAsync(periodStart, periodEnd);
        List<Timesheet> relevantGeneratedTimesheets = generatedTimesheets
            .Where(p_timesheet => employeeProfileIds.Contains(p_timesheet.EmployeeProfileId))
            .ToList();

        return new GenerateWeeklyTimesheetsResponseDto
        {
            PeriodStart = periodStart,
            PeriodEnd = periodEnd,
            LocationId = locationId,
            CreatedCount = newTimesheets.Count,
            ExistingCount = relevantExistingTimesheets.Count,
            LinkedTimeEntryCount = linkedTimeEntryCount,
            Timesheets = relevantGeneratedTimesheets
                .OrderBy(p_timesheet => p_timesheet.EmployeeProfile.LastName)
                .ThenBy(p_timesheet => p_timesheet.EmployeeProfile.FirstName)
                .Select(MapToDto)
                .ToList()
        };
    }

    public async Task<TimesheetResponseDto> UpdateAsync(int p_id, CreateTimesheetRequest p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        ValidatePeriod(p_request.PeriodStart, p_request.PeriodEnd);

        Timesheet? existingTimesheet = await m_timesheetRepository.GetTrackedByIdAsync(p_id);
        if (existingTimesheet is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Timesheet.NotFound);
        }

        if (existingTimesheet.Status != TimesheetStatus.Draft)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.OnlyDraftCanBeModified);
        }

        if (existingTimesheet.EmployeeProfileId != p_request.EmployeeProfileId)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.EmployeeCannotBeChanged);
        }

        await EnsureEmployeeProfileExistsAsync(p_request.EmployeeProfileId);

        existingTimesheet.PeriodStart = p_request.PeriodStart;
        existingTimesheet.PeriodEnd = p_request.PeriodEnd;

        await m_timesheetRepository.UpdateAsync(existingTimesheet);
        await m_timesheetRepository.SaveChangesAsync();

        await UnlinkAllTimeEntriesAsync(p_id);
        await LinkTimeEntriesAsync(p_id, p_request.EmployeeProfileId, p_request.PeriodStart, p_request.PeriodEnd, p_request.TimeEntryIds);

        Timesheet? updatedTimesheet = await m_timesheetRepository.GetByIdAsync(p_id);
        if (updatedTimesheet is null)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.UpdateRetrievalFailed);
        }

        return MapToDto(updatedTimesheet);
    }

    public async Task<TimesheetResponseDto> UpdateStatusAsync(
        int p_id,
        UpdateTimesheetStatusRequest p_request,
        string p_userId)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Timesheet? existingTimesheet = await m_timesheetRepository.GetTrackedByIdAsync(p_id);
        if (existingTimesheet is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Timesheet.NotFound);
        }

        if (!await CanAccessTimesheetAsync(existingTimesheet, p_userId))
        {
            throw new KeyNotFoundException(ErrorMessages.Timesheet.NotFound);
        }

        ValidateStatusTransition(existingTimesheet.Status, p_request.Status);

        existingTimesheet.Status = p_request.Status;

        await m_timesheetRepository.UpdateAsync(existingTimesheet);
        await m_timesheetRepository.SaveChangesAsync();

        Timesheet? updatedTimesheet = await m_timesheetRepository.GetByIdAsync(p_id);
        if (updatedTimesheet is null)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.StatusUpdateRetrievalFailed);
        }

        return MapToDto(updatedTimesheet);
    }

    public async Task<TimesheetResponseDto> UpdatePaidAsync(
        int p_id,
        UpdateTimesheetPaidRequest p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Timesheet? existingTimesheet = await m_timesheetRepository.GetTrackedByIdAsync(p_id);
        if (existingTimesheet is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Timesheet.NotFound);
        }

        existingTimesheet.IsPaid = p_request.IsPaid;

        await m_timesheetRepository.UpdateAsync(existingTimesheet);
        await m_timesheetRepository.SaveChangesAsync();

        Timesheet? updatedTimesheet = await m_timesheetRepository.GetByIdAsync(p_id);
        if (updatedTimesheet is null)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.PaidUpdateRetrievalFailed);
        }

        return MapToDto(updatedTimesheet);
    }

    public async Task<TimesheetResponseDto> ReloadTimeEntriesAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Timesheet? existingTimesheet = await m_timesheetRepository.GetTrackedByIdAsync(p_id);
        if (existingTimesheet is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Timesheet.NotFound);
        }

        if (existingTimesheet.Status != TimesheetStatus.Draft)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.OnlyDraftCanBeReloaded);
        }

        IList<TimeEntry> unlinkedEntries = await m_timeEntryRepository.GetTrackedUnlinkedByPeriodAsync(
            existingTimesheet.PeriodStart,
            existingTimesheet.PeriodEnd);

        foreach (TimeEntry entry in unlinkedEntries)
        {
            if (entry.EmployeeProfileId == existingTimesheet.EmployeeProfileId)
            {
                entry.TimesheetId = existingTimesheet.Id;
            }
        }

        await m_timeEntryRepository.SaveChangesAsync();

        Timesheet? updatedTimesheet = await m_timesheetRepository.GetByIdAsync(p_id);
        if (updatedTimesheet is null)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.ReloadRetrievalFailed);
        }

        return MapToDto(updatedTimesheet);
    }

    public async Task<TimesheetResponseDto> AddTimeEntryAsync(int p_id, CreateTimeEntryRequest p_request)
    {
        Timesheet timesheet = await GetTrackedDraftTimesheetAsync(p_id);

        ValidateTimeEntryForTimesheet(
            timesheet,
            p_request.EmployeeProfileId,
            p_request.Date,
            p_request.StartTime,
            p_request.EndTime);
        await EnsureScheduledShiftBelongsToEmployeeAsync(
            p_request.ScheduledShiftId,
            timesheet.EmployeeProfileId);

        TimeEntry timeEntry = new TimeEntry
        {
            EmployeeProfileId = timesheet.EmployeeProfileId,
            ScheduledShiftId = p_request.ScheduledShiftId,
            TimesheetId = timesheet.Id,
            Date = p_request.Date,
            StartTime = p_request.StartTime,
            EndTime = p_request.EndTime,
            IsDeleted = false
        };

        await m_timeEntryRepository.AddAsync(timeEntry);
        await m_timeEntryRepository.SaveChangesAsync();

        return await GetRequiredTimesheetDtoAsync(p_id, ErrorMessages.Timesheet.AddTimeEntryRetrievalFailed);
    }

    public async Task<TimesheetResponseDto> UpdateTimeEntryAsync(
        int p_id,
        int p_timeEntryId,
        UpdateTimeEntryRequest p_request)
    {
        EntityIdentifierValidator.EnsureValid(p_timeEntryId);

        Timesheet timesheet = await GetTrackedDraftTimesheetAsync(p_id);
        TimeEntry? existingEntry = await m_timeEntryRepository.GetTrackedByIdAsync(p_timeEntryId);
        if (existingEntry is null || existingEntry.TimesheetId != timesheet.Id)
        {
            throw new KeyNotFoundException(ErrorMessages.Timesheet.TimeEntryNotFoundOnTimesheet);
        }

        ValidateTimeEntryForTimesheet(
            timesheet,
            p_request.EmployeeProfileId,
            p_request.Date,
            p_request.StartTime,
            p_request.EndTime);
        await EnsureScheduledShiftBelongsToEmployeeAsync(
            p_request.ScheduledShiftId,
            timesheet.EmployeeProfileId);

        existingEntry.EmployeeProfileId = timesheet.EmployeeProfileId;
        existingEntry.ScheduledShiftId = p_request.ScheduledShiftId;
        existingEntry.Date = p_request.Date;
        existingEntry.StartTime = p_request.StartTime;
        existingEntry.EndTime = p_request.EndTime;

        await m_timeEntryRepository.UpdateAsync(existingEntry);
        await m_timeEntryRepository.SaveChangesAsync();

        return await GetRequiredTimesheetDtoAsync(p_id, ErrorMessages.Timesheet.UpdateTimeEntryRetrievalFailed);
    }

    public async Task<TimesheetResponseDto> RemoveTimeEntryAsync(int p_id, int p_timeEntryId)
    {
        EntityIdentifierValidator.EnsureValid(p_timeEntryId);

        Timesheet timesheet = await GetTrackedDraftTimesheetAsync(p_id);
        TimeEntry? existingEntry = await m_timeEntryRepository.GetTrackedByIdAsync(p_timeEntryId);
        if (existingEntry is null || existingEntry.TimesheetId != timesheet.Id)
        {
            throw new KeyNotFoundException(ErrorMessages.Timesheet.TimeEntryNotFoundOnTimesheet);
        }

        existingEntry.TimesheetId = null;

        await m_timeEntryRepository.UpdateAsync(existingEntry);
        await m_timeEntryRepository.SaveChangesAsync();

        return await GetRequiredTimesheetDtoAsync(p_id, ErrorMessages.Timesheet.RemoveTimeEntryRetrievalFailed);
    }

    public async Task DeleteAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Timesheet? existingTimesheet = await m_timesheetRepository.GetTrackedByIdAsync(p_id);
        if (existingTimesheet is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Timesheet.NotFound);
        }

        await UnlinkAllTimeEntriesAsync(p_id);

        await m_timesheetRepository.SoftDeleteAsync(existingTimesheet);
        await m_timesheetRepository.SaveChangesAsync();
    }

    private async Task<bool> CanAccessTimesheetAsync(Timesheet p_timesheet, string p_userId)
    {
        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.Timesheet))
        {
            return true;
        }

        if (await m_employeeScopeService.CanSubmitAsync(p_userId, PermissionSubjects.Timesheet))
        {
            int? locationId = await m_employeeScopeService.GetEmployeeLocationIdAsync(p_userId);
            return locationId.HasValue && p_timesheet.EmployeeProfile.LocationId == locationId.Value;
        }

        int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
        return profileId.HasValue && p_timesheet.EmployeeProfileId == profileId.Value;
    }

    private async Task<int?> ResolveGenerationLocationIdAsync(int? p_requestedLocationId, string p_userId)
    {
        if (await m_employeeScopeService.CanManageAsync(p_userId, PermissionSubjects.Timesheet))
        {
            return p_requestedLocationId;
        }

        if (!await m_employeeScopeService.CanSubmitAsync(p_userId, PermissionSubjects.Timesheet))
        {
            throw new UnauthorizedAccessException(ErrorMessages.AccessDenied);
        }

        int? userLocationId = await m_employeeScopeService.GetEmployeeLocationIdAsync(p_userId);
        if (!userLocationId.HasValue)
        {
            throw new UnauthorizedAccessException(ErrorMessages.Timesheet.NoLocationLinkedToUser);
        }

        if (p_requestedLocationId.HasValue && p_requestedLocationId.Value != userLocationId.Value)
        {
            throw new UnauthorizedAccessException(ErrorMessages.Timesheet.GenerateOnlyOwnLocation);
        }

        return userLocationId.Value;
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

    private async Task EnsureLocationExistsAsync(int? p_locationId)
    {
        if (!p_locationId.HasValue)
        {
            return;
        }

        Location? location = await m_locationRepository.GetByIdAsync(p_locationId.Value);
        if (location is null)
        {
            throw new InvalidOperationException(ErrorMessages.Location.NotFound);
        }
    }

    private async Task<Timesheet> GetTrackedDraftTimesheetAsync(int p_id)
    {
        EntityIdentifierValidator.EnsureValid(p_id);

        Timesheet? timesheet = await m_timesheetRepository.GetTrackedByIdAsync(p_id);
        if (timesheet is null)
        {
            throw new KeyNotFoundException(ErrorMessages.Timesheet.NotFound);
        }

        if (timesheet.Status != TimesheetStatus.Draft)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.OnlyDraftCanBeModified);
        }

        return timesheet;
    }

    private async Task<TimesheetResponseDto> GetRequiredTimesheetDtoAsync(int p_id, string p_errorMessage)
    {
        Timesheet? updatedTimesheet = await m_timesheetRepository.GetByIdAsync(p_id);
        if (updatedTimesheet is null)
        {
            throw new InvalidOperationException(p_errorMessage);
        }

        return MapToDto(updatedTimesheet);
    }

    private async Task EnsureScheduledShiftBelongsToEmployeeAsync(int? p_scheduledShiftId, int p_employeeProfileId)
    {
        if (!p_scheduledShiftId.HasValue)
        {
            return;
        }

        EntityIdentifierValidator.EnsureValid(p_scheduledShiftId.Value);

        ScheduledShift? scheduledShift = await m_scheduledShiftRepository.GetByIdAsync(p_scheduledShiftId.Value);
        if (scheduledShift is null)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.ScheduledShiftNotFound);
        }

        if (!scheduledShift.EmployeeProfileId.HasValue
            || scheduledShift.EmployeeProfileId.Value != p_employeeProfileId)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.ScheduledShiftEmployeeMismatch);
        }
    }

    private static void ValidatePeriod(DateOnly p_periodStart, DateOnly p_periodEnd)
    {
        ValidatePeriodStart(p_periodStart);

        if (p_periodEnd < p_periodStart)
        {
            throw new ArgumentException(ErrorMessages.Timesheet.PeriodEndBeforeStart);
        }
    }

    private static void ValidatePeriodStart(DateOnly p_periodStart)
    {
        if (p_periodStart == default)
        {
            throw new ArgumentException(ErrorMessages.Timesheet.PeriodStartRequired);
        }
    }

    private static void ValidateGeneratedWeekStartsOnMonday(DateOnly p_periodStart)
    {
        if (p_periodStart.DayOfWeek != DayOfWeek.Monday)
        {
            throw new ArgumentException(ErrorMessages.Timesheet.GeneratedWeekMustStartOnMonday);
        }
    }

    private static void ValidateGeneratedWeekIsComplete(DateOnly p_periodStart)
    {
        DateOnly periodEnd = p_periodStart.AddDays(6);
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);

        if (periodEnd >= today)
        {
            throw new ArgumentException(ErrorMessages.Timesheet.GeneratedWeekMustBeComplete);
        }
    }

    private static void ValidateLocationId(int? p_locationId)
    {
        if (p_locationId.HasValue)
        {
            EntityIdentifierValidator.EnsureValid(p_locationId.Value);
        }
    }

    private static void ValidateStatusTransition(TimesheetStatus p_currentStatus, TimesheetStatus p_newStatus)
    {
        if (p_currentStatus == p_newStatus)
        {
            return;
        }

        bool isValidTransition = (p_currentStatus, p_newStatus) switch
        {
            (TimesheetStatus.Draft, TimesheetStatus.Submitted) => true,
            (TimesheetStatus.Submitted, TimesheetStatus.Approved) => true,
            (TimesheetStatus.Submitted, TimesheetStatus.Rejected) => true,
            (TimesheetStatus.Rejected, TimesheetStatus.Submitted) => true,
            _ => false
        };

        if (!isValidTransition)
        {
            string message = p_currentStatus switch
            {
                TimesheetStatus.Draft when p_newStatus == TimesheetStatus.Approved =>
                    ErrorMessages.Timesheet.CannotApproveDraftTimesheet,
                TimesheetStatus.Draft when p_newStatus == TimesheetStatus.Rejected =>
                    ErrorMessages.Timesheet.CannotRejectDraftTimesheet,
                TimesheetStatus.Approved =>
                    ErrorMessages.Timesheet.ApprovedTimesheetCannotChangeStatus,
                _ => string.Format(ErrorMessages.Timesheet.InvalidStatusTransition, p_currentStatus, p_newStatus)
            };

            throw new InvalidOperationException(message);
        }
    }

    private static void ValidateTimeEntryForTimesheet(
        Timesheet p_timesheet,
        int p_employeeProfileId,
        DateOnly p_date,
        TimeOnly p_startTime,
        TimeOnly? p_endTime)
    {
        if (p_employeeProfileId != p_timesheet.EmployeeProfileId)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.TimeEntryMustBelongToTimesheetEmployee);
        }

        if (p_date < p_timesheet.PeriodStart || p_date > p_timesheet.PeriodEnd)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.TimeEntryMustBeWithinTimesheetPeriod);
        }

        if (p_endTime.HasValue && p_endTime.Value <= p_startTime)
        {
            throw new ArgumentException(ErrorMessages.Timesheet.EndTimeBeforeStartTime);
        }
    }

    private async Task LinkTimeEntriesAsync(
        int p_timesheetId,
        int p_employeeProfileId,
        DateOnly p_periodStart,
        DateOnly p_periodEnd,
        IList<int> p_timeEntryIds)
    {
        if (p_timeEntryIds.Count == 0)
        {
            return;
        }

        IList<TimeEntry> entries = await m_timeEntryRepository.GetTrackedByIdsAsync(p_timeEntryIds);

        if (entries.Count != p_timeEntryIds.Count)
        {
            throw new InvalidOperationException(ErrorMessages.Timesheet.TimeEntriesNotFound);
        }

        foreach (TimeEntry entry in entries)
        {
            if (entry.EmployeeProfileId != p_employeeProfileId)
            {
                throw new InvalidOperationException(ErrorMessages.Timesheet.TimeEntriesMustBelongToEmployee);
            }

            if (entry.TimesheetId.HasValue && entry.TimesheetId.Value != p_timesheetId)
            {
                throw new InvalidOperationException(ErrorMessages.Timesheet.TimeEntriesAlreadyLinked);
            }

            if (entry.Date < p_periodStart || entry.Date > p_periodEnd)
            {
                throw new InvalidOperationException(ErrorMessages.Timesheet.TimeEntriesOutsidePeriod);
            }

            entry.TimesheetId = p_timesheetId;
        }

        await m_timeEntryRepository.SaveChangesAsync();
    }

    private async Task UnlinkAllTimeEntriesAsync(int p_timesheetId)
    {
        IList<TimeEntry> linkedEntries = await m_timeEntryRepository.GetTrackedByTimesheetIdAsync(p_timesheetId);

        foreach (TimeEntry entry in linkedEntries)
        {
            entry.TimesheetId = null;
        }

        if (linkedEntries.Count > 0)
        {
            await m_timeEntryRepository.SaveChangesAsync();
        }
    }

    private async Task<int> LinkUnlinkedWeeklyTimeEntriesAsync(
        DateOnly p_periodStart,
        DateOnly p_periodEnd,
        IReadOnlyDictionary<int, Timesheet> p_timesheetsByEmployeeId)
    {
        IList<TimeEntry> unlinkedEntries = await m_timeEntryRepository.GetTrackedUnlinkedByPeriodAsync(p_periodStart, p_periodEnd);
        int linkedCount = 0;

        foreach (TimeEntry entry in unlinkedEntries)
        {
            if (!p_timesheetsByEmployeeId.TryGetValue(entry.EmployeeProfileId, out Timesheet? timesheet))
            {
                continue;
            }

            if (timesheet.Status != TimesheetStatus.Draft)
            {
                continue;
            }

            entry.TimesheetId = timesheet.Id;
            linkedCount++;
        }

        if (linkedCount > 0)
        {
            await m_timeEntryRepository.SaveChangesAsync();
        }

        return linkedCount;
    }

    private static TimesheetResponseDto MapToDto(Timesheet p_timesheet)
    {
        List<TimeEntryResponseDto> timeEntryDtos = p_timesheet.TimeEntries
            .Select(MapTimeEntryToDto)
            .ToList();

        return new TimesheetResponseDto
        {
            Id = p_timesheet.Id,
            EmployeeProfileId = p_timesheet.EmployeeProfileId,
            EmployeeFirstName = p_timesheet.EmployeeProfile.FirstName,
            EmployeeLastName = p_timesheet.EmployeeProfile.LastName,
            PeriodStart = p_timesheet.PeriodStart,
            PeriodEnd = p_timesheet.PeriodEnd,
            Status = p_timesheet.Status.ToString(),
            IsPaid = p_timesheet.IsPaid,
            TimeEntries = timeEntryDtos
        };
    }

    private static TimeEntryResponseDto MapTimeEntryToDto(TimeEntry p_timeEntry)
    {
        return new TimeEntryResponseDto
        {
            Id = p_timeEntry.Id,
            EmployeeProfileId = p_timeEntry.EmployeeProfileId,
            EmployeeFirstName = p_timeEntry.EmployeeProfile?.FirstName ?? string.Empty,
            EmployeeLastName = p_timeEntry.EmployeeProfile?.LastName ?? string.Empty,
            ScheduledShiftId = p_timeEntry.ScheduledShiftId,
            Date = p_timeEntry.Date,
            StartTime = p_timeEntry.StartTime,
            EndTime = p_timeEntry.EndTime
        };
    }
}
