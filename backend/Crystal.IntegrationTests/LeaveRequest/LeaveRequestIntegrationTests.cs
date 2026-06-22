using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Enums;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Crystal.IntegrationTests.LeaveRequest;

public sealed class LeaveRequestIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    private static readonly DateOnly s_existingStart = new(2026, 7, 1);
    private static readonly DateOnly s_existingEnd = new(2026, 7, 10);

    public LeaveRequestIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns200OK_WithEmployeeToken()
    {
        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/leave-requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created_WithPendingStatus()
    {
        await AuthenticateAsEmployeeAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync(p_linkToEmployeeAccount: true);
        CreateLeaveRequestDto request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            s_existingStart,
            s_existingEnd,
            LeaveType.Vacation);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/leave-requests", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        LeaveRequestResponseDto? body = await response.Content.ReadFromJsonAsync<LeaveRequestResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("Pending", body.Status);
        Assert.Equal("Vacation", body.LeaveType);
        Assert.Equal(seedResult.EmployeeFirstName, body.EmployeeFirstName);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenOverlappingStart()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await CreateLeaveRequestAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        CreateLeaveRequestDto request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2026, 7, 5),
            new DateOnly(2026, 7, 15),
            LeaveType.Sick);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/leave-requests", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenOverlappingEnd()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await CreateLeaveRequestAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        CreateLeaveRequestDto request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2026, 6, 25),
            new DateOnly(2026, 7, 5),
            LeaveType.Sick);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/leave-requests", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenCompletelyEnclosed()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await CreateLeaveRequestAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        CreateLeaveRequestDto request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2026, 7, 3),
            new DateOnly(2026, 7, 7),
            LeaveType.Unpaid);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/leave-requests", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenCompletelyEnclosesExisting()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await CreateLeaveRequestAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        CreateLeaveRequestDto request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2026, 6, 1),
            new DateOnly(2026, 8, 31),
            LeaveType.Other);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/leave-requests", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created_WhenNonOverlappingAfterRejectedLeave()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int leaveRequestId = await CreateLeaveRequestAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        await m_client.PatchAsJsonAsync(
            $"/api/leave-requests/{leaveRequestId}/status",
            new UpdateLeaveRequestStatusDto { Status = LeaveRequestStatus.Rejected });

        CreateLeaveRequestDto request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            s_existingStart,
            s_existingEnd,
            LeaveType.Vacation);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/leave-requests", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created_WhenNonOverlappingAdjacentPeriod()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        await CreateLeaveRequestAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        CreateLeaveRequestDto request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2026, 7, 11),
            new DateOnly(2026, 7, 20),
            LeaveType.Vacation);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/leave-requests", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns400BadRequest_WhenEndDateIsBeforeStartDate()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();

        CreateLeaveRequestDto request = BuildCreateRequest(
            seedResult.EmployeeProfileId,
            new DateOnly(2026, 7, 10),
            new DateOnly(2026, 7, 1),
            LeaveType.Vacation);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/leave-requests", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_ApprovalFlow_PendingToApproved()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int leaveRequestId = await CreateLeaveRequestAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        HttpResponseMessage response = await m_client.PatchAsJsonAsync(
            $"/api/leave-requests/{leaveRequestId}/status",
            new UpdateLeaveRequestStatusDto { Status = LeaveRequestStatus.Approved });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        LeaveRequestResponseDto? body = await response.Content.ReadFromJsonAsync<LeaveRequestResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("Approved", body.Status);
    }

    [Fact]
    public async Task UpdateStatus_Returns409Conflict_WhenApprovingAlreadyApproved()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int leaveRequestId = await CreateLeaveRequestAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        await m_client.PatchAsJsonAsync(
            $"/api/leave-requests/{leaveRequestId}/status",
            new UpdateLeaveRequestStatusDto { Status = LeaveRequestStatus.Approved });

        HttpResponseMessage response = await m_client.PatchAsJsonAsync(
            $"/api/leave-requests/{leaveRequestId}/status",
            new UpdateLeaveRequestStatusDto { Status = LeaveRequestStatus.Rejected });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task UpdateStatus_Returns403Forbidden_WithEmployeeToken()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int leaveRequestId = await CreateLeaveRequestAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage response = await m_client.PatchAsJsonAsync(
            $"/api/leave-requests/{leaveRequestId}/status",
            new UpdateLeaveRequestStatusDto { Status = LeaveRequestStatus.Approved });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns403Forbidden_WithEmployeeToken()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int leaveRequestId = await CreateLeaveRequestAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage response = await m_client.DeleteAsync($"/api/leave-requests/{leaveRequestId}");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Delete_Returns204NoContent_WithAdminToken()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        int leaveRequestId = await CreateLeaveRequestAsync(seedResult.EmployeeProfileId, s_existingStart, s_existingEnd);

        HttpResponseMessage response = await m_client.DeleteAsync($"/api/leave-requests/{leaveRequestId}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.LeaveRequest? deleted = await context.LeaveRequests
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p_request => p_request.Id == leaveRequestId);

        Assert.NotNull(deleted);
        Assert.True(deleted.IsDeleted);
    }

    [Fact]
    public async Task GetAll_SoftDeletesLeaveRequestsTheDayAfterEndDate()
    {
        await AuthenticateAsAdminAsync();

        HrSeedResult seedResult = await SeedHrReferenceDataAsync();
        DateOnly yesterday = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1));
        DateOnly twoDaysAgo = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-2));
        DateOnly tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1));
        DateOnly nextWeek = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7));

        int expiredLeaveRequestId = await CreateLeaveRequestAsync(
            seedResult.EmployeeProfileId,
            twoDaysAgo,
            yesterday);
        int activeLeaveRequestId = await CreateLeaveRequestAsync(
            seedResult.EmployeeProfileId,
            tomorrow,
            nextWeek);

        HttpResponseMessage response = await m_client.GetAsync("/api/leave-requests");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<LeaveRequestResponseDto>? body =
            await response.Content.ReadFromJsonAsync<List<LeaveRequestResponseDto>>();
        Assert.NotNull(body);
        Assert.DoesNotContain(body, p_request => p_request.Id == expiredLeaveRequestId);
        Assert.Contains(body, p_request => p_request.Id == activeLeaveRequestId);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.LeaveRequest? expiredLeaveRequest = await context.LeaveRequests
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p_request => p_request.Id == expiredLeaveRequestId);

        Assert.NotNull(expiredLeaveRequest);
        Assert.True(expiredLeaveRequest.IsDeleted);
    }

    private async Task<int> CreateLeaveRequestAsync(int p_employeeProfileId, DateOnly p_startDate, DateOnly p_endDate)
    {
        CreateLeaveRequestDto request = BuildCreateRequest(
            p_employeeProfileId,
            p_startDate,
            p_endDate,
            LeaveType.Vacation);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/leave-requests", request);
        response.EnsureSuccessStatusCode();

        LeaveRequestResponseDto? created = await response.Content.ReadFromJsonAsync<LeaveRequestResponseDto>();
        Assert.NotNull(created);
        return created.Id;
    }

    private static CreateLeaveRequestDto BuildCreateRequest(
        int p_employeeProfileId,
        DateOnly p_startDate,
        DateOnly p_endDate,
        LeaveType p_leaveType)
    {
        return new CreateLeaveRequestDto
        {
            EmployeeProfileId = p_employeeProfileId,
            LeaveType = p_leaveType,
            StartDate = p_startDate,
            EndDate = p_endDate,
            Reason = "Test absence"
        };
    }

    private async Task<HrSeedResult> SeedHrReferenceDataAsync(bool p_linkToEmployeeAccount = false)
    {
        string uniqueSuffix = Guid.NewGuid().ToString();
        string jobPositionName = $"JobPosition-{uniqueSuffix}";
        string employeeEmail = $"employee-{uniqueSuffix}@test.local";
        string employeeFirstName = "Leave";
        string employeeLastName = $"Test-{uniqueSuffix}";

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.JobPosition jobPosition = new()
        {
            Name = jobPositionName,
            Description = "Poste pour tests d'absence",
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

        return new HrSeedResult(
            employeeProfile.Id,
            employeeFirstName,
            employeeLastName);
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
        string EmployeeLastName);
}
