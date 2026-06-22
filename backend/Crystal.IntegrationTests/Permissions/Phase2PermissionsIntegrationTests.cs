using Crystal.Core;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Crystal.IntegrationTests.Permissions;

/// <summary>
/// Vérifie les critères d'acceptation de la Phase 2 — permissions unifiées (API).
/// </summary>
public sealed class Phase2PermissionsIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public Phase2PermissionsIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetMyPermissions_ReturnsPresetPermissions_ForAdmin()
    {
        await AuthenticateAsync("admin@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/users/me/permissions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserPermissionsResponseDto? body = await response.Content.ReadFromJsonAsync<UserPermissionsResponseDto>();
        Assert.NotNull(body);
        Assert.Equal("Admin", body.RoleId);
        Assert.Contains(body.Permissions, p_rule =>
            p_rule.Action == "manage" && p_rule.Subject == "all");
    }

    [Fact]
    public async Task GetMyPermissions_ReturnsEmployeePermissions_WithoutHrDashboard()
    {
        await ResetEmployeeDynamicRoleAsync();
        await AuthenticateAsync("employee@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/users/me/permissions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserPermissionsResponseDto? body = await response.Content.ReadFromJsonAsync<UserPermissionsResponseDto>();
        Assert.NotNull(body);
        Assert.DoesNotContain(body.Permissions, p_rule => p_rule.Subject == "hr_dashboard");
        Assert.Contains(body.Permissions, p_rule =>
            p_rule.Action == "read" && p_rule.Subject == "scheduled_shift");
        Assert.Contains(body.Permissions, p_rule =>
            p_rule.Action == "read" && p_rule.Subject == "employment_contract");
        Assert.Contains(body.Permissions, p_rule =>
            p_rule.Action == "read" && p_rule.Subject == "payroll");
    }

    [Fact]
    public async Task Roles_GetAll_Returns200_ForAdmin()
    {
        await AuthenticateAsync("admin@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/roles");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<DynamicRoleResponseDto>? roles = await response.Content.ReadFromJsonAsync<List<DynamicRoleResponseDto>>();
        Assert.NotNull(roles);
        Assert.True(roles.Count >= 4);
        Assert.Contains(roles, p_role => p_role.Id == "Admin" && p_role.IsPreset);
    }

    [Fact]
    public async Task Roles_GetAll_Returns403_ForEmployee()
    {
        await AuthenticateAsync("employee@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/roles");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Roles_CreateCustomRole_Returns201_ForAdmin()
    {
        await AuthenticateAsync("admin@crystal.local");

        CreateDynamicRoleRequest request = new()
        {
            Name = $"Rôle test {Guid.NewGuid():N}",
            Permissions =
            [
                new PermissionRuleRequest { Action = "read", Subject = "item" },
                new PermissionRuleRequest { Action = "read", Subject = "location" },
            ],
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/roles", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        DynamicRoleResponseDto? created = await response.Content.ReadFromJsonAsync<DynamicRoleResponseDto>();
        Assert.NotNull(created);
        Assert.False(created.IsPreset);
        Assert.Equal(2, created.Permissions.Count);
    }

    [Fact]
    public async Task Roles_CreateFromPreset_Returns201_WithPresetPermissions()
    {
        await AuthenticateAsync("admin@crystal.local");

        CreateDynamicRoleRequest request = new()
        {
            Name = $"Copie assistant {Guid.NewGuid():N}",
            PresetId = "Assistant",
            Permissions = [],
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/roles", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        DynamicRoleResponseDto? created = await response.Content.ReadFromJsonAsync<DynamicRoleResponseDto>();
        Assert.NotNull(created);
        Assert.Contains(created.Permissions, p_rule =>
            p_rule.Action == "read" && p_rule.Subject == "hr_dashboard");
    }

    [Fact]
    public async Task Roles_DeletePreset_Returns409_ForAdmin()
    {
        await AuthenticateAsync("admin@crystal.local");

        HttpResponseMessage response = await m_client.DeleteAsync("/api/roles/Admin");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Roles_DeleteAssignedRole_Returns409()
    {
        await ResetEmployeeDynamicRoleAsync();
        await AuthenticateAsync("admin@crystal.local");

        CreateDynamicRoleRequest createRequest = new()
        {
            Name = $"Rôle assigné {Guid.NewGuid():N}",
            Permissions =
            [
                new PermissionRuleRequest { Action = "read", Subject = "item" },
            ],
        };

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/roles", createRequest);
        createResponse.EnsureSuccessStatusCode();
        DynamicRoleResponseDto? created = await createResponse.Content.ReadFromJsonAsync<DynamicRoleResponseDto>();
        Assert.NotNull(created);

        using IServiceScope scope = m_factory.Services.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        ApplicationUser? employee = await userManager.FindByEmailAsync("employee@crystal.local");
        Assert.NotNull(employee);

        UpdateUserRequest updateRequest = new()
        {
            Email = employee.Email!,
            UserName = employee.UserName!,
            DynamicRoleId = created.Id,
        };

        HttpResponseMessage updateResponse = await m_client.PutAsJsonAsync($"/api/users/{employee.Id}", updateRequest);
        updateResponse.EnsureSuccessStatusCode();

        HttpResponseMessage deleteResponse = await m_client.DeleteAsync($"/api/roles/{created.Id}");

        Assert.Equal(HttpStatusCode.Conflict, deleteResponse.StatusCode);
    }

    [Fact]
    public async Task EmployeeProfiles_Create_Returns403_ForEmployee()
    {
        await AuthenticateAsync("employee@crystal.local");

        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        CreateEmployeeProfileRequest request = BuildCreateRequest(referenceData);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/employee-profiles", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task EmployeeProfiles_Create_Returns201_ForGerant()
    {
        await AuthenticateAsync("gerant@crystal.local");

        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        CreateEmployeeProfileRequest request = BuildCreateRequest(referenceData);

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/employee-profiles", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task PermissionEntities_GetAll_Returns200_ForAdmin()
    {
        await AuthenticateAsync("admin@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/permission-entities");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<PermissionEntityResponseDto>? entities =
            await response.Content.ReadFromJsonAsync<List<PermissionEntityResponseDto>>();
        Assert.NotNull(entities);
        Assert.Contains(entities, p_entity => p_entity.Id == "employee_profile");
        Assert.Contains(entities, p_entity => p_entity.Id == "user_role");
    }

    private async Task ResetEmployeeDynamicRoleAsync()
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        ApplicationUser? employee = await userManager.FindByEmailAsync("employee@crystal.local");
        if (employee is not null)
        {
            employee.DynamicRoleId = ApplicationRoles.Employee;
            await userManager.UpdateAsync(employee);
        }
    }

    private async Task AuthenticateAsync(string p_email)
    {
        LoginRequest request = new()
        {
            Email = p_email,
            Password = "ValidPass1!a",
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/auth/login", request);
        response.EnsureSuccessStatusCode();

        LoginResponse? login = await response.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.NotNull(login);

        m_client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", login.Token);
    }

    private async Task<ReferenceDataSeedResult> SeedReferenceDataAsync()
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.JobPosition jobPosition = new()
        {
            Name = $"Poste-{Guid.NewGuid():N}",
            Description = "Test"
        };
        await context.JobPositions.AddAsync(jobPosition);
        await context.SaveChangesAsync();

        return new ReferenceDataSeedResult(jobPosition.Id);
    }

    private static CreateEmployeeProfileRequest BuildCreateRequest(ReferenceDataSeedResult p_referenceData)
    {
        return new CreateEmployeeProfileRequest
        {
            FirstName = "Test",
            LastName = "Permission",
            Email = $"perm-{Guid.NewGuid():N}@test.local",
            HiringDate = new DateOnly(2024, 6, 1),
            JobPositionId = p_referenceData.JobPositionId,
            Salary = 45000m,
            Status = "Active",
        };
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

    private sealed record ReferenceDataSeedResult(int JobPositionId);
}
