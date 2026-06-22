using Crystal.Core.Constants;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;

namespace Crystal.Infrastructure.Services;

public class PunchEligibilityService : IPunchEligibilityService
{
    private readonly ITimeEntryRepository m_timeEntryRepository;
    private readonly IScheduledShiftRepository m_scheduledShiftRepository;
    private readonly IEmployeeScopeService m_employeeScopeService;

    public PunchEligibilityService(
        ITimeEntryRepository p_timeEntryRepository,
        IScheduledShiftRepository p_scheduledShiftRepository,
        IEmployeeScopeService p_employeeScopeService)
    {
        m_timeEntryRepository = p_timeEntryRepository;
        m_scheduledShiftRepository = p_scheduledShiftRepository;
        m_employeeScopeService = p_employeeScopeService;
    }

    public async Task<PunchEligibilityDto> EvaluateAsync(string p_userId)
    {
        int? profileId = await m_employeeScopeService.GetEmployeeProfileIdAsync(p_userId);
        if (!profileId.HasValue)
        {
            return new PunchEligibilityDto
            {
                CanPunchIn = false,
                CanPunchOut = false,
                BlockCode = PunchEligibilityBlockCodes.NoProfile,
                BlockedReason = ErrorMessages.PunchEligibility.AccountNotLinkedToProfile
            };
        }

        TimeEntry? activeEntry =
            await m_timeEntryRepository.GetActiveOpenByEmployeeProfileIdAsync(profileId.Value);
        if (activeEntry is not null)
        {
            return BuildActiveEntryEligibility(activeEntry);
        }

        return await EvaluatePunchInEligibilityAsync(profileId.Value);
    }

    public async Task EnsurePunchInAllowedAsync(string p_userId)
    {
        PunchEligibilityDto eligibility = await EvaluateAsync(p_userId);

        if (eligibility.ActiveEntryId.HasValue)
        {
            throw new InvalidOperationException(ErrorMessages.TimeEntry.PunchAlreadyInProgress);
        }

        if (!eligibility.CanPunchIn)
        {
            throw new InvalidOperationException(
                eligibility.BlockedReason ?? ErrorMessages.PunchEligibility.PunchInNotAllowedNow);
        }
    }

    private static PunchEligibilityDto BuildActiveEntryEligibility(TimeEntry p_activeEntry)
    {
        return new PunchEligibilityDto
        {
            CanPunchIn = false,
            CanPunchOut = true,
            ActiveEntryId = p_activeEntry.Id,
            ActiveEntryStartTime = p_activeEntry.StartTime,
            ScheduledShiftId = p_activeEntry.ScheduledShiftId
        };
    }

    private async Task<PunchEligibilityDto> EvaluatePunchInEligibilityAsync(int p_employeeProfileId)
    {
        DateOnly today = BusinessClock.Today;
        TimeOnly currentTime = BusinessClock.CurrentTime;

        ScheduledShift? todayShift =
            await m_scheduledShiftRepository.GetByEmployeeProfileIdAndDateAsync(p_employeeProfileId, today);

        PunchEligibilityDto eligibility = new PunchEligibilityDto
        {
            CanPunchIn = true,
            CanPunchOut = false
        };

        if (todayShift is null)
        {
            eligibility.CanPunchIn = false;
            eligibility.BlockCode = PunchEligibilityBlockCodes.NoShift;
            eligibility.BlockedReason = ErrorMessages.PunchEligibility.NoShiftScheduledToday;
            return eligibility;
        }

        eligibility.ScheduledShiftId = todayShift.Id;
        eligibility.ShiftDate = todayShift.Date;
        eligibility.ShiftStartTime = todayShift.StartTime;
        eligibility.ShiftEndTime = todayShift.EndTime;

        TimeOnly earliestAllowed = todayShift.StartTime.AddMinutes(-TimeAttendancePolicy.EarlyPunchToleranceMinutes);
        eligibility.EarliestPunchInTime = earliestAllowed;

        if (currentTime < earliestAllowed)
        {
            eligibility.CanPunchIn = false;
            eligibility.BlockCode = PunchEligibilityBlockCodes.TooEarly;
            eligibility.BlockedReason = string.Format(
                ErrorMessages.PunchEligibility.PunchInOpensAt,
                FormatTime(earliestAllowed),
                FormatTime(todayShift.StartTime));
            return eligibility;
        }

        DateTime now = BusinessClock.NowInBusinessZone;
        DateTime shiftEndOnDate = todayShift.Date.ToDateTime(todayShift.EndTime);
        DateTime latestAllowed = shiftEndOnDate.AddMinutes(TimeAttendancePolicy.LatePunchGraceMinutes);

        if (now > latestAllowed)
        {
            eligibility.CanPunchIn = false;
            eligibility.BlockCode = PunchEligibilityBlockCodes.TooLate;
            eligibility.BlockedReason = string.Format(
                ErrorMessages.PunchEligibility.PunchInClosedAt,
                FormatTime(TimeOnly.FromDateTime(latestAllowed)),
                FormatTime(todayShift.EndTime));
        }

        return eligibility;
    }

    private static string FormatTime(TimeOnly p_time)
    {
        return p_time.ToString("HH\\:mm");
    }
}
