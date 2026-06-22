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

namespace Crystal.IntegrationTests.EmployeePortal;

/// <summary>
/// Vérifie les critères d'acceptation Phase 3 — scoping « mes données ».
/// </summary>
public sealed class Phase3EmployeeScopingIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public Phase3EmployeeScopingIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task LeaveRequests_GetAll_ReturnsOnlyOwnRequests_ForEmployee()
    {
        ScopingSeedResult seed = await SeedTwoEmployeeProfilesAsync();

        await AuthenticateAsync("employee@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/leave-requests");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<LeaveRequestResponseDto>? leaves =
            await response.Content.ReadFromJsonAsync<List<LeaveRequestResponseDto>>();
        Assert.NotNull(leaves);
        Assert.All(leaves, p_item => Assert.Equal(seed.EmployeeAProfileId, p_item.EmployeeProfileId));
        Assert.DoesNotContain(leaves, p_item => p_item.EmployeeProfileId == seed.EmployeeBProfileId);
    }

    [Fact]
    public async Task LeaveRequests_GetById_Returns404_WhenEmployeeAccessesColleagueRequest()
    {
        ScopingSeedResult seed = await SeedTwoEmployeeProfilesAsync();

        await AuthenticateAsync("employee@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync($"/api/leave-requests/{seed.EmployeeBLeaveRequestId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task LeaveRequests_GetAll_ReturnsAllRequests_ForGerant()
    {
        ScopingSeedResult seed = await SeedTwoEmployeeProfilesAsync();

        await AuthenticateAsync("gerant@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/leave-requests");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<LeaveRequestResponseDto>? leaves =
            await response.Content.ReadFromJsonAsync<List<LeaveRequestResponseDto>>();
        Assert.NotNull(leaves);
        Assert.Contains(leaves, p_item => p_item.EmployeeProfileId == seed.EmployeeAProfileId);
        Assert.Contains(leaves, p_item => p_item.EmployeeProfileId == seed.EmployeeBProfileId);
    }

    [Fact]
    public async Task EmployeeProfiles_GetAll_Returns403_ForEmployee()
    {
        await AuthenticateAsync("employee@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/employee-profiles");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Contracts_GetAll_ReturnsOnlyOwnContracts_ForEmployee()
    {
        ScopingSeedResult seed = await SeedTwoEmployeeProfilesAsync();

        await AuthenticateAsync("employee@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/contracts");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<EmploymentContractResponseDto>? contracts =
            await response.Content.ReadFromJsonAsync<List<EmploymentContractResponseDto>>();
        Assert.NotNull(contracts);
        Assert.Contains(contracts, p_item => p_item.EmployeeProfileId == seed.EmployeeAProfileId);
        Assert.DoesNotContain(contracts, p_item => p_item.EmployeeProfileId == seed.EmployeeBProfileId);
    }

    [Fact]
    public async Task Contracts_GetById_Returns404_WhenEmployeeAccessesColleagueContract()
    {
        ScopingSeedResult seed = await SeedTwoEmployeeProfilesAsync();

        await AuthenticateAsync("employee@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync($"/api/contracts/{seed.EmployeeBContractId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task PayStubs_GetAll_ReturnsOnlyOwnPublishedStubs_ForEmployee()
    {
        ScopingSeedResult seed = await SeedTwoEmployeeProfilesAsync();

        await AuthenticateAsync("employee@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/payroll/stubs");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<PayStubResponseDto>? payStubs =
            await response.Content.ReadFromJsonAsync<List<PayStubResponseDto>>();
        Assert.NotNull(payStubs);
        Assert.Contains(payStubs, p_item =>
            p_item.EmployeeProfileId == seed.EmployeeAProfileId
            && p_item.GrossPay == seed.EmployeeAPublishedGrossPay
            && p_item.IsPublished);
        Assert.DoesNotContain(payStubs, p_item => p_item.EmployeeProfileId == seed.EmployeeBProfileId);
        Assert.DoesNotContain(payStubs, p_item =>
            p_item.EmployeeProfileId == seed.EmployeeAProfileId
            && p_item.GrossPay == seed.EmployeeADraftGrossPay);
        Assert.All(payStubs, p_item => Assert.True(p_item.IsPublished));
    }

    [Fact]
    public async Task Schedules_GetAll_ReturnsOnlyOwnShifts_ForEmployee()
    {
        ScopingSeedResult seed = await SeedTwoEmployeeProfilesAsync();

        await AuthenticateAsync("employee@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/schedules");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<ScheduledShiftResponseDto>? shifts =
            await response.Content.ReadFromJsonAsync<List<ScheduledShiftResponseDto>>();
        Assert.NotNull(shifts);
        Assert.All(shifts, p_item => Assert.Equal(seed.EmployeeAProfileId, p_item.EmployeeProfileId));
    }

    [Fact]
    public async Task Schedules_GetTeam_ReturnsOnlyOwnLocationShifts_ForEmployee()
    {
        ScopingSeedResult seed = await SeedTwoEmployeeProfilesAsync();

        await AuthenticateAsync("employee@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/schedules/team");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<ScheduledShiftResponseDto>? shifts =
            await response.Content.ReadFromJsonAsync<List<ScheduledShiftResponseDto>>();
        Assert.NotNull(shifts);
        Assert.Contains(shifts, p_item => p_item.EmployeeProfileId == seed.EmployeeAProfileId);
        Assert.DoesNotContain(shifts, p_item => p_item.EmployeeProfileId == seed.EmployeeBProfileId);
        Assert.Contains(shifts, p_item =>
            p_item.EmployeeProfileId == seed.EmployeeAProfileId
            && p_item.LocationId == seed.EmployeeALocationId
            && p_item.LocationTitle == seed.EmployeeALocationTitle);
    }

    private async Task<ScopingSeedResult> SeedTwoEmployeeProfilesAsync()
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        ApplicationUser? employeeA = await userManager.FindByEmailAsync("employee@crystal.local");
        ApplicationUser? employeeB = await userManager.FindByEmailAsync("assistant@crystal.local");
        Assert.NotNull(employeeA);
        Assert.NotNull(employeeB);

        Crystal.Core.Entities.Location locationA = new()
        {
            Title = $"Location-scope-A-{Guid.NewGuid():N}",
            Address = "1 rue Scope",
            Description = "Branch A"
        };
        Crystal.Core.Entities.Location locationB = new()
        {
            Title = $"Location-scope-B-{Guid.NewGuid():N}",
            Address = "2 rue Scope",
            Description = "Branch B"
        };
        await context.Locations.AddRangeAsync(locationA, locationB);
        await context.SaveChangesAsync();

        Crystal.Core.Entities.EmployeeProfile? profileA = await context.EmployeeProfiles
            .FirstOrDefaultAsync(p_ep => p_ep.ApplicationUserId == employeeA.Id);
        Crystal.Core.Entities.EmployeeProfile? profileB = await context.EmployeeProfiles
            .FirstOrDefaultAsync(p_ep => p_ep.ApplicationUserId == employeeB.Id);

        if (profileA is null || profileB is null)
        {
            Crystal.Core.Entities.JobPosition jobPosition = new()
            {
                Name = $"Poste-scope-{Guid.NewGuid():N}",
                Description = "Test"
            };
            await context.JobPositions.AddAsync(jobPosition);
            await context.SaveChangesAsync();

            if (profileA is null)
            {
                profileA = new Crystal.Core.Entities.EmployeeProfile
                {
                    FirstName = "Alice",
                    LastName = "ScopeA",
                    Email = employeeA.Email ?? $"alice-scope-{Guid.NewGuid():N}@test.local",
                    ApplicationUserId = employeeA.Id,
                    Salary = 45000m,
                    Status = "Active",
                    PositionId = jobPosition.Id,
                    HiringDate = new DateOnly(2024, 1, 1),
                    LocationId = locationA.Id,
                };
                await context.EmployeeProfiles.AddAsync(profileA);
            }

            if (profileB is null)
            {
                profileB = new Crystal.Core.Entities.EmployeeProfile
                {
                    FirstName = "Bob",
                    LastName = "ScopeB",
                    Email = employeeB.Email ?? $"bob-scope-{Guid.NewGuid():N}@test.local",
                    ApplicationUserId = employeeB.Id,
                    Salary = 46000m,
                    Status = "Active",
                    PositionId = jobPosition.Id,
                    HiringDate = new DateOnly(2024, 2, 1),
                    LocationId = locationB.Id,
                };
                await context.EmployeeProfiles.AddAsync(profileB);
            }

            await context.SaveChangesAsync();
        }

        Assert.NotNull(profileA);
        Assert.NotNull(profileB);

        profileA.LocationId = locationA.Id;
        profileB.LocationId = locationB.Id;
        await context.SaveChangesAsync();

        int jobPositionId = profileA.PositionId;

        Crystal.Core.Entities.LeaveRequest leaveA = new()
        {
            EmployeeProfileId = profileA.Id,
            LeaveType = LeaveType.Vacation,
            Status = LeaveRequestStatus.Pending,
            StartDate = new DateOnly(2026, 8, 1),
            EndDate = new DateOnly(2026, 8, 5),
            Reason = "Vacances A",
        };

        Crystal.Core.Entities.LeaveRequest leaveB = new()
        {
            EmployeeProfileId = profileB.Id,
            LeaveType = LeaveType.Sick,
            Status = LeaveRequestStatus.Pending,
            StartDate = new DateOnly(2026, 9, 1),
            EndDate = new DateOnly(2026, 9, 3),
            Reason = "Maladie B",
        };

        Crystal.Core.Entities.ScheduledShift shiftA = new()
        {
            EmployeeProfileId = profileA.Id,
            JobPositionId = jobPositionId,
            LocationId = locationA.Id,
            Date = new DateOnly(2026, 10, 15),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
        };

        Crystal.Core.Entities.ScheduledShift shiftB = new()
        {
            EmployeeProfileId = profileB.Id,
            JobPositionId = jobPositionId,
            LocationId = locationB.Id,
            Date = new DateOnly(2026, 10, 16),
            StartTime = new TimeOnly(10, 0),
            EndTime = new TimeOnly(18, 0),
        };

        Crystal.Core.Entities.EmploymentContract contractA = new()
        {
            EmployeeProfileId = profileA.Id,
            ContractType = ContractType.FullTime,
            WageType = WageType.Fixed,
            BaseRate = 50000m,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31),
        };

        Crystal.Core.Entities.EmploymentContract contractB = new()
        {
            EmployeeProfileId = profileB.Id,
            ContractType = ContractType.PartTime,
            WageType = WageType.Monthly,
            BaseRate = 25m,
            StartDate = new DateOnly(2026, 1, 1),
            EndDate = new DateOnly(2026, 12, 31),
        };

        Crystal.Core.Entities.PayPeriod payPeriod = new()
        {
            StartDate = new DateOnly(2026, 11, 1),
            EndDate = new DateOnly(2026, 11, 7),
            IsProcessed = true,
        };

        await context.LeaveRequests.AddRangeAsync(leaveA, leaveB);
        await context.ScheduledShifts.AddRangeAsync(shiftA, shiftB);
        await context.EmploymentContracts.AddRangeAsync(contractA, contractB);
        await context.PayPeriods.AddAsync(payPeriod);
        await context.SaveChangesAsync();

        decimal employeeAPublishedGrossPay = 1111.11m;
        decimal employeeADraftGrossPay = 2222.22m;

        Crystal.Core.Entities.PayStub publishedStubA = new()
        {
            EmployeeProfileId = profileA.Id,
            PayPeriodId = payPeriod.Id,
            TotalHours = 40m,
            GrossPay = employeeAPublishedGrossPay,
            IsPublished = true,
        };

        Crystal.Core.Entities.PayStub draftStubA = new()
        {
            EmployeeProfileId = profileA.Id,
            PayPeriodId = payPeriod.Id,
            TotalHours = 40m,
            GrossPay = employeeADraftGrossPay,
            IsPublished = false,
        };

        Crystal.Core.Entities.PayStub publishedStubB = new()
        {
            EmployeeProfileId = profileB.Id,
            PayPeriodId = payPeriod.Id,
            TotalHours = 24m,
            GrossPay = 3333.33m,
            IsPublished = true,
        };

        await context.PayStubs.AddRangeAsync(publishedStubA, draftStubA, publishedStubB);
        await context.SaveChangesAsync();

        return new ScopingSeedResult(
            profileA.Id,
            profileB.Id,
            leaveB.Id,
            contractB.Id,
            employeeAPublishedGrossPay,
            employeeADraftGrossPay,
            locationA.Id,
            locationA.Title);
    }

    private async Task AuthenticateAsync(string p_email)
    {
        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = p_email,
            Password = "ValidPass1!a",
        });
        response.EnsureSuccessStatusCode();

        LoginResponse? login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);

        m_client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Token);
    }

    public void Dispose()
    {
        m_client.Dispose();
    }

    private sealed class LoginResponse
    {
        public string Token { get; set; } = string.Empty;
    }

    private sealed record ScopingSeedResult(
        int EmployeeAProfileId,
        int EmployeeBProfileId,
        int EmployeeBLeaveRequestId,
        int EmployeeBContractId,
        decimal EmployeeAPublishedGrossPay,
        decimal EmployeeADraftGrossPay,
        int EmployeeALocationId,
        string EmployeeALocationTitle);
}
