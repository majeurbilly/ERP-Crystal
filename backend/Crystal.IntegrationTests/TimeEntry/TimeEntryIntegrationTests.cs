using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Crystal.IntegrationTests.TimeEntry;

[Collection("TimeEntryIntegrationTests")]
public sealed class TimeEntryIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public TimeEntryIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns200OK_WithEmployeeToken()
    {
        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/time-entries");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Returns404_WhenEmployeeAccessesAnotherEntry()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync(p_includeScheduledShift: true);
        CreateTimeEntryRequest createRequest = BuildCreateRequest(
            seedResult,
            seedResult.ScheduledShiftId,
            new DateOnly(2026, 10, 1),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/time-entries", createRequest);
        createResponse.EnsureSuccessStatusCode();

        TimeEntryResponseDto? created = await createResponse.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
        Assert.NotNull(created);

        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage getResponse = await m_client.GetAsync($"/api/time-entries/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created_WithEmployeeToken_WhenOwnProfileLinked()
    {
        int employeeProfileId = await EnsureEmployeeProfileForEmployeeUserAsync();
        await AuthenticateAsEmployeeAsync();

        CreateTimeEntryRequest request = new CreateTimeEntryRequest
        {
            EmployeeProfileId = employeeProfileId,
            ScheduledShiftId = null,
            Date = new DateOnly(2026, 10, 3),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0)
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/time-entries", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created_WithAdminToken_WithoutScheduledShift()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync(p_includeScheduledShift: false);
        CreateTimeEntryRequest request = BuildCreateRequest(
            seedResult,
            null,
            new DateOnly(2026, 10, 2),
            new TimeOnly(8, 0),
            null);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/time-entries", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        TimeEntryResponseDto? body = await response.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Null(body.EndTime);
        Assert.Null(body.ScheduledShiftId);
        Assert.Equal(seedResult.EmployeeFirstName, body.EmployeeFirstName);
    }

    [Fact]
    public async Task Create_Returns400BadRequest_WhenEndTimeIsNotAfterStartTime()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync(p_includeScheduledShift: false);
        CreateTimeEntryRequest request = BuildCreateRequest(
            seedResult,
            null,
            new DateOnly(2026, 10, 4),
            new TimeOnly(17, 0),
            new TimeOnly(9, 0));

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/time-entries", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal(ErrorMessages.TimeEntry.EndTimeBeforeStartTime, message);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenEmployeeProfileDoesNotExist()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync(p_includeScheduledShift: false);
        CreateTimeEntryRequest request = BuildCreateRequest(
            seedResult,
            null,
            new DateOnly(2026, 10, 5),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));
        request.EmployeeProfileId = 999_999;

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/time-entries", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal("The specified employee profile was not found.", message);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenScheduledShiftDoesNotExist()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync(p_includeScheduledShift: false);
        CreateTimeEntryRequest request = BuildCreateRequest(
            seedResult,
            999_999,
            new DateOnly(2026, 10, 6),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/time-entries", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal("The specified scheduled shift was not found.", message);
    }

    [Fact]
    public async Task Update_Returns200OK_WithAdminToken()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync(p_includeScheduledShift: true);
        CreateTimeEntryRequest createRequest = BuildCreateRequest(
            seedResult,
            seedResult.ScheduledShiftId,
            new DateOnly(2026, 10, 7),
            new TimeOnly(9, 0),
            null);

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/time-entries", createRequest);
        createResponse.EnsureSuccessStatusCode();

        TimeEntryResponseDto? created = await createResponse.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
        Assert.NotNull(created);

        UpdateTimeEntryRequest updateRequest = new()
        {
            EmployeeProfileId = seedResult.EmployeeProfileId,
            ScheduledShiftId = seedResult.ScheduledShiftId,
            Date = new DateOnly(2026, 10, 7),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 30)
        };

        HttpResponseMessage updateResponse = await m_client.PutAsJsonAsync(
            $"/api/time-entries/{created.Id}",
            updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        TimeEntryResponseDto? updated = await updateResponse.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
        Assert.NotNull(updated);
        Assert.Equal(new TimeOnly(17, 30), updated.EndTime);
    }

    [Fact]
    public async Task Update_Returns409Conflict_WhenScheduledShiftBelongsToAnotherEmployee()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync(p_includeScheduledShift: true);
        HrSeedResult otherEmployeeSeed = await SeedHrReferenceDataAsync(p_includeScheduledShift: true);

        CreateTimeEntryRequest createRequest = BuildCreateRequest(
            seedResult,
            seedResult.ScheduledShiftId,
            new DateOnly(2026, 10, 8),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/time-entries", createRequest);
        createResponse.EnsureSuccessStatusCode();

        TimeEntryResponseDto? created = await createResponse.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
        Assert.NotNull(created);

        UpdateTimeEntryRequest updateRequest = new()
        {
            EmployeeProfileId = seedResult.EmployeeProfileId,
            ScheduledShiftId = otherEmployeeSeed.ScheduledShiftId,
            Date = new DateOnly(2026, 10, 8),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0)
        };

        HttpResponseMessage updateResponse = await m_client.PutAsJsonAsync(
            $"/api/time-entries/{created.Id}",
            updateRequest);

        Assert.Equal(HttpStatusCode.Conflict, updateResponse.StatusCode);

        JsonDocument document = JsonDocument.Parse(await updateResponse.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal("The scheduled shift does not match the time entry employee.", message);
    }

    [Fact]
    public async Task GetActive_Returns200OK_WithNullBody_WhenNoOpenEntry()
    {
        await EnsureEmployeeProfileForEmployeeUserAsync();
        await AuthenticateAsEmployeeAsync();
        await EnsureNoActivePunchAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/time-entries/me/active");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task PunchIn_Returns200OK_AndPunchOut_ClosesEntry()
    {
        await EnsureEmployeeProfileForEmployeeUserAsync();
        await AuthenticateAsEmployeeAsync();
        await EnsureNoActivePunchAsync();
        await SeedTodayShiftWithinToleranceForEmployeeUserAsync();

        HttpResponseMessage punchInResponse = await m_client.PostAsync("/api/time-entries/me/punch-in", null);
        Assert.Equal(HttpStatusCode.OK, punchInResponse.StatusCode);

        TimeEntryResponseDto? punchedIn = await punchInResponse.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
        Assert.NotNull(punchedIn);
        Assert.Null(punchedIn.EndTime);
        Assert.True(punchedIn.Id > 0);

        HttpResponseMessage activeResponse = await m_client.GetAsync("/api/time-entries/me/active");
        activeResponse.EnsureSuccessStatusCode();
        TimeEntryResponseDto? activeEntry = await activeResponse.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
        Assert.NotNull(activeEntry);
        Assert.Equal(punchedIn.Id, activeEntry.Id);

        HttpResponseMessage punchOutResponse = await m_client.PostAsync("/api/time-entries/me/punch-out", null);
        Assert.Equal(HttpStatusCode.OK, punchOutResponse.StatusCode);

        TimeEntryResponseDto? punchedOut = await punchOutResponse.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
        Assert.NotNull(punchedOut);
        Assert.NotNull(punchedOut.EndTime);

        HttpResponseMessage activeAfterResponse = await m_client.GetAsync("/api/time-entries/me/active");
        Assert.Equal(HttpStatusCode.NoContent, activeAfterResponse.StatusCode);
    }

    [Fact]
    public async Task PunchIn_Returns409Conflict_WhenAlreadyPunchedIn()
    {
        await EnsureEmployeeProfileForEmployeeUserAsync();
        await AuthenticateAsEmployeeAsync();
        await EnsureNoActivePunchAsync();
        await SeedTodayShiftWithinToleranceForEmployeeUserAsync();

        HttpResponseMessage firstResponse = await m_client.PostAsync("/api/time-entries/me/punch-in", null);
        firstResponse.EnsureSuccessStatusCode();

        HttpResponseMessage secondResponse = await m_client.PostAsync("/api/time-entries/me/punch-in", null);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        JsonDocument document = JsonDocument.Parse(await secondResponse.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal(ErrorMessages.TimeEntry.PunchAlreadyInProgress, message);

        HttpResponseMessage cleanupResponse = await m_client.PostAsync("/api/time-entries/me/punch-out", null);
        cleanupResponse.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task GetPunchEligibility_ReturnsCanPunchInFalse_WhenTooEarlyForShift()
    {
        await EnsureEmployeeProfileForEmployeeUserAsync();
        await AuthenticateAsEmployeeAsync();
        await EnsureNoActivePunchAsync();

        if (!TryBuildSameDayTooEarlyShiftTimes(out TimeOnly shiftStart, out TimeOnly shiftEnd))
        {
            return;
        }

        await SeedTodayShiftForEmployeeUserAsync(shiftStart, shiftEnd);

        HttpResponseMessage response = await m_client.GetAsync("/api/time-entries/me/punch-eligibility");
        response.EnsureSuccessStatusCode();

        PunchEligibilityDto? eligibility = await response.Content.ReadFromJsonAsync<PunchEligibilityDto>();
        Assert.NotNull(eligibility);
        Assert.False(eligibility.CanPunchIn);
        Assert.NotNull(eligibility.BlockedReason);
        Assert.Contains("Punch-in opens at", eligibility.BlockedReason);
    }

    [Fact]
    public async Task GetPunchEligibility_ReturnsCanPunchInFalse_WhenNoShiftToday()
    {
        await EnsureEmployeeProfileForEmployeeUserAsync();
        await AuthenticateAsEmployeeAsync();
        await EnsureNoActivePunchAsync();
        await ClearTodayShiftsForEmployeeUserAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/time-entries/me/punch-eligibility");
        response.EnsureSuccessStatusCode();

        PunchEligibilityDto? eligibility = await response.Content.ReadFromJsonAsync<PunchEligibilityDto>();
        Assert.NotNull(eligibility);
        Assert.False(eligibility.CanPunchIn);
        Assert.Equal(PunchEligibilityBlockCodes.NoShift, eligibility.BlockCode);
        Assert.Contains("No shift", eligibility.BlockedReason);
    }

    [Fact]
    public async Task PunchIn_Returns409Conflict_WhenNoShiftToday()
    {
        await EnsureEmployeeProfileForEmployeeUserAsync();
        await AuthenticateAsEmployeeAsync();
        await EnsureNoActivePunchAsync();
        await ClearTodayShiftsForEmployeeUserAsync();

        HttpResponseMessage response = await m_client.PostAsync("/api/time-entries/me/punch-in", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Contains("No shift", message);
    }

    [Fact]
    public async Task PunchIn_Returns409Conflict_WhenTooEarlyForShift()
    {
        await EnsureEmployeeProfileForEmployeeUserAsync();
        await AuthenticateAsEmployeeAsync();
        await EnsureNoActivePunchAsync();

        if (!TryBuildSameDayTooEarlyShiftTimes(out TimeOnly shiftStart, out TimeOnly shiftEnd))
        {
            return;
        }

        await SeedTodayShiftForEmployeeUserAsync(shiftStart, shiftEnd);

        HttpResponseMessage response = await m_client.PostAsync("/api/time-entries/me/punch-in", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Contains("Punch-in opens at", message);
    }

    [Fact]
    public async Task GetPunchEligibility_ReturnsCanPunchInTrue_WhenWithinEarlyTolerance()
    {
        await EnsureEmployeeProfileForEmployeeUserAsync();
        await AuthenticateAsEmployeeAsync();
        await EnsureNoActivePunchAsync();

        (TimeOnly shiftStart, TimeOnly shiftEnd) = BuildSameDayWithinToleranceShiftTimes();
        await SeedTodayShiftForEmployeeUserAsync(shiftStart, shiftEnd);

        HttpResponseMessage response = await m_client.GetAsync("/api/time-entries/me/punch-eligibility");
        response.EnsureSuccessStatusCode();

        PunchEligibilityDto? eligibility = await response.Content.ReadFromJsonAsync<PunchEligibilityDto>();
        Assert.NotNull(eligibility);
        Assert.True(eligibility.CanPunchIn);
        Assert.Null(eligibility.BlockedReason);
    }

    [Fact]
    public async Task GetPunchEligibility_ReturnsCanPunchInFalse_WhenTooLateForShift()
    {
        await EnsureEmployeeProfileForEmployeeUserAsync();
        await AuthenticateAsEmployeeAsync();
        await EnsureNoActivePunchAsync();

        if (!TryBuildSameDayTooLateShiftTimes(out TimeOnly shiftStart, out TimeOnly shiftEnd))
        {
            return;
        }

        await SeedTodayShiftForEmployeeUserAsync(shiftStart, shiftEnd);

        HttpResponseMessage response = await m_client.GetAsync("/api/time-entries/me/punch-eligibility");
        response.EnsureSuccessStatusCode();

        PunchEligibilityDto? eligibility = await response.Content.ReadFromJsonAsync<PunchEligibilityDto>();
        Assert.NotNull(eligibility);
        Assert.False(eligibility.CanPunchIn);
        Assert.Equal(PunchEligibilityBlockCodes.TooLate, eligibility.BlockCode);
        Assert.NotNull(eligibility.BlockedReason);
        Assert.Contains("Punch-in closed at", eligibility.BlockedReason);
    }

    [Fact]
    public async Task PunchIn_Returns409Conflict_WhenTooLateForShift()
    {
        await EnsureEmployeeProfileForEmployeeUserAsync();
        await AuthenticateAsEmployeeAsync();
        await EnsureNoActivePunchAsync();

        if (!TryBuildSameDayTooLateShiftTimes(out TimeOnly shiftStart, out TimeOnly shiftEnd))
        {
            return;
        }

        await SeedTodayShiftForEmployeeUserAsync(shiftStart, shiftEnd);

        HttpResponseMessage response = await m_client.PostAsync("/api/time-entries/me/punch-in", null);
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Contains("Punch-in closed at", message);
    }

    private async Task EnsureNoActivePunchAsync()
    {
        HttpResponseMessage activeResponse = await m_client.GetAsync("/api/time-entries/me/active");
        if (activeResponse.StatusCode == HttpStatusCode.NoContent)
        {
            return;
        }

        activeResponse.EnsureSuccessStatusCode();
        TimeEntryResponseDto? activeEntry = await activeResponse.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
        if (activeEntry is not null)
        {
            HttpResponseMessage punchOutResponse = await m_client.PostAsync("/api/time-entries/me/punch-out", null);
            punchOutResponse.EnsureSuccessStatusCode();
        }
    }

    private async Task SeedTodayShiftWithinToleranceForEmployeeUserAsync()
    {
        (TimeOnly shiftStart, TimeOnly shiftEnd) = BuildSameDayWithinToleranceShiftTimes();
        await SeedTodayShiftForEmployeeUserAsync(shiftStart, shiftEnd);
    }

    /// <summary>
    /// Builds a same-day shift that starts soon enough to allow punch-in now.
    /// </summary>
    private static (TimeOnly Start, TimeOnly End) BuildSameDayWithinToleranceShiftTimes()
    {
        DateTime now = BusinessClock.NowInBusinessZone;
        TimeOnly currentTime = BusinessClock.CurrentTime;
        DateOnly today = BusinessClock.Today;

        TimeOnly shiftStart = currentTime.AddMinutes(5);
        TimeOnly shiftEnd = shiftStart.AddHours(4);
        if (shiftEnd <= shiftStart)
        {
            shiftEnd = new TimeOnly(23, 59);
        }

        DateTime latestAllowed = today.ToDateTime(shiftEnd)
            .AddMinutes(TimeAttendancePolicy.LatePunchGraceMinutes);
        if (now > latestAllowed)
        {
            shiftEnd = currentTime.AddMinutes(TimeAttendancePolicy.LatePunchGraceMinutes - 1);
            shiftStart = shiftEnd.AddHours(-4);
            if (shiftStart >= shiftEnd)
            {
                shiftStart = new TimeOnly(6, 0);
                shiftEnd = new TimeOnly(14, 0);
            }
        }

        TimeOnly earliestAllowed = shiftStart.AddMinutes(-TimeAttendancePolicy.EarlyPunchToleranceMinutes);
        if (currentTime < earliestAllowed)
        {
            shiftStart = currentTime.AddMinutes(5);
            shiftEnd = shiftStart.AddHours(4);
            if (shiftEnd <= shiftStart)
            {
                shiftEnd = new TimeOnly(23, 59);
            }
        }

        return (shiftStart, shiftEnd);
    }

    /// <summary>
    /// Builds a same-day shift where punch-in is still blocked (before early tolerance window).
    /// Returns false when no valid same-day window exists (last minutes before midnight).
    /// </summary>
    private static bool TryBuildSameDayTooEarlyShiftTimes(out TimeOnly p_shiftStart, out TimeOnly p_shiftEnd)
    {
        DateTime now = BusinessClock.NowInBusinessZone;
        TimeOnly currentTime = BusinessClock.CurrentTime;
        DateOnly today = BusinessClock.Today;
        int minimumLeadMinutes = TimeAttendancePolicy.EarlyPunchToleranceMinutes + 30;

        DateTime preferredStart = now.AddHours(2);
        DateTime shiftStartDateTime =
            DateOnly.FromDateTime(preferredStart) == today
                ? preferredStart
                : today.ToDateTime(new TimeOnly(23, 59, 59));

        TimeOnly shiftStart = TimeOnly.FromDateTime(shiftStartDateTime);
        TimeOnly earliestAllowed = shiftStart.AddMinutes(-TimeAttendancePolicy.EarlyPunchToleranceMinutes);

        if (currentTime >= earliestAllowed)
        {
            DateTime fallbackStart = now.AddMinutes(minimumLeadMinutes);
            if (DateOnly.FromDateTime(fallbackStart) != today)
            {
                p_shiftStart = default;
                p_shiftEnd = default;
                return false;
            }

            shiftStart = TimeOnly.FromDateTime(fallbackStart);
            earliestAllowed = shiftStart.AddMinutes(-TimeAttendancePolicy.EarlyPunchToleranceMinutes);
        }

        if (currentTime >= earliestAllowed)
        {
            p_shiftStart = default;
            p_shiftEnd = default;
            return false;
        }

        TimeOnly shiftEnd = shiftStart.AddHours(8);
        if (shiftEnd <= shiftStart)
        {
            shiftEnd = new TimeOnly(23, 59);
        }

        p_shiftStart = shiftStart;
        p_shiftEnd = shiftEnd;
        return true;
    }

    /// <summary>
    /// Builds a same-day shift where punch-in is blocked because the late grace window has passed.
    /// Returns false when no valid same-day window exists (early morning).
    /// </summary>
    private static bool TryBuildSameDayTooLateShiftTimes(out TimeOnly p_shiftStart, out TimeOnly p_shiftEnd)
    {
        DateTime now = BusinessClock.NowInBusinessZone;
        TimeOnly currentTime = BusinessClock.CurrentTime;
        DateOnly today = BusinessClock.Today;
        int minimumShiftDurationMinutes = 60;
        int bufferMinutes = 5;
        int minutesAfterGrace = TimeAttendancePolicy.LatePunchGraceMinutes + bufferMinutes;

        int currentTotalMinutes = (currentTime.Hour * 60) + currentTime.Minute;
        int shiftEndTotalMinutes = currentTotalMinutes - minutesAfterGrace;

        if (shiftEndTotalMinutes < minimumShiftDurationMinutes)
        {
            p_shiftStart = default;
            p_shiftEnd = default;
            return false;
        }

        TimeOnly shiftEnd = TimeOnly.FromTimeSpan(TimeSpan.FromMinutes(shiftEndTotalMinutes));
        TimeOnly shiftStart = shiftEnd.AddHours(-8);

        if (shiftStart >= shiftEnd)
        {
            p_shiftStart = default;
            p_shiftEnd = default;
            return false;
        }

        DateTime latestAllowed = today.ToDateTime(shiftEnd)
            .AddMinutes(TimeAttendancePolicy.LatePunchGraceMinutes);
        if (now <= latestAllowed)
        {
            p_shiftStart = default;
            p_shiftEnd = default;
            return false;
        }

        p_shiftStart = shiftStart;
        p_shiftEnd = shiftEnd;
        return true;
    }

    private async Task ClearTodayShiftsForEmployeeUserAsync()
    {
        int employeeProfileId = await EnsureEmployeeProfileForEmployeeUserAsync();
        DateOnly today = BusinessClock.Today;

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        List<Crystal.Core.Entities.ScheduledShift> todayShifts = await context.ScheduledShifts
            .Where(p_shift => p_shift.EmployeeProfileId == employeeProfileId && p_shift.Date == today)
            .ToListAsync();

        if (todayShifts.Count == 0)
        {
            return;
        }

        List<int> shiftIds = todayShifts.Select(p_shift => p_shift.Id).ToList();

        List<Crystal.Core.Entities.TimeEntry> linkedEntries = await context.TimeEntries
            .Where(p_entry =>
                p_entry.ScheduledShiftId.HasValue &&
                shiftIds.Contains(p_entry.ScheduledShiftId.Value))
            .ToListAsync();

        foreach (Crystal.Core.Entities.TimeEntry entry in linkedEntries)
        {
            entry.ScheduledShiftId = null;
        }

        context.ScheduledShifts.RemoveRange(todayShifts);
        await context.SaveChangesAsync();
    }

    private async Task SeedTodayShiftForEmployeeUserAsync(TimeOnly p_startTime, TimeOnly p_endTime)
    {
        int employeeProfileId = await EnsureEmployeeProfileForEmployeeUserAsync();
        DateOnly today = BusinessClock.Today;

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        await ClearTodayShiftsForEmployeeUserAsync();

        Crystal.Core.Entities.EmployeeProfile? profile = await context.EmployeeProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p_ep => p_ep.Id == employeeProfileId);

        Assert.NotNull(profile);

        Crystal.Core.Entities.ScheduledShift scheduledShift = new()
        {
            EmployeeProfileId = employeeProfileId,
            JobPositionId = profile.PositionId,
            Date = today,
            StartTime = p_startTime,
            EndTime = p_endTime,
            IsDeleted = false
        };

        await context.ScheduledShifts.AddAsync(scheduledShift);
        await context.SaveChangesAsync();
    }

    private async Task<int> GetEmployeeProfileIdForEmployeeUserAsync()
    {
        return await EnsureEmployeeProfileForEmployeeUserAsync();
    }

    private async Task<int> EnsureEmployeeProfileForEmployeeUserAsync()
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        Microsoft.AspNetCore.Identity.UserManager<Crystal.Core.Entities.ApplicationUser> userManager =
            scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Crystal.Core.Entities.ApplicationUser>>();

        Crystal.Core.Entities.ApplicationUser? user =
            await userManager.FindByEmailAsync("employee@crystal.local");
        Assert.NotNull(user);

        Crystal.Core.Entities.EmployeeProfile? profileByUser = await context.EmployeeProfiles
            .FirstOrDefaultAsync(p_profile => p_profile.ApplicationUserId == user.Id);

        if (profileByUser is not null)
        {
            return profileByUser.Id;
        }

        Crystal.Core.Entities.EmployeeProfile? profileByEmail = await context.EmployeeProfiles
            .FirstOrDefaultAsync(p_profile => p_profile.Email == "employee@crystal.local");

        if (profileByEmail is not null)
        {
            return profileByEmail.Id;
        }

        Crystal.Core.Entities.Location location = await context.Locations.FirstAsync();
        Crystal.Core.Entities.JobPosition jobPosition = new()
        {
            Name = $"Poste-{Guid.NewGuid()}",
            Description = "Poste pour tests de pointage",
            IsDeleted = false
        };
        await context.JobPositions.AddAsync(jobPosition);
        await context.SaveChangesAsync();

        Crystal.Core.Entities.EmployeeProfile profile = new()
        {
            FirstName = "Émilie",
            LastName = "Test",
            Email = "employee@crystal.local",
            ApplicationUserId = user.Id,
            Salary = 43000m,
            Status = "Active",
            PositionId = jobPosition.Id,
            HiringDate = new DateOnly(2024, 1, 15),
            LocationId = location.Id,
            IsDeleted = false
        };

        await context.EmployeeProfiles.AddAsync(profile);
        await context.SaveChangesAsync();

        return profile.Id;
    }

    [Fact]
    public async Task Delete_Returns204NoContent_AndPerformsSoftDelete()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync(p_includeScheduledShift: false);
        CreateTimeEntryRequest createRequest = BuildCreateRequest(
            seedResult,
            null,
            new DateOnly(2026, 10, 8),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/time-entries", createRequest);
        createResponse.EnsureSuccessStatusCode();

        TimeEntryResponseDto? created = await createResponse.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
        Assert.NotNull(created);

        HttpResponseMessage deleteResponse = await m_client.DeleteAsync($"/api/time-entries/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        HttpResponseMessage getResponse = await m_client.GetAsync($"/api/time-entries/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.TimeEntry? deletedEntry = await context.TimeEntries
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(p_entry => p_entry.Id == created.Id);

        Assert.NotNull(deletedEntry);
        Assert.True(deletedEntry.IsDeleted);
    }

    private async Task<HrSeedResult> SeedHrReferenceDataAsync(
        bool p_includeScheduledShift,
        bool p_linkToEmployeeAccount = false)
    {
        string uniqueSuffix = Guid.NewGuid().ToString();
        string jobPositionName = $"JobPosition-{uniqueSuffix}";
        string employeeEmail = $"employee-{uniqueSuffix}@test.local";
        string employeeFirstName = "Punch";
        string employeeLastName = $"Test-{uniqueSuffix}";

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.JobPosition jobPosition = new()
        {
            Name = jobPositionName,
            Description = "Poste pour tests de pointage",
            IsDeleted = false
        };

        await context.JobPositions.AddAsync(jobPosition);
        await context.SaveChangesAsync();

        string? applicationUserId = null;
        if (p_linkToEmployeeAccount)
        {
            Microsoft.AspNetCore.Identity.UserManager<Crystal.Core.Entities.ApplicationUser> userManager =
                scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Crystal.Core.Entities.ApplicationUser>>();
            Crystal.Core.Entities.ApplicationUser? employeeUser =
                await userManager.FindByEmailAsync("employee@crystal.local");
            applicationUserId = employeeUser?.Id;
        }

        Crystal.Core.Entities.EmployeeProfile employeeProfile = new()
        {
            FirstName = employeeFirstName,
            LastName = employeeLastName,
            Email = employeeEmail,
            Salary = 50000m,
            Status = "Active",
            PositionId = jobPosition.Id,
            HiringDate = new DateOnly(2024, 1, 1),
            ApplicationUserId = applicationUserId,
            IsDeleted = false
        };

        await context.EmployeeProfiles.AddAsync(employeeProfile);
        await context.SaveChangesAsync();

        int? scheduledShiftId = null;
        if (p_includeScheduledShift)
        {
            Crystal.Core.Entities.ScheduledShift scheduledShift = new()
            {
                EmployeeProfileId = employeeProfile.Id,
                JobPositionId = jobPosition.Id,
                Date = new DateOnly(2026, 10, 1),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0),
                IsDeleted = false
            };

            await context.ScheduledShifts.AddAsync(scheduledShift);
            await context.SaveChangesAsync();
            scheduledShiftId = scheduledShift.Id;
        }

        return new HrSeedResult(
            employeeProfile.Id,
            employeeFirstName,
            employeeLastName,
            jobPosition.Id,
            scheduledShiftId);
    }

    private static CreateTimeEntryRequest BuildCreateRequest(
        HrSeedResult p_seedResult,
        int? p_scheduledShiftId,
        DateOnly p_date,
        TimeOnly p_startTime,
        TimeOnly? p_endTime)
    {
        return new CreateTimeEntryRequest
        {
            EmployeeProfileId = p_seedResult.EmployeeProfileId,
            ScheduledShiftId = p_scheduledShiftId,
            Date = p_date,
            StartTime = p_startTime,
            EndTime = p_endTime
        };
    }

    private async Task AuthenticateAsAdminAsync()
    {
        await AuthenticateAsync("admin@crystal.local");
    }

    private async Task AuthenticateAsEmployeeAsync()
    {
        await AuthenticateAsync("employee@crystal.local");
    }

    private async Task AuthenticateAsync(string p_email)
    {
        LoginRequest request = new()
        {
            Email = p_email,
            Password = "ValidPass1!a"
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();

        LoginResponse? login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));

        m_client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Token);
    }

    public void Dispose()
    {
        m_client.Dispose();
    }

    private sealed class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    private sealed record HrSeedResult(
        int EmployeeProfileId,
        string EmployeeFirstName,
        string EmployeeLastName,
        int JobPositionId,
        int? ScheduledShiftId);
}
