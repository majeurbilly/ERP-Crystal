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

namespace Crystal.IntegrationTests.JobPosition;

public sealed class JobPositionIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public JobPositionIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns200OK_WithEmployeeToken()
    {
        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/job-positions");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetById_Returns200OK_WhenEntityExists()
    {
        await AuthenticateAsAdminAsync();

        string uniqueName = $"JobPosition-{Guid.NewGuid()}";
        CreateJobPositionRequest createRequest = new()
        {
            Name = uniqueName,
            Description = "Description de test"
        };

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/job-positions", createRequest);
        createResponse.EnsureSuccessStatusCode();

        JobPositionResponseDto? created = await createResponse.Content.ReadFromJsonAsync<JobPositionResponseDto>();
        Assert.NotNull(created);

        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage getResponse = await m_client.GetAsync($"/api/job-positions/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        JobPositionResponseDto? retrieved = await getResponse.Content.ReadFromJsonAsync<JobPositionResponseDto>();
        Assert.NotNull(retrieved);
        Assert.Equal(created.Id, retrieved.Id);
        Assert.Equal(uniqueName, retrieved.Name);
        Assert.Equal("Description de test", retrieved.Description);
    }

    [Fact]
    public async Task Create_Returns201Created_WithAdminToken()
    {
        await AuthenticateAsAdminAsync();

        string uniqueName = $"JobPosition-{Guid.NewGuid()}";
        CreateJobPositionRequest request = new()
        {
            Name = uniqueName,
            Description = "Nouveau poste"
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/job-positions", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        JobPositionResponseDto? body = await response.Content.ReadFromJsonAsync<JobPositionResponseDto>();
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal(uniqueName, body.Name);
        Assert.Equal("Nouveau poste", body.Description);
    }

    [Fact]
    public async Task Create_Returns403Forbidden_WithEmployeeToken()
    {
        await AuthenticateAsEmployeeAsync();

        string uniqueName = $"JobPosition-{Guid.NewGuid()}";
        CreateJobPositionRequest request = new()
        {
            Name = uniqueName,
            Description = "Tentative employé"
        };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/job-positions", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns409Conflict_WhenNameAlreadyExists()
    {
        await AuthenticateAsAdminAsync();

        string uniqueName = $"JobPosition-{Guid.NewGuid()}";
        CreateJobPositionRequest request = new()
        {
            Name = uniqueName,
            Description = "Premier poste"
        };

        HttpResponseMessage firstResponse = await m_client.PostAsJsonAsync("/api/job-positions", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        HttpResponseMessage duplicateResponse = await m_client.PostAsJsonAsync("/api/job-positions", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        JsonDocument document = JsonDocument.Parse(await duplicateResponse.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal(ErrorMessages.JobPosition.NameAlreadyExists, message);
    }

    [Fact]
    public async Task Update_Returns200OK_WithAdminToken()
    {
        await AuthenticateAsAdminAsync();

        string uniqueName = $"JobPosition-{Guid.NewGuid()}";
        CreateJobPositionRequest createRequest = new()
        {
            Name = uniqueName,
            Description = "Description initiale"
        };

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/job-positions", createRequest);
        createResponse.EnsureSuccessStatusCode();

        JobPositionResponseDto? created = await createResponse.Content.ReadFromJsonAsync<JobPositionResponseDto>();
        Assert.NotNull(created);

        string updatedName = $"{uniqueName}-Updated";
        UpdateJobPositionRequest updateRequest = new()
        {
            Name = updatedName,
            Description = "Updated description"
        };

        HttpResponseMessage updateResponse = await m_client.PutAsJsonAsync(
            $"/api/job-positions/{created.Id}",
            updateRequest);

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        JobPositionResponseDto? updated = await updateResponse.Content.ReadFromJsonAsync<JobPositionResponseDto>();
        Assert.NotNull(updated);
        Assert.Equal(created.Id, updated.Id);
        Assert.Equal(updatedName, updated.Name);
        Assert.Equal("Updated description", updated.Description);
    }

    [Fact]
    public async Task Delete_Returns204NoContent_AndPerformsSoftDelete()
    {
        await AuthenticateAsAdminAsync();

        string uniqueName = $"JobPosition-{Guid.NewGuid()}";
        CreateJobPositionRequest createRequest = new()
        {
            Name = uniqueName,
            Description = "À supprimer"
        };

        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/job-positions", createRequest);
        createResponse.EnsureSuccessStatusCode();

        JobPositionResponseDto? created = await createResponse.Content.ReadFromJsonAsync<JobPositionResponseDto>();
        Assert.NotNull(created);

        HttpResponseMessage deleteResponse = await m_client.DeleteAsync($"/api/job-positions/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        HttpResponseMessage getResponse = await m_client.GetAsync($"/api/job-positions/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.JobPosition? deletedJobPosition = await context.JobPositions
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(p_position => p_position.Id == created.Id);

        Assert.NotNull(deletedJobPosition);
        Assert.True(deletedJobPosition.IsDeleted);
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
}
