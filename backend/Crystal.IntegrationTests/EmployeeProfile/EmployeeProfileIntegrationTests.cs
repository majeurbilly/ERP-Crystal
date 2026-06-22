using Crystal.Core.Constants;
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
using System.Text.Json;

namespace Crystal.IntegrationTests.EmployeeProfile;

public sealed class EmployeeProfileIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public EmployeeProfileIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns403Forbidden_WithEmployeeToken()
    {
        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/employee-profiles");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Returns404_WhenEmployeeAccessesAnotherProfile()
    {
        await AuthenticateAsAdminAsync();

        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        string uniqueEmail = $"employee-{Guid.NewGuid()}@test.local";

        CreateEmployeeProfileRequest createRequest = BuildCreateRequest(
            referenceData,
            uniqueEmail,
            "Alice",
            "Martin");

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/employee-profiles", createRequest);
        createResponse.EnsureSuccessStatusCode();

        EmployeeProfileResponseDto? created = await createResponse.Content.ReadFromJsonAsync<EmployeeProfileResponseDto>();
        Assert.NotNull(created);

        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage getResponse = await m_client.GetAsync($"/api/employee-profiles/{created.Id}");

        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetById_Returns200OK_WhenEmployeeAccessesOwnProfile()
    {
        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        int profileId = await EnsureEmployeeProfileForUserAsync("employee@crystal.local", referenceData);

        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage getResponse = await m_client.GetAsync($"/api/employee-profiles/{profileId}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201Created_WithAdminToken()
    {
        await AuthenticateAsAdminAsync();

        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        string uniqueEmail = $"employee-{Guid.NewGuid()}@test.local";

        CreateEmployeeProfileRequest request = BuildCreateRequest(
            referenceData,
            uniqueEmail,
            "Bob",
            "Tremblay");

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/employee-profiles", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        EmployeeProfileResponseDto? body = await response.Content.ReadFromJsonAsync<EmployeeProfileResponseDto>();
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal("Bob", body.FirstName);
        Assert.Equal("Tremblay", body.LastName);
        Assert.Equal(uniqueEmail, body.Email);
        Assert.Equal(referenceData.JobPositionId, body.JobPositionId);
        Assert.Equal(referenceData.JobPositionName, body.JobPositionName);
    }

    [Fact]
    public async Task Create_Returns403Forbidden_WithEmployeeToken()
    {
        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        await AuthenticateAsEmployeeAsync();

        string uniqueEmail = $"employee-{Guid.NewGuid()}@test.local";
        CreateEmployeeProfileRequest request = BuildCreateRequest(
            referenceData,
            uniqueEmail,
            "Charlie",
            "Gagnon");

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/employee-profiles", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenEmailAlreadyExists()
    {
        await AuthenticateAsAdminAsync();

        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        string uniqueEmail = $"employee-{Guid.NewGuid()}@test.local";

        CreateEmployeeProfileRequest request = BuildCreateRequest(
            referenceData,
            uniqueEmail,
            "Diane",
            "Roy");

        HttpResponseMessage firstResponse = await m_client.PostAsJsonAsync("/api/employee-profiles", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        CreateEmployeeProfileRequest duplicateRequest = BuildCreateRequest(
            referenceData,
            uniqueEmail,
            "Eve",
            "Lavoie");

        HttpResponseMessage duplicateResponse = await m_client.PostAsJsonAsync("/api/employee-profiles", duplicateRequest);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        JsonDocument document = JsonDocument.Parse(await duplicateResponse.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal(ErrorMessages.EmployeeProfile.EmailAlreadyExists, message);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenPositionDoesNotExist()
    {
        await AuthenticateAsAdminAsync();

        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        string uniqueEmail = $"employee-{Guid.NewGuid()}@test.local";

        CreateEmployeeProfileRequest invalidPositionRequest = BuildCreateRequest(
            referenceData,
            uniqueEmail,
            "Frank",
            "Bouchard");
        invalidPositionRequest.JobPositionId = 999_999;

        HttpResponseMessage invalidPositionResponse = await m_client.PostAsJsonAsync(
            "/api/employee-profiles",
            invalidPositionRequest);

        Assert.Equal(HttpStatusCode.Conflict, invalidPositionResponse.StatusCode);

        JsonDocument invalidPositionDocument = JsonDocument.Parse(
            await invalidPositionResponse.Content.ReadAsStringAsync());
        string invalidPositionMessage = invalidPositionDocument.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal(ErrorMessages.EmployeeProfile.JobPositionNotFound, invalidPositionMessage);
    }

    [Fact]
    public async Task Update_Returns200OK_WithAdminToken()
    {
        await AuthenticateAsAdminAsync();

        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        string uniqueEmail = $"employee-{Guid.NewGuid()}@test.local";

        CreateEmployeeProfileRequest createRequest = BuildCreateRequest(
            referenceData,
            uniqueEmail,
            "Hugo",
            "Morin");

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/employee-profiles", createRequest);
        createResponse.EnsureSuccessStatusCode();

        EmployeeProfileResponseDto? created = await createResponse.Content.ReadFromJsonAsync<EmployeeProfileResponseDto>();
        Assert.NotNull(created);

        string updatedEmail = $"employee-{Guid.NewGuid()}@test.local";
        UpdateEmployeeProfileRequest updateRequest = new()
        {
            FirstName = "Hugo-Updated",
            LastName = "Morin-Updated",
            Email = updatedEmail,
            ApplicationUserId = null,
            Salary = 62000m,
            Status = "OnLeave",
            JobPositionId = referenceData.JobPositionId,
            HiringDate = new DateOnly(2023, 6, 1)
        };

        HttpResponseMessage updateResponse = await m_client.PutAsJsonAsync(
            $"/api/employee-profiles/{created.Id}",
            updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        EmployeeProfileResponseDto? updated = await updateResponse.Content.ReadFromJsonAsync<EmployeeProfileResponseDto>();
        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal("Hugo-Updated", updated.FirstName);
        Assert.Equal("Morin-Updated", updated.LastName);
        Assert.Equal(updatedEmail, updated.Email);
        Assert.Equal(62000m, updated.Salary);
        Assert.Equal("OnLeave", updated.Status);
        Assert.Equal(referenceData.JobPositionName, updated.JobPositionName);
    }

    [Fact]
    public async Task Delete_Returns204NoContent_AndPerformsSoftDelete()
    {
        await AuthenticateAsAdminAsync();

        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        string uniqueEmail = $"employee-{Guid.NewGuid()}@test.local";

        CreateEmployeeProfileRequest createRequest = BuildCreateRequest(
            referenceData,
            uniqueEmail,
            "Iris",
            "Fortin");

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/employee-profiles", createRequest);
        createResponse.EnsureSuccessStatusCode();

        EmployeeProfileResponseDto? created = await createResponse.Content.ReadFromJsonAsync<EmployeeProfileResponseDto>();
        Assert.NotNull(created);

        HttpResponseMessage deleteResponse = await m_client.DeleteAsync($"/api/employee-profiles/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        HttpResponseMessage getResponse = await m_client.GetAsync($"/api/employee-profiles/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.EmployeeProfile? deletedProfile = await context.EmployeeProfiles
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(p_profile => p_profile.Id == created.Id);

        Assert.NotNull(deletedProfile);
        Assert.True(deletedProfile.IsDeleted);
    }

    [Fact]
    public async Task GetMe_Returns200OK_WhenProfileLinkedToAuthenticatedUser()
    {
        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        int profileId = await EnsureEmployeeProfileForUserAsync("employee@crystal.local", referenceData);

        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage getMeResponse = await m_client.GetAsync("/api/employee-profiles/me");

        Assert.Equal(HttpStatusCode.OK, getMeResponse.StatusCode);

        EmployeeProfileResponseDto? myProfile = await getMeResponse.Content.ReadFromJsonAsync<EmployeeProfileResponseDto>();
        Assert.NotNull(myProfile);
        Assert.Equal(profileId, myProfile.Id);
        Assert.Equal("employee@crystal.local", myProfile.Email);
        Assert.NotNull(myProfile.ApplicationUserId);
    }

    [Fact]
    public async Task GetMe_Returns404_WhenNoProfileLinkedToAuthenticatedUser()
    {
        await AuthenticateAsAdminAsync();

        HttpResponseMessage getMeResponse = await m_client.GetAsync("/api/employee-profiles/me");

        Assert.Equal(HttpStatusCode.NotFound, getMeResponse.StatusCode);
    }

    [Fact]
    public async Task Create_Returns404_WhenApplicationUserIdDoesNotExist()
    {
        await AuthenticateAsAdminAsync();

        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        string uniqueEmail = $"employee-{Guid.NewGuid()}@test.local";

        CreateEmployeeProfileRequest request = BuildCreateRequest(
            referenceData,
            uniqueEmail,
            "Karim",
            "Nguyen");
        request.ApplicationUserId = Guid.NewGuid().ToString();

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/employee-profiles", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal(ErrorMessages.EmployeeProfile.UserAccountNotFound, message);
    }

    [Fact]
    public async Task Create_Returns409_WhenApplicationUserIdAlreadyAssigned()
    {
        await AuthenticateAsAdminAsync();

        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        string gerantApplicationUserId = await GetApplicationUserIdByEmailAsync("gerant@crystal.local");

        string firstEmail = $"employee-{Guid.NewGuid()}@test.local";
        CreateEmployeeProfileRequest firstRequest = BuildCreateRequest(
            referenceData,
            firstEmail,
            "Laura",
            "Girard");
        firstRequest.ApplicationUserId = gerantApplicationUserId;

        HttpResponseMessage firstResponse = await m_client.PostAsJsonAsync("/api/employee-profiles", firstRequest);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        string secondEmail = $"employee-{Guid.NewGuid()}@test.local";
        CreateEmployeeProfileRequest secondRequest = BuildCreateRequest(
            referenceData,
            secondEmail,
            "Marc",
            "Paquette");
        secondRequest.ApplicationUserId = gerantApplicationUserId;

        HttpResponseMessage secondResponse = await m_client.PostAsJsonAsync("/api/employee-profiles", secondRequest);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        JsonDocument document = JsonDocument.Parse(await secondResponse.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal(ErrorMessages.EmployeeProfile.UserAlreadyLinked, message);
    }

    [Fact]
    public async Task Update_Returns409_WhenApplicationUserIdAlreadyAssignedToAnotherProfile()
    {
        await AuthenticateAsAdminAsync();

        ReferenceDataSeedResult referenceData = await SeedReferenceDataAsync();
        string assistantApplicationUserId = await GetApplicationUserIdByEmailAsync("assistant@crystal.local");

        string linkedEmail = $"employee-{Guid.NewGuid()}@test.local";
        CreateEmployeeProfileRequest linkedRequest = BuildCreateRequest(
            referenceData,
            linkedEmail,
            "Nina",
            "Bergeron");
        linkedRequest.ApplicationUserId = assistantApplicationUserId;

        HttpResponseMessage linkedCreateResponse = await m_client.PostAsJsonAsync("/api/employee-profiles", linkedRequest);
        linkedCreateResponse.EnsureSuccessStatusCode();

        string unlinkedEmail = $"employee-{Guid.NewGuid()}@test.local";
        CreateEmployeeProfileRequest unlinkedRequest = BuildCreateRequest(
            referenceData,
            unlinkedEmail,
            "Olivier",
            "Caron");

        HttpResponseMessage unlinkedCreateResponse = await m_client.PostAsJsonAsync("/api/employee-profiles", unlinkedRequest);
        unlinkedCreateResponse.EnsureSuccessStatusCode();

        EmployeeProfileResponseDto? unlinkedProfile = await unlinkedCreateResponse.Content.ReadFromJsonAsync<EmployeeProfileResponseDto>();
        Assert.NotNull(unlinkedProfile);

        UpdateEmployeeProfileRequest updateRequest = new()
        {
            FirstName = unlinkedProfile.FirstName,
            LastName = unlinkedProfile.LastName,
            Email = unlinkedProfile.Email,
            ApplicationUserId = assistantApplicationUserId,
            Salary = unlinkedProfile.Salary,
            Status = unlinkedProfile.Status,
            JobPositionId = unlinkedProfile.JobPositionId,
            HiringDate = unlinkedProfile.HiringDate
        };

        HttpResponseMessage updateResponse = await m_client.PutAsJsonAsync(
            $"/api/employee-profiles/{unlinkedProfile.Id}",
            updateRequest);

        Assert.Equal(HttpStatusCode.Conflict, updateResponse.StatusCode);

        JsonDocument document = JsonDocument.Parse(await updateResponse.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal(ErrorMessages.EmployeeProfile.UserAlreadyLinked, message);
    }

    private async Task<int> EnsureEmployeeProfileForUserAsync(
        string p_email,
        ReferenceDataSeedResult p_referenceData)
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

        Crystal.Core.Entities.EmployeeProfile profile = new()
        {
            FirstName = "?milie",
            LastName = "Employ?e",
            Email = p_email,
            ApplicationUserId = user.Id,
            Salary = 43000m,
            Status = "Active",
            PositionId = p_referenceData.JobPositionId,
            HiringDate = new DateOnly(2024, 1, 15),
            IsDeleted = false,
        };

        await context.EmployeeProfiles.AddAsync(profile);
        await context.SaveChangesAsync();

        return profile.Id;
    }

    private async Task<string> GetApplicationUserIdByEmailAsync(string p_email)
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        ApplicationUser? applicationUser = await userManager.FindByEmailAsync(p_email);
        Assert.NotNull(applicationUser);

        return applicationUser.Id;
    }

    private async Task<ReferenceDataSeedResult> SeedReferenceDataAsync()
    {
        string uniqueSuffix = Guid.NewGuid().ToString();
        string jobPositionName = $"JobPosition-{uniqueSuffix}";

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.JobPosition jobPosition = new()
        {
            Name = jobPositionName,
            Description = "Poste de test pour profil employ?",
            IsDeleted = false
        };

        await context.JobPositions.AddAsync(jobPosition);
        await context.SaveChangesAsync();

        return new ReferenceDataSeedResult(
            jobPosition.Id,
            jobPositionName);
    }

    private static CreateEmployeeProfileRequest BuildCreateRequest(
        ReferenceDataSeedResult p_referenceData,
        string p_email,
        string p_firstName,
        string p_lastName)
    {
        return new CreateEmployeeProfileRequest
        {
            FirstName = p_firstName,
            LastName = p_lastName,
            Email = p_email,
            ApplicationUserId = null,
            Salary = 50000m,
            Status = "Active",
            JobPositionId = p_referenceData.JobPositionId,
            HiringDate = new DateOnly(2024, 3, 15)
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

    private sealed record ReferenceDataSeedResult(
        int JobPositionId,
        string JobPositionName);
}
