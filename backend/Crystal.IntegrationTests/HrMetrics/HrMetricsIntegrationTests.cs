using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Enums;
using Crystal.Infrastructure.Context;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Crystal.IntegrationTests.HrMetrics;

public sealed class HrMetricsIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public HrMetricsIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetDashboardMetrics_Returns200OK_WithCorrectAggregations()
    {
        MetricsSeedResult seedResult = await SeedMetricsDataAsync();
        await AuthenticateAsAdminAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/hr/metrics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        HrDashboardMetricsDto? body = await response.Content.ReadFromJsonAsync<HrDashboardMetricsDto>();
        Assert.NotNull(body);
        Assert.True(body.TotalActiveEmployees >= seedResult.ExpectedActiveEmployees);
        Assert.True(body.PendingTimesheetsCount >= seedResult.ExpectedSubmittedTimesheets);
        Assert.True(body.PendingLeaveRequestsCount >= seedResult.ExpectedPendingLeaveRequests);
        Assert.True(body.TotalGrossPayroll >= seedResult.ExpectedTotalGrossPayroll);
    }

    [Fact]
    public async Task GetDashboardMetrics_Returns403Forbidden_WithEmployeeToken()
    {
        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/hr/metrics");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private async Task<MetricsSeedResult> SeedMetricsDataAsync()
    {
        string uniqueSuffix = Guid.NewGuid().ToString();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.JobPosition jobPosition = new()
        {
            Name = $"JobPosition-{uniqueSuffix}",
            Description = "Poste pour tests métriques RH",
            IsDeleted = false
        };

        await context.JobPositions.AddAsync(jobPosition);
        await context.SaveChangesAsync();

        Crystal.Core.Entities.EmployeeProfile employeeProfile = new()
        {
            FirstName = "Metrics",
            LastName = $"Test-{uniqueSuffix}",
            Email = $"metrics-{uniqueSuffix}@test.local",
            Salary = 50000m,
            Status = "Active",
            PositionId = jobPosition.Id,
            HiringDate = new DateOnly(2024, 1, 1),
            IsDeleted = false
        };

        await context.EmployeeProfiles.AddAsync(employeeProfile);
        await context.SaveChangesAsync();

        Crystal.Core.Entities.Timesheet submittedTimesheet = new()
        {
            EmployeeProfileId = employeeProfile.Id,
            PeriodStart = new DateOnly(2026, 8, 1),
            PeriodEnd = new DateOnly(2026, 8, 31),
            Status = TimesheetStatus.Submitted,
            IsDeleted = false
        };

        await context.Timesheets.AddAsync(submittedTimesheet);
        await context.SaveChangesAsync();

        Crystal.Core.Entities.LeaveRequest pendingLeaveRequest = new()
        {
            EmployeeProfileId = employeeProfile.Id,
            LeaveType = LeaveType.Vacation,
            Status = LeaveRequestStatus.Pending,
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 9, 5),
            Reason = "Vacances test mÃ©triques",
            IsDeleted = false
        };

        await context.LeaveRequests.AddAsync(pendingLeaveRequest);
        await context.SaveChangesAsync();

        Crystal.Core.Entities.PayPeriod payPeriod = new()
        {
            StartDate = new DateOnly(2026, 8, 1),
            EndDate = new DateOnly(2026, 8, 31),
            IsProcessed = false
        };

        await context.PayPeriods.AddAsync(payPeriod);
        await context.SaveChangesAsync();

        Crystal.Core.Entities.PayStub payStub = new()
        {
            PayPeriodId = payPeriod.Id,
            EmployeeProfileId = employeeProfile.Id,
            TotalHours = 40m,
            GrossPay = 1000m,
            IsDeleted = false
        };

        await context.PayStubs.AddAsync(payStub);
        await context.SaveChangesAsync();

        return new MetricsSeedResult(
            ExpectedActiveEmployees: 1,
            ExpectedSubmittedTimesheets: 1,
            ExpectedPendingLeaveRequests: 1,
            ExpectedTotalGrossPayroll: 1000m);
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

    private sealed record MetricsSeedResult(
        int ExpectedActiveEmployees,
        int ExpectedSubmittedTimesheets,
        int ExpectedPendingLeaveRequests,
        decimal ExpectedTotalGrossPayroll);
}
