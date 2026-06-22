using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Enums;
using Crystal.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Crystal.IntegrationTests.Timesheet;

public sealed class TimesheetIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public TimesheetIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns200OK_WithEmployeeToken()
    {
        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/timesheets");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created_LinksTimeEntries_AndPersistsRelations()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        List<int> timeEntryIds = await CreateTimeEntriesAsync(seedResult, 2);

        CreateTimesheetRequest request = new()
        {
            EmployeeProfileId = seedResult.EmployeeProfileId,
            PeriodStart = new DateOnly(2026, 5, 1),
            PeriodEnd = new DateOnly(2026, 5, 31),
            TimeEntryIds = timeEntryIds
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/timesheets", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        TimesheetResponseDto? body = await response.Content.ReadFromJsonAsync<TimesheetResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("Draft", body.Status);
        Assert.Equal(seedResult.EmployeeFirstName, body.EmployeeFirstName);
        Assert.Equal(2, body.TimeEntries.Count);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.TimeEntry? firstEntry = await context.TimeEntries.FindAsync(timeEntryIds[0]);
        Crystal.Core.Entities.TimeEntry? secondEntry = await context.TimeEntries.FindAsync(timeEntryIds[1]);

        Assert.NotNull(firstEntry);
        Assert.NotNull(secondEntry);
        Assert.Equal(body.Id, firstEntry.TimesheetId);
        Assert.Equal(body.Id, secondEntry.TimesheetId);
    }

    [Fact]
    public async Task GenerateWeekly_CreatesMissingTimesheets_LinksTimeEntries_AndIsIdempotent()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        DateOnly periodStart = GetCompletedWeekMonday(weeksAgo: 2);
        List<int> timeEntryIds = await CreateTimeEntriesAsync(seedResult, 2, periodStart);

        GenerateWeeklyTimesheetsRequest request = new()
        {
            PeriodStart = periodStart
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/timesheets/generate-weekly", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GenerateWeeklyTimesheetsResponseDto? body = await response.Content.ReadFromJsonAsync<GenerateWeeklyTimesheetsResponseDto>();
        Assert.NotNull(body);
        Assert.Equal(periodStart, body.PeriodStart);
        Assert.Equal(periodStart.AddDays(6), body.PeriodEnd);
        Assert.True(body.CreatedCount > 0);
        Assert.True(body.LinkedTimeEntryCount >= 2);

        TimesheetResponseDto employeeTimesheet = Assert.Single(
            body.Timesheets,
            p_timesheet => p_timesheet.EmployeeProfileId == seedResult.EmployeeProfileId);

        Assert.Equal("Draft", employeeTimesheet.Status);
        Assert.Equal(2, employeeTimesheet.TimeEntries.Count);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.TimeEntry? firstEntry = await context.TimeEntries.FindAsync(timeEntryIds[0]);
        Crystal.Core.Entities.TimeEntry? secondEntry = await context.TimeEntries.FindAsync(timeEntryIds[1]);

        Assert.NotNull(firstEntry);
        Assert.NotNull(secondEntry);
        Assert.Equal(employeeTimesheet.Id, firstEntry.TimesheetId);
        Assert.Equal(employeeTimesheet.Id, secondEntry.TimesheetId);

        HttpResponseMessage secondResponse = await m_client.PostAsJsonAsync("/api/timesheets/generate-weekly", request);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        GenerateWeeklyTimesheetsResponseDto? secondBody =
            await secondResponse.Content.ReadFromJsonAsync<GenerateWeeklyTimesheetsResponseDto>();

        Assert.NotNull(secondBody);
        Assert.Equal(0, secondBody.CreatedCount);

        int employeeTimesheetCount = context.Timesheets.Count(p_timesheet =>
            p_timesheet.EmployeeProfileId == seedResult.EmployeeProfileId
            && p_timesheet.PeriodStart == periodStart
            && p_timesheet.PeriodEnd == periodStart.AddDays(6));

        Assert.Equal(1, employeeTimesheetCount);
    }

    [Fact]
    public async Task GenerateWeekly_WithLocationId_CreatesTimesheetsOnlyForThatLocation()
    {
        await AuthenticateAsAdminAsync();

        int firstLocationId = await CreateLocationAsync();
        int secondLocationId = await CreateLocationAsync();
        HrSeedResult firstLocationEmployee = await SeedHrReferenceDataAsync(firstLocationId);
        HrSeedResult secondLocationEmployee = await SeedHrReferenceDataAsync(secondLocationId);
        DateOnly periodStart = GetCompletedWeekMonday(weeksAgo: 3);
        List<int> firstLocationTimeEntryIds = await CreateTimeEntriesAsync(firstLocationEmployee, 1, periodStart);
        List<int> secondLocationTimeEntryIds = await CreateTimeEntriesAsync(secondLocationEmployee, 1, periodStart);

        GenerateWeeklyTimesheetsRequest request = new()
        {
            PeriodStart = periodStart,
            LocationId = firstLocationId
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/timesheets/generate-weekly", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GenerateWeeklyTimesheetsResponseDto? body = await response.Content.ReadFromJsonAsync<GenerateWeeklyTimesheetsResponseDto>();
        Assert.NotNull(body);
        Assert.Equal(firstLocationId, body.LocationId);

        TimesheetResponseDto firstLocationTimesheet = Assert.Single(
            body.Timesheets,
            p_timesheet => p_timesheet.EmployeeProfileId == firstLocationEmployee.EmployeeProfileId);

        Assert.DoesNotContain(
            body.Timesheets,
            p_timesheet => p_timesheet.EmployeeProfileId == secondLocationEmployee.EmployeeProfileId);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.TimeEntry? firstLocationEntry = await context.TimeEntries.FindAsync(firstLocationTimeEntryIds[0]);
        Crystal.Core.Entities.TimeEntry? secondLocationEntry = await context.TimeEntries.FindAsync(secondLocationTimeEntryIds[0]);

        Assert.NotNull(firstLocationEntry);
        Assert.NotNull(secondLocationEntry);
        Assert.Equal(firstLocationTimesheet.Id, firstLocationEntry.TimesheetId);
        Assert.Null(secondLocationEntry.TimesheetId);

        int secondLocationTimesheetCount = context.Timesheets.Count(p_timesheet =>
            p_timesheet.EmployeeProfileId == secondLocationEmployee.EmployeeProfileId
            && p_timesheet.PeriodStart == periodStart
            && p_timesheet.PeriodEnd == periodStart.AddDays(6));

        Assert.Equal(0, secondLocationTimesheetCount);
    }

    [Fact]
    public async Task GenerateWeekly_Returns400BadRequest_WhenWeekIsNotComplete()
    {
        await AuthenticateAsAdminAsync();

        GenerateWeeklyTimesheetsRequest request = new()
        {
            PeriodStart = GetCompletedWeekMonday(weeksAgo: 0)
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/timesheets/generate-weekly", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GenerateWeekly_Returns400BadRequest_WhenPeriodStartIsNotMonday()
    {
        await AuthenticateAsAdminAsync();

        GenerateWeeklyTimesheetsRequest request = new()
        {
            PeriodStart = GetCompletedWeekMonday(weeksAgo: 2).AddDays(1)
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/timesheets/generate-weekly", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ApprovalFlow_TransitionsDraftToSubmittedToApproved()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int timesheetId = await CreateTimesheetAsync(seedResult, new List<int>());

        HttpResponseMessage submitResponse = await m_client.PatchAsJsonAsync(
            $"/api/timesheets/{timesheetId}/status",
            new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Submitted });

        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        TimesheetResponseDto? submitted = await submitResponse.Content.ReadFromJsonAsync<TimesheetResponseDto>();
        Assert.NotNull(submitted);
        Assert.Equal("Submitted", submitted.Status);

        HttpResponseMessage approveResponse = await m_client.PatchAsJsonAsync(
            $"/api/timesheets/{timesheetId}/status",
            new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Approved });

        Assert.Equal(HttpStatusCode.OK, approveResponse.StatusCode);

        TimesheetResponseDto? approved = await approveResponse.Content.ReadFromJsonAsync<TimesheetResponseDto>();
        Assert.NotNull(approved);
        Assert.Equal("Approved", approved.Status);
    }

    [Fact]
    public async Task UpdatePaid_PersistsAndReturnsPaymentState()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int timesheetId = await CreateTimesheetAsync(seedResult, new List<int>());

        HttpResponseMessage response = await m_client.PatchAsJsonAsync(
            $"/api/timesheets/{timesheetId}/paid",
            new UpdateTimesheetPaidRequest { IsPaid = true });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        TimesheetResponseDto? updated = await response.Content.ReadFromJsonAsync<TimesheetResponseDto>();
        Assert.NotNull(updated);
        Assert.True(updated.IsPaid);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        Crystal.Core.Entities.Timesheet persisted = await context.Timesheets
            .IgnoreQueryFilters()
            .SingleAsync(p_timesheet => p_timesheet.Id == timesheetId);
        Assert.True(persisted.IsPaid);
    }

    [Fact]
    public async Task UpdateStatus_AssistantCanSubmitTimesheetInOwnLocation()
    {
        await AuthenticateAsAdminAsync();

        int assistantLocationId = await GetEmployeeLocationIdByEmailAsync("assistant@crystal.local");
        HrSeedResult employeeInAssistantLocation = await SeedHrReferenceDataAsync(assistantLocationId);
        int timesheetId = await CreateTimesheetAsync(employeeInAssistantLocation, new List<int>());

        await AuthenticateAsAssistantAsync();

        HttpResponseMessage submitResponse = await m_client.PatchAsJsonAsync(
            $"/api/timesheets/{timesheetId}/status",
            new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Submitted });

        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        TimesheetResponseDto? submitted = await submitResponse.Content.ReadFromJsonAsync<TimesheetResponseDto>();
        Assert.NotNull(submitted);
        Assert.Equal("Submitted", submitted.Status);
    }

    [Fact]
    public async Task GenerateWeekly_AssistantGeneratesTimesheetsOnlyForOwnLocation()
    {
        await AuthenticateAsAdminAsync();

        int assistantLocationId = await GetEmployeeLocationIdByEmailAsync("assistant@crystal.local");
        int otherLocationId = await CreateLocationAsync();
        HrSeedResult employeeInAssistantLocation = await SeedHrReferenceDataAsync(assistantLocationId);
        HrSeedResult employeeInOtherLocation = await SeedHrReferenceDataAsync(otherLocationId);
        DateOnly periodStart = GetCompletedWeekMonday(weeksAgo: 4);
        List<int> assistantLocationTimeEntryIds = await CreateTimeEntriesAsync(employeeInAssistantLocation, 1, periodStart);
        List<int> otherLocationTimeEntryIds = await CreateTimeEntriesAsync(employeeInOtherLocation, 1, periodStart);

        await AuthenticateAsAssistantAsync();

        GenerateWeeklyTimesheetsRequest request = new()
        {
            PeriodStart = periodStart
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/timesheets/generate-weekly", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GenerateWeeklyTimesheetsResponseDto? body = await response.Content.ReadFromJsonAsync<GenerateWeeklyTimesheetsResponseDto>();
        Assert.NotNull(body);
        Assert.Equal(assistantLocationId, body.LocationId);
        Assert.Contains(body.Timesheets, p_timesheet => p_timesheet.EmployeeProfileId == employeeInAssistantLocation.EmployeeProfileId);
        Assert.DoesNotContain(body.Timesheets, p_timesheet => p_timesheet.EmployeeProfileId == employeeInOtherLocation.EmployeeProfileId);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.TimeEntry? assistantLocationEntry =
            await context.TimeEntries.FindAsync(assistantLocationTimeEntryIds[0]);
        Crystal.Core.Entities.TimeEntry? otherLocationEntry =
            await context.TimeEntries.FindAsync(otherLocationTimeEntryIds[0]);

        Assert.NotNull(assistantLocationEntry);
        Assert.NotNull(otherLocationEntry);
        Assert.NotNull(assistantLocationEntry.TimesheetId);
        Assert.Null(otherLocationEntry.TimesheetId);
    }

    [Fact]
    public async Task UpdateStatus_AssistantCannotApproveTimesheet()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult assistantProfile = await EnsureEmployeeProfileForUserAsync("assistant@crystal.local");
        int timesheetId = await CreateTimesheetAsync(assistantProfile, new List<int>());

        HttpResponseMessage submitResponse = await m_client.PatchAsJsonAsync(
            $"/api/timesheets/{timesheetId}/status",
            new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Submitted });
        submitResponse.EnsureSuccessStatusCode();

        await AuthenticateAsAssistantAsync();

        HttpResponseMessage approveResponse = await m_client.PatchAsJsonAsync(
            $"/api/timesheets/{timesheetId}/status",
            new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Approved });

        Assert.Equal(HttpStatusCode.Forbidden, approveResponse.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_Returns409Conflict_WhenApprovingDraftDirectly()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int timesheetId = await CreateTimesheetAsync(seedResult, new List<int>());

        HttpResponseMessage response = await m_client.PatchAsJsonAsync(
            $"/api/timesheets/{timesheetId}/status",
            new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Approved });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_Returns409Conflict_WhenRejectingDraftDirectly()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int timesheetId = await CreateTimesheetAsync(seedResult, new List<int>());

        HttpResponseMessage response = await m_client.PatchAsJsonAsync(
            $"/api/timesheets/{timesheetId}/status",
            new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Rejected });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_RejectedCanBeResubmitted()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int timesheetId = await CreateTimesheetAsync(seedResult, new List<int>());

        await m_client.PatchAsJsonAsync(
            $"/api/timesheets/{timesheetId}/status",
            new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Submitted });

        HttpResponseMessage rejectResponse = await m_client.PatchAsJsonAsync(
            $"/api/timesheets/{timesheetId}/status",
            new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Rejected });

        rejectResponse.EnsureSuccessStatusCode();

        HttpResponseMessage resubmitResponse = await m_client.PatchAsJsonAsync(
            $"/api/timesheets/{timesheetId}/status",
            new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Submitted });

        Assert.Equal(HttpStatusCode.OK, resubmitResponse.StatusCode);

        TimesheetResponseDto? resubmitted = await resubmitResponse.Content.ReadFromJsonAsync<TimesheetResponseDto>();
        Assert.NotNull(resubmitted);
        Assert.Equal("Submitted", resubmitted.Status);
    }

    [Fact]
    public async Task Update_Returns409Conflict_WhenTimesheetIsNotDraft()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int timesheetId = await CreateTimesheetAsync(seedResult, new List<int>());

        await m_client.PatchAsJsonAsync(
            $"/api/timesheets/{timesheetId}/status",
            new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Submitted });

        CreateTimesheetRequest updateRequest = new()
        {
            EmployeeProfileId = seedResult.EmployeeProfileId,
            PeriodStart = new DateOnly(2026, 6, 1),
            PeriodEnd = new DateOnly(2026, 6, 30),
            TimeEntryIds = new List<int>()
        };

        HttpResponseMessage response = await m_client.PutAsJsonAsync($"/api/timesheets/{timesheetId}", updateRequest);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns400BadRequest_WhenPeriodEndIsBeforePeriodStart()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();

        CreateTimesheetRequest request = new()
        {
            EmployeeProfileId = seedResult.EmployeeProfileId,
            PeriodStart = new DateOnly(2026, 5, 31),
            PeriodEnd = new DateOnly(2026, 5, 1),
            TimeEntryIds = new List<int>()
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/timesheets", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns403Forbidden_WithEmployeeToken()
    {
        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await AuthenticateAsEmployeeAsync();

        CreateTimesheetRequest request = new()
        {
            EmployeeProfileId = seedResult.EmployeeProfileId,
            PeriodStart = new DateOnly(2026, 5, 1),
            PeriodEnd = new DateOnly(2026, 5, 31),
            TimeEntryIds = new List<int>()
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/timesheets", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<int> CreateTimesheetAsync(HrSeedResult p_seedResult, IList<int> p_timeEntryIds)
    {
        CreateTimesheetRequest request = new()
        {
            EmployeeProfileId = p_seedResult.EmployeeProfileId,
            PeriodStart = new DateOnly(2026, 5, 1),
            PeriodEnd = new DateOnly(2026, 5, 31),
            TimeEntryIds = p_timeEntryIds
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/timesheets", request);
        response.EnsureSuccessStatusCode();

        TimesheetResponseDto? created = await response.Content.ReadFromJsonAsync<TimesheetResponseDto>();
        Assert.NotNull(created);
        return created.Id;
    }

    private static DateOnly GetCompletedWeekMonday(int weeksAgo)
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        int daysSinceMonday = today.DayOfWeek == DayOfWeek.Sunday
            ? 6
            : (int)today.DayOfWeek - (int)DayOfWeek.Monday;

        DateOnly currentWeekMonday = today.AddDays(-daysSinceMonday);
        return currentWeekMonday.AddDays(-7 * weeksAgo);
    }

    private async Task<List<int>> CreateTimeEntriesAsync(
        HrSeedResult p_seedResult,
        int p_count,
        DateOnly? p_startDate = null)
    {
        List<int> ids = new List<int>();
        DateOnly startDate = p_startDate ?? new DateOnly(2026, 5, 1);

        for (int index = 0; index < p_count; index++)
        {
            CreateTimeEntryRequest request = new()
            {
                EmployeeProfileId = p_seedResult.EmployeeProfileId,
                ScheduledShiftId = null,
                Date = startDate.AddDays(index),
                StartTime = new TimeOnly(9, 0),
                EndTime = new TimeOnly(17, 0)
            };

            HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/time-entries", request);
            response.EnsureSuccessStatusCode();

            TimeEntryResponseDto? created = await response.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
            Assert.NotNull(created);
            ids.Add(created.Id);
        }

        return ids;
    }

    private async Task<HrSeedResult> SeedHrReferenceDataAsync(int? p_locationId = null)
    {
        string uniqueSuffix = Guid.NewGuid().ToString();
        string jobPositionName = $"JobPosition-{uniqueSuffix}";
        string employeeEmail = $"employee-{uniqueSuffix}@test.local";
        string employeeFirstName = "Timesheet";
        string employeeLastName = $"Test-{uniqueSuffix}";

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.JobPosition jobPosition = new()
        {
            Name = jobPositionName,
            Description = "Poste pour tests de feuille de temps",
            IsDeleted = false
        };

        await context.JobPositions.AddAsync(jobPosition);
        await context.SaveChangesAsync();

        Crystal.Core.Entities.EmployeeProfile employeeProfile = new()
        {
            FirstName = employeeFirstName,
            LastName = employeeLastName,
            Email = employeeEmail,
            Salary = 50000m,
            Status = "Active",
            PositionId = jobPosition.Id,
            HiringDate = new DateOnly(2024, 1, 1),
            LocationId = p_locationId,
            IsDeleted = false
        };

        await context.EmployeeProfiles.AddAsync(employeeProfile);
        await context.SaveChangesAsync();

        return new HrSeedResult(
            employeeProfile.Id,
            employeeFirstName,
            employeeLastName);
    }

    private async Task<HrSeedResult> EnsureEmployeeProfileForUserAsync(string p_email)
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        ApplicationUser? user = await userManager.FindByEmailAsync(p_email);
        Assert.NotNull(user);

        Crystal.Core.Entities.EmployeeProfile? existing = await context.EmployeeProfiles
            .FirstOrDefaultAsync(p_profile => p_profile.ApplicationUserId == user.Id);

        if (existing is not null)
        {
            return new HrSeedResult(existing.Id, existing.FirstName, existing.LastName);
        }

        Crystal.Core.Entities.Location location = await context.Locations.FirstAsync();
        Crystal.Core.Entities.JobPosition jobPosition = new()
        {
            Name = $"JobPosition-{Guid.NewGuid()}",
            Description = "Poste pour tests de feuille de temps",
            IsDeleted = false
        };

        await context.JobPositions.AddAsync(jobPosition);
        await context.SaveChangesAsync();

        Crystal.Core.Entities.EmployeeProfile profile = new()
        {
            FirstName = "Assistant",
            LastName = "Test",
            Email = p_email,
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

        return new HrSeedResult(profile.Id, profile.FirstName, profile.LastName);
    }

    private async Task<int> GetEmployeeLocationIdByEmailAsync(string p_email)
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        ApplicationUser? user = await userManager.FindByEmailAsync(p_email);
        Assert.NotNull(user);

        Crystal.Core.Entities.EmployeeProfile? profile = await context.EmployeeProfiles
            .FirstOrDefaultAsync(p_employeeProfile => p_employeeProfile.ApplicationUserId == user.Id);

        if (profile is null)
        {
            await EnsureEmployeeProfileForUserAsync(p_email);
            profile = await context.EmployeeProfiles
                .FirstOrDefaultAsync(p_employeeProfile => p_employeeProfile.ApplicationUserId == user.Id);
        }

        Assert.NotNull(profile);
        Assert.NotNull(profile.LocationId);

        return profile.LocationId.Value;
    }

    private async Task<int> CreateLocationAsync()
    {
        string uniqueSuffix = Guid.NewGuid().ToString();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.Location location = new()
        {
            Title = $"Location-{uniqueSuffix}",
            Address = "123 Test Street",
            Description = "Branch for timesheet tests"
        };

        await context.Locations.AddAsync(location);
        await context.SaveChangesAsync();

        return location.Id;
    }

    private async Task AuthenticateAsAdminAsync()
    {
        await AuthenticateAsync("admin@crystal.local");
    }

    private async Task AuthenticateAsEmployeeAsync()
    {
        await AuthenticateAsync("employee@crystal.local");
    }

    private async Task AuthenticateAsAssistantAsync()
    {
        await AuthenticateAsync("assistant@crystal.local");
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
        string EmployeeLastName);
}
