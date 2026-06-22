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

namespace Crystal.IntegrationTests.Alignment;

/// <summary>
/// Vérifie les critères d'acceptation de la Phase 1 — alignement BE/FE.
/// </summary>
public sealed class Phase1AlignmentIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public Phase1AlignmentIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task EmployeeProfileMe_ReturnsLinkedProfile_WithLocation_WhenProfileExists()
    {
        await AuthenticateAsync("admin@crystal.local");
        int profileId = await SeedEmployeeProfileForUserAsync("employee@crystal.local");

        await AuthenticateAsync("employee@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/employee-profiles/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        EmployeeProfileResponseDto? profile = await response.Content.ReadFromJsonAsync<EmployeeProfileResponseDto>();
        Assert.NotNull(profile);
        Assert.Equal(profileId, profile.Id);
        Assert.Equal("employee@crystal.local", profile.Email);
        Assert.NotNull(profile.LocationId);
        Assert.True(profile.LocationId > 0);
        Assert.False(string.IsNullOrWhiteSpace(profile.LocationTitle));
    }

    [Fact]
    public async Task Schedules_GetAll_Returns200_ForGerant()
    {
        await AuthenticateAsync("gerant@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/schedules");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task JobPositions_GetAll_Returns200_ForAssistant()
    {
        await AuthenticateAsync("assistant@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/job-positions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Authors_GetAll_Returns200_ForEmployee()
    {
        await AuthenticateAsync("employee@crystal.local");

        HttpResponseMessage response = await m_client.GetAsync("/api/authors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    private async Task<int> SeedEmployeeProfileForUserAsync(string p_email)
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
            return existing.Id;
        }

        Crystal.Core.Entities.Location location = await context.Locations.FirstAsync();
        Crystal.Core.Entities.JobPosition jobPosition = new() { Name = $"Poste-{Guid.NewGuid()}", Description = "Test" };
        await context.JobPositions.AddAsync(jobPosition);
        await context.SaveChangesAsync();

        Crystal.Core.Entities.EmployeeProfile profile = new()
        {
            FirstName = "Émilie",
            LastName = "Test",
            Email = p_email,
            ApplicationUserId = user.Id,
            Salary = 43000m,
            Status = "Active",
            PositionId = jobPosition.Id,
            HiringDate = new DateOnly(2024, 1, 15),
            LocationId = location.Id,
            IsDeleted = false,
        };

        await context.EmployeeProfiles.AddAsync(profile);
        await context.SaveChangesAsync();

        return profile.Id;
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
}
