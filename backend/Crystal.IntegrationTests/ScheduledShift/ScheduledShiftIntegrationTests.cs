using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Crystal.IntegrationTests.ScheduledShift;

public sealed class ScheduledShiftIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public ScheduledShiftIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns200OK_WithEmployeeToken()
    {
        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/schedules");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Returns404_WhenEmployeeAccessesAnotherShift()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        CreateScheduledShiftRequest createRequest = BuildCreateRequest(
            seedResult,
            new DateOnly(2026, 6, 10),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();

        ScheduledShiftResponseDto? created = await createResponse.Content.ReadFromJsonAsync<ScheduledShiftResponseDto>();
        Assert.NotNull(created);

        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage getResponse = await m_client.GetAsync($"/api/schedules/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created_WithAdminToken()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        CreateScheduledShiftRequest request = BuildCreateRequest(
            seedResult,
            new DateOnly(2026, 7, 1),
            new TimeOnly(8, 30),
            new TimeOnly(16, 30));

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/schedules", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        ScheduledShiftResponseDto? body = await response.Content.ReadFromJsonAsync<ScheduledShiftResponseDto>();
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal(seedResult.EmployeeProfileId, body.EmployeeProfileId);
        Assert.Equal(seedResult.JobPositionId, body.JobPositionId);
        Assert.Equal(seedResult.EmployeeFirstName, body.EmployeeFirstName);
        Assert.Equal(seedResult.JobPositionName, body.JobPositionName);
    }

    [Fact]
    public async Task Create_Returns403Forbidden_WithEmployeeToken()
    {
        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await AuthenticateAsEmployeeAsync();

        CreateScheduledShiftRequest request = BuildCreateRequest(
            seedResult,
            new DateOnly(2026, 7, 2),
            new TimeOnly(10, 0),
            new TimeOnly(18, 0));

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/schedules", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created_WithOpenShiftByPositionOnly()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        CreateScheduledShiftRequest request = BuildCreateRequest(
            seedResult,
            new DateOnly(2026, 7, 4),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));
        request.EmployeeProfileId = null;

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/schedules", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        ScheduledShiftResponseDto? body = await response.Content.ReadFromJsonAsync<ScheduledShiftResponseDto>();
        Assert.NotNull(body);
        Assert.Null(body.EmployeeProfileId);
        Assert.Equal(seedResult.LocationId, body.LocationId);
        Assert.Equal(seedResult.JobPositionId, body.JobPositionId);
        Assert.Equal(seedResult.JobPositionName, body.JobPositionName);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenEmployeeProfileDoesNotExist()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        CreateScheduledShiftRequest request = BuildCreateRequest(
            seedResult,
            new DateOnly(2026, 7, 3),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));
        request.EmployeeProfileId = 999_999;

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/schedules", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal("The specified employee profile was not found.", message);
    }

    [Fact]
    public async Task Update_Returns200OK_WithAdminToken()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        CreateScheduledShiftRequest createRequest = BuildCreateRequest(
            seedResult,
            new DateOnly(2026, 8, 1),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();

        ScheduledShiftResponseDto? created = await createResponse.Content.ReadFromJsonAsync<ScheduledShiftResponseDto>();
        Assert.NotNull(created);

        UpdateScheduledShiftRequest updateRequest = new()
        {
            EmployeeProfileId = seedResult.EmployeeProfileId,
            LocationId = seedResult.LocationId,
            JobPositionId = seedResult.JobPositionId,
            Date = new DateOnly(2026, 8, 2),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(18, 0)
        };

        HttpResponseMessage updateResponse = await m_client.PutAsJsonAsync(
            $"/api/schedules/{created.Id}",
            updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        ScheduledShiftResponseDto? updated = await updateResponse.Content.ReadFromJsonAsync<ScheduledShiftResponseDto>();
        Assert.NotNull(updated);
        Assert.Equal(new DateOnly(2026, 8, 2), updated.Date);
        Assert.Equal(new TimeOnly(10, 0), updated.StartTime);
        Assert.Equal(new TimeOnly(18, 0), updated.EndTime);
    }

    [Fact]
    public async Task MovingEmployeeToAnotherLocation_DoesNotMoveExistingShift()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        CreateScheduledShiftRequest createRequest = BuildCreateRequest(
            seedResult,
            new DateOnly(2026, 8, 3),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();
        ScheduledShiftResponseDto? created = await createResponse.Content.ReadFromJsonAsync<ScheduledShiftResponseDto>();
        Assert.NotNull(created);
        Assert.Equal(seedResult.LocationId, created.LocationId);

        int newLocationId = await CreateLocationAsync();
        UpdateEmployeeProfileRequest updateEmployeeRequest = new()
        {
            FirstName = seedResult.EmployeeFirstName,
            LastName = seedResult.EmployeeLastName,
            Email = seedResult.EmployeeEmail,
            Salary = 50000m,
            Status = "Active",
            JobPositionId = seedResult.JobPositionId,
            HiringDate = new DateOnly(2024, 1, 1),
            LocationId = newLocationId,
        };

        HttpResponseMessage employeeResponse = await m_client.PutAsJsonAsync(
            $"/api/employee-profiles/{seedResult.EmployeeProfileId}",
            updateEmployeeRequest);
        employeeResponse.EnsureSuccessStatusCode();

        HttpResponseMessage shiftResponse = await m_client.GetAsync($"/api/schedules/{created.Id}");
        shiftResponse.EnsureSuccessStatusCode();
        ScheduledShiftResponseDto? unchangedShift =
            await shiftResponse.Content.ReadFromJsonAsync<ScheduledShiftResponseDto>();
        Assert.NotNull(unchangedShift);
        Assert.Equal(seedResult.LocationId, unchangedShift.LocationId);
    }

    [Fact]
    public async Task Delete_Returns204NoContent_AndPerformsSoftDelete()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        CreateScheduledShiftRequest createRequest = BuildCreateRequest(
            seedResult,
            new DateOnly(2026, 9, 1),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0));

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/schedules", createRequest);
        createResponse.EnsureSuccessStatusCode();

        ScheduledShiftResponseDto? created = await createResponse.Content.ReadFromJsonAsync<ScheduledShiftResponseDto>();
        Assert.NotNull(created);

        HttpResponseMessage deleteResponse = await m_client.DeleteAsync($"/api/schedules/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        HttpResponseMessage getResponse = await m_client.GetAsync($"/api/schedules/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.ScheduledShift? deletedShift = await context.ScheduledShifts
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(p_shift => p_shift.Id == created.Id);

        Assert.NotNull(deletedShift);
        Assert.True(deletedShift.IsDeleted);
    }

    private async Task<HrSeedResult> SeedHrReferenceDataAsync()
    {
        string uniqueSuffix = Guid.NewGuid().ToString();
        string jobPositionName = $"JobPosition-{uniqueSuffix}";
        string employeeEmail = $"employee-{uniqueSuffix}@test.local";
        string employeeFirstName = "Test";
        string employeeLastName = $"Employee-{uniqueSuffix}";

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.JobPosition jobPosition = new()
        {
            Name = jobPositionName,
            Description = "Poste pour tests d'horaires",
            IsDeleted = false
        };

        await context.JobPositions.AddAsync(jobPosition);

        Crystal.Core.Entities.Location location = new()
        {
            Title = $"Location-{uniqueSuffix}",
            Address = "Adresse test",
            Description = "Branch for scheduled shift tests",
        };

        await context.Locations.AddAsync(location);
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
            LocationId = location.Id,
            IsDeleted = false
        };

        await context.EmployeeProfiles.AddAsync(employeeProfile);
        await context.SaveChangesAsync();

        return new HrSeedResult(
            employeeProfile.Id,
            employeeFirstName,
            employeeLastName,
            employeeEmail,
            jobPosition.Id,
            jobPositionName,
            location.Id);
    }

    private async Task<int> CreateLocationAsync()
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        Crystal.Core.Entities.Location location = new()
        {
            Title = $"Location-{Guid.NewGuid()}",
            Address = "Nouvelle adresse test",
            Description = "Nouvelle succursale pour tests d'horaires",
        };
        await context.Locations.AddAsync(location);
        await context.SaveChangesAsync();
        return location.Id;
    }

    private static CreateScheduledShiftRequest BuildCreateRequest(
        HrSeedResult p_seedResult,
        DateOnly p_date,
        TimeOnly p_startTime,
        TimeOnly p_endTime)
    {
        return new CreateScheduledShiftRequest
        {
            EmployeeProfileId = p_seedResult.EmployeeProfileId,
            LocationId = p_seedResult.LocationId,
            JobPositionId = p_seedResult.JobPositionId,
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
        string EmployeeEmail,
        int JobPositionId,
        string JobPositionName,
        int LocationId);
}
