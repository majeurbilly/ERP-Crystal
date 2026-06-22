using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Enums;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Crystal.IntegrationTests.Payroll;

public sealed class PayrollIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    private static readonly DateOnly s_periodStart = GetLastCompleteWeekMonday();
    private static readonly DateOnly s_periodEnd = s_periodStart.AddDays(6);
    private const decimal s_hourlyRate = 25m;
    private const decimal s_expectedTotalHours = 16m;
    private const decimal s_expectedGrossPay = 400m;

    public PayrollIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task Generate_Returns200OK_WithCorrectHoursAndGrossPay()
    {
        await AuthenticateAsAdminAsync();

        PayrollSeedResult seedResult = await SeedPayrollScenarioAsync();

        GeneratePayrollRequest request = new()
        {
            PayPeriodId = seedResult.PayPeriodId,
            EmployeeProfileId = seedResult.EmployeeProfileId
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/payroll/generate", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        PayStubResponseDto? body = await response.Content.ReadFromJsonAsync<PayStubResponseDto>();
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal(seedResult.EmployeeFirstName, body.EmployeeFirstName);
        Assert.Equal(seedResult.EmployeeLastName, body.EmployeeLastName);
        Assert.Equal(s_periodStart, body.PeriodStartDate);
        Assert.Equal(s_periodEnd, body.PeriodEndDate);
        Assert.Equal(s_expectedTotalHours, body.TotalHours);
        Assert.Equal(s_expectedGrossPay, body.GrossPay);
        Assert.False(body.IsPublished);
    }

    [Fact]
    public async Task GetStubs_Returns200OK_IncludesGeneratedStub()
    {
        await AuthenticateAsAdminAsync();

        PayrollSeedResult seedResult = await SeedPayrollScenarioAsync();

        GeneratePayrollRequest request = new()
        {
            PayPeriodId = seedResult.PayPeriodId,
            EmployeeProfileId = seedResult.EmployeeProfileId
        };

        await m_client.PostAsJsonAsync("/api/payroll/generate", request);

        HttpResponseMessage response = await m_client.GetAsync("/api/payroll/stubs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<PayStubResponseDto>? stubs = await response.Content.ReadFromJsonAsync<List<PayStubResponseDto>>();
        Assert.NotNull(stubs);
        Assert.Contains(stubs, p_stub => p_stub.EmployeeProfileId == seedResult.EmployeeProfileId && p_stub.GrossPay == s_expectedGrossPay);
    }

    [Fact]
    public async Task Publish_Returns200OK_AndMarksStubAsPublished()
    {
        await AuthenticateAsAdminAsync();

        PayrollSeedResult seedResult = await SeedPayrollScenarioAsync();

        GeneratePayrollRequest request = new()
        {
            PayPeriodId = seedResult.PayPeriodId,
            EmployeeProfileId = seedResult.EmployeeProfileId
        };

        HttpResponseMessage generateResponse = await m_client.PostAsJsonAsync("/api/payroll/generate", request);
        generateResponse.EnsureSuccessStatusCode();

        PayStubResponseDto? generated = await generateResponse.Content.ReadFromJsonAsync<PayStubResponseDto>();
        Assert.NotNull(generated);
        Assert.False(generated.IsPublished);

        HttpResponseMessage publishResponse = await m_client.PostAsync(
            $"/api/payroll/stubs/{generated.Id}/publish",
            null);

        Assert.Equal(HttpStatusCode.OK, publishResponse.StatusCode);

        PayStubResponseDto? published = await publishResponse.Content.ReadFromJsonAsync<PayStubResponseDto>();
        Assert.NotNull(published);
        Assert.Equal(generated.Id, published.Id);
        Assert.True(published.IsPublished);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        Crystal.Core.Entities.Timesheet paidTimesheet = await context.Timesheets
            .SingleAsync(p_timesheet => p_timesheet.Id == seedResult.TimesheetId);
        Assert.True(paidTimesheet.IsPaid);
    }

    [Fact]
    public async Task GenerateForPeriod_Returns200OK_CreatesDraftStubAndIsIdempotent()
    {
        await AuthenticateAsAdminAsync();

        PayrollSeedResult seedResult = await SeedPayrollScenarioAsync();

        GeneratePayrollForPeriodRequest request = new()
        {
            PayPeriodId = seedResult.PayPeriodId
        };

        HttpResponseMessage firstResponse = await m_client.PostAsJsonAsync("/api/payroll/generate-period", request);

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        GeneratePayrollForPeriodResponseDto? firstBody =
            await firstResponse.Content.ReadFromJsonAsync<GeneratePayrollForPeriodResponseDto>();
        Assert.NotNull(firstBody);
        Assert.Equal(seedResult.PayPeriodId, firstBody.PayPeriodId);
        Assert.True(firstBody.CreatedCount > 0);
        Assert.Contains(firstBody.PayStubs, p_stub =>
            p_stub.EmployeeProfileId == seedResult.EmployeeProfileId
            && p_stub.GrossPay == s_expectedGrossPay
            && !p_stub.IsPublished);

        HttpResponseMessage secondResponse = await m_client.PostAsJsonAsync("/api/payroll/generate-period", request);
        secondResponse.EnsureSuccessStatusCode();

        GeneratePayrollForPeriodResponseDto? secondBody =
            await secondResponse.Content.ReadFromJsonAsync<GeneratePayrollForPeriodResponseDto>();
        Assert.NotNull(secondBody);
        Assert.Equal(0, secondBody.CreatedCount);
        Assert.Equal(firstBody.CreatedCount + firstBody.ExistingCount, secondBody.ExistingCount);
    }

    [Fact]
    public async Task GenerateForPeriod_WithAdminAndNoLocation_GeneratesForAllLocations()
    {
        await AuthenticateAsAdminAsync();

        int firstLocationId = await CreateLocationAsync();
        int secondLocationId = await CreateLocationAsync();
        int payPeriodId = await SeedPayPeriodAsync();
        PayrollSeedResult firstSeedResult = await SeedPayrollScenarioAsync(
            p_payPeriodId: payPeriodId,
            p_locationId: firstLocationId);
        PayrollSeedResult secondSeedResult = await SeedPayrollScenarioAsync(
            p_payPeriodId: payPeriodId,
            p_locationId: secondLocationId);

        GeneratePayrollForPeriodRequest request = new()
        {
            PayPeriodId = payPeriodId,
            LocationId = null
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/payroll/generate-period", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        GeneratePayrollForPeriodResponseDto? body =
            await response.Content.ReadFromJsonAsync<GeneratePayrollForPeriodResponseDto>();
        Assert.NotNull(body);
        Assert.Null(body.LocationId);
        Assert.Contains(body.PayStubs, p_stub => p_stub.EmployeeProfileId == firstSeedResult.EmployeeProfileId);
        Assert.Contains(body.PayStubs, p_stub => p_stub.EmployeeProfileId == secondSeedResult.EmployeeProfileId);
    }

    [Fact]
    public async Task Generate_Returns409Conflict_WhenTimesheetIsNotApproved()
    {
        await AuthenticateAsAdminAsync();

        PayrollSeedResult seedResult = await SeedPayrollScenarioAsync(p_approveTimesheet: false);

        GeneratePayrollRequest request = new()
        {
            PayPeriodId = seedResult.PayPeriodId,
            EmployeeProfileId = seedResult.EmployeeProfileId
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/payroll/generate", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Generate_Returns409Conflict_WhenNoActiveContract()
    {
        await AuthenticateAsAdminAsync();

        PayrollSeedResult seedResult = await SeedPayrollScenarioAsync(p_includeContract: false);

        GeneratePayrollRequest request = new()
        {
            PayPeriodId = seedResult.PayPeriodId,
            EmployeeProfileId = seedResult.EmployeeProfileId
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/payroll/generate", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Generate_Returns403Forbidden_WithEmployeeToken()
    {
        await AuthenticateAsAdminAsync();
        PayrollSeedResult seedResult = await SeedPayrollScenarioAsync();
        await AuthenticateAsEmployeeAsync();

        GeneratePayrollRequest request = new()
        {
            PayPeriodId = seedResult.PayPeriodId,
            EmployeeProfileId = seedResult.EmployeeProfileId
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/payroll/generate", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GenerateForPeriod_Returns403Forbidden_WhenManagerRequestsAnotherLocation()
    {
        int otherLocationId = await GetLocationIdOutsideGerantScopeAsync();
        await AuthenticateAsync("gerant@crystal.local");

        GeneratePayrollForPeriodRequest request = new()
        {
            PayPeriodId = 1,
            LocationId = otherLocationId,
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/payroll/generate-period", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task CreatePeriod_Returns400BadRequest_WhenPeriodIsNotComplete()
    {
        await AuthenticateAsAdminAsync();

        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        CreatePayPeriodRequest request = new()
        {
            StartDate = today,
            EndDate = today,
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/payroll/periods", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreatePeriod_Returns201Created_WithCreatedPeriodDto()
    {
        await AuthenticateAsAdminAsync();

        DateOnly periodStart = s_periodStart.AddDays(-7);
        CreatePayPeriodRequest request = new()
        {
            StartDate = periodStart,
            EndDate = periodStart.AddDays(6),
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/payroll/periods", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        PayPeriodResponseDto? body = await response.Content.ReadFromJsonAsync<PayPeriodResponseDto>();
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal(request.StartDate, body.StartDate);
        Assert.Equal(request.EndDate, body.EndDate);
    }

    [Fact]
    public async Task GetStubs_Returns200OK_WithEmployeeToken()
    {
        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/payroll/stubs");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<PayrollSeedResult> SeedPayrollScenarioAsync(
        bool p_approveTimesheet = true,
        bool p_includeContract = true,
        int? p_payPeriodId = null,
        int? p_locationId = null)
    {
        HrSeedResult hrSeed = await SeedHrReferenceDataAsync(p_locationId);
        int payPeriodId = p_payPeriodId ?? await SeedPayPeriodAsync();

        if (p_includeContract)
        {
            await CreateEmploymentContractAsync(hrSeed.EmployeeProfileId);
        }

        List<int> timeEntryIds = await CreateTimeEntriesAsync(hrSeed);
        int timesheetId = await CreateTimesheetAsync(hrSeed.EmployeeProfileId, timeEntryIds);

        if (p_approveTimesheet)
        {
            HttpResponseMessage submitResponse = await m_client.PatchAsJsonAsync(
                $"/api/timesheets/{timesheetId}/status",
                new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Submitted });

            submitResponse.EnsureSuccessStatusCode();

            HttpResponseMessage approveResponse = await m_client.PatchAsJsonAsync(
                $"/api/timesheets/{timesheetId}/status",
                new UpdateTimesheetStatusRequest { Status = TimesheetStatus.Approved });

            approveResponse.EnsureSuccessStatusCode();
        }

        return new PayrollSeedResult(
            hrSeed.EmployeeProfileId,
            hrSeed.EmployeeFirstName,
            hrSeed.EmployeeLastName,
            payPeriodId,
            timesheetId);
    }

    private async Task<int> SeedPayPeriodAsync()
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.PayPeriod payPeriod = new()
        {
            StartDate = s_periodStart,
            EndDate = s_periodEnd,
            IsProcessed = false
        };

        await context.PayPeriods.AddAsync(payPeriod);
        await context.SaveChangesAsync();

        return payPeriod.Id;
    }

    private async Task<int> GetLocationIdOutsideGerantScopeAsync()
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.ApplicationUser gerant = await context.Users
            .SingleAsync(p_user => p_user.Email == "gerant@crystal.local");

        Crystal.Core.Entities.EmployeeProfile? gerantProfile = await context.EmployeeProfiles
            .FirstOrDefaultAsync(p_profile => p_profile.ApplicationUserId == gerant.Id);

        if (gerantProfile is null)
        {
            Crystal.Core.Entities.Location location = await context.Locations.FirstAsync();
            Crystal.Core.Entities.JobPosition jobPosition = new()
            {
                Name = $"JobPosition-{Guid.NewGuid()}",
                Description = "Poste pour test de scoping paie",
                IsDeleted = false
            };

            await context.JobPositions.AddAsync(jobPosition);
            await context.SaveChangesAsync();

            gerantProfile = new Crystal.Core.Entities.EmployeeProfile
            {
                FirstName = "Gabriel",
                LastName = "Gerant",
                Email = "gerant@crystal.local",
                ApplicationUserId = gerant.Id,
                Salary = 65000m,
                Status = "Active",
                PositionId = jobPosition.Id,
                HiringDate = new DateOnly(2024, 1, 15),
                LocationId = location.Id,
                IsDeleted = false
            };

            await context.EmployeeProfiles.AddAsync(gerantProfile);
            await context.SaveChangesAsync();
        }

        Crystal.Core.Entities.Location? otherLocation = await context.Locations
            .FirstOrDefaultAsync(p_location => p_location.Id != gerantProfile.LocationId);

        if (otherLocation is not null)
        {
            return otherLocation.Id;
        }

        Crystal.Core.Entities.Location createdLocation = new()
        {
            Title = $"Test Branch {Guid.NewGuid():N}",
            Address = "Adresse test",
            Description = "Branch for scoping test",
        };

        await context.Locations.AddAsync(createdLocation);
        await context.SaveChangesAsync();

        return createdLocation.Id;
    }

    private async Task<int> CreateLocationAsync()
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.Location location = new()
        {
            Title = $"Test Branch {Guid.NewGuid():N}",
            Address = "Adresse test",
            Description = "Branch for payroll tests"
        };

        await context.Locations.AddAsync(location);
        await context.SaveChangesAsync();

        return location.Id;
    }

    private async Task CreateEmploymentContractAsync(int p_employeeProfileId)
    {
        CreateEmploymentContractRequest request = new()
        {
            EmployeeProfileId = p_employeeProfileId,
            ContractType = ContractType.FullTime,
            WageType = WageType.Monthly,
            BaseRate = s_hourlyRate,
            StartDate = new DateOnly(2020, 1, 1),
            EndDate = null
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/contracts", request);
        response.EnsureSuccessStatusCode();
    }

    private async Task<List<int>> CreateTimeEntriesAsync(HrSeedResult p_hrSeed)
    {
        List<int> timeEntryIds = new List<int>();

        CreateTimeEntryRequest firstEntry = new()
        {
            EmployeeProfileId = p_hrSeed.EmployeeProfileId,
            ScheduledShiftId = p_hrSeed.ScheduledShiftId,
            Date = s_periodStart.AddDays(4),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0)
        };

        HttpResponseMessage firstResponse = await m_client.PostAsJsonAsync("/api/time-entries", firstEntry);
        firstResponse.EnsureSuccessStatusCode();

        TimeEntryResponseDto? firstCreated = await firstResponse.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
        Assert.NotNull(firstCreated);
        timeEntryIds.Add(firstCreated.Id);

        CreateTimeEntryRequest secondEntry = new()
        {
            EmployeeProfileId = p_hrSeed.EmployeeProfileId,
            ScheduledShiftId = null,
            Date = s_periodStart.AddDays(5),
            StartTime = new TimeOnly(8, 0),
            EndTime = new TimeOnly(16, 0)
        };

        HttpResponseMessage secondResponse = await m_client.PostAsJsonAsync("/api/time-entries", secondEntry);
        secondResponse.EnsureSuccessStatusCode();

        TimeEntryResponseDto? secondCreated = await secondResponse.Content.ReadFromJsonAsync<TimeEntryResponseDto>();
        Assert.NotNull(secondCreated);
        timeEntryIds.Add(secondCreated.Id);

        return timeEntryIds;
    }

    private async Task<int> CreateTimesheetAsync(int p_employeeProfileId, IList<int> p_timeEntryIds)
    {
        CreateTimesheetRequest request = new()
        {
            EmployeeProfileId = p_employeeProfileId,
            PeriodStart = s_periodStart,
            PeriodEnd = s_periodEnd,
            TimeEntryIds = p_timeEntryIds
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/timesheets", request);
        response.EnsureSuccessStatusCode();

        TimesheetResponseDto? created = await response.Content.ReadFromJsonAsync<TimesheetResponseDto>();
        Assert.NotNull(created);
        return created.Id;
    }

    private async Task<HrSeedResult> SeedHrReferenceDataAsync(int? p_locationId = null)
    {
        string uniqueSuffix = Guid.NewGuid().ToString();
        string jobPositionName = $"JobPosition-{uniqueSuffix}";
        string employeeEmail = $"employee-{uniqueSuffix}@test.local";
        string employeeFirstName = "Payroll";
        string employeeLastName = $"Test-{uniqueSuffix}";

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.JobPosition jobPosition = new()
        {
            Name = jobPositionName,
            Description = "Poste pour tests de paie",
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

        Crystal.Core.Entities.ScheduledShift scheduledShift = new()
        {
            EmployeeProfileId = employeeProfile.Id,
            JobPositionId = jobPosition.Id,
            Date = s_periodStart.AddDays(4),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            IsDeleted = false
        };

        await context.ScheduledShifts.AddAsync(scheduledShift);
        await context.SaveChangesAsync();

        return new HrSeedResult(
            employeeProfile.Id,
            employeeFirstName,
            employeeLastName,
            scheduledShift.Id);
    }

    private static DateOnly GetLastCompleteWeekMonday()
    {
        DateOnly today = DateOnly.FromDateTime(DateTime.Today);
        int daysSinceMonday = today.DayOfWeek == DayOfWeek.Sunday
            ? 6
            : (int)today.DayOfWeek - (int)DayOfWeek.Monday;

        DateOnly currentWeekMonday = today.AddDays(-daysSinceMonday);
        return currentWeekMonday.AddDays(-7);
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
        int ScheduledShiftId);

    private sealed record PayrollSeedResult(
        int EmployeeProfileId,
        string EmployeeFirstName,
        string EmployeeLastName,
        int PayPeriodId,
        int TimesheetId);
}
