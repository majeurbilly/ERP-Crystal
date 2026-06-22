using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace Crystal.IntegrationTests.Category;

public sealed class CategoryIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public CategoryIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns200_WhenAuthenticated()
    {
        await AuthenticateAsync();
        HttpResponseMessage response = await m_client.GetAsync("/api/categories");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetAll_Returns401_WhenNotAuthenticated()
    {
        HttpResponseMessage response = await m_client.GetAsync("/api/categories");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201_WithValidRequest()
    {
        await AuthenticateAsAdminAsync();
        CreateCategoryRequestDto request = new() { Name = "Science-Fiction" };

        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/categories", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        CategoryResponseDto? body = await response.Content.ReadFromJsonAsync<CategoryResponseDto>();
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal("Science-Fiction", body.Name);
    }

    [Fact]
    public async Task Create_Returns409_WhenNameAlreadyExists()
    {
        await AuthenticateAsAdminAsync();
        CreateCategoryRequestDto request = new() { Name = "Romans" };

        HttpResponseMessage firstResponse = await m_client.PostAsJsonAsync("/api/categories", request);
        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);

        HttpResponseMessage duplicateResponse = await m_client.PostAsJsonAsync("/api/categories", request);
        Assert.Equal(HttpStatusCode.Conflict, duplicateResponse.StatusCode);

        JsonDocument document = JsonDocument.Parse(await duplicateResponse.Content.ReadAsStringAsync());
        string message = document.RootElement.GetProperty("message").GetString() ?? string.Empty;
        Assert.Equal("A category with this name already exists.", message);
    }

    [Fact]
    public async Task Update_Returns409_WhenNameAlreadyUsedByAnotherCategory()
    {
        await AuthenticateAsAdminAsync();

        CreateCategoryRequestDto first = new() { Name = "Policier" };
        CreateCategoryRequestDto second = new() { Name = "Biographie" };

        HttpResponseMessage firstResponse = await m_client.PostAsJsonAsync("/api/categories", first);
        HttpResponseMessage secondResponse = await m_client.PostAsJsonAsync("/api/categories", second);
        firstResponse.EnsureSuccessStatusCode();
        secondResponse.EnsureSuccessStatusCode();

        CategoryResponseDto? secondCategory = await secondResponse.Content.ReadFromJsonAsync<CategoryResponseDto>();
        Assert.NotNull(secondCategory);

        UpdateCategoryRequestDto updateRequest = new() { Name = "Policier" };
        HttpResponseMessage updateResponse = await m_client.PutAsJsonAsync(
            $"/api/categories/{secondCategory.Id}",
            updateRequest);

        Assert.Equal(HttpStatusCode.Conflict, updateResponse.StatusCode);
    }

    [Fact]
    public async Task GetById_Returns404_WhenCategoryDoesNotExist()
    {
        await AuthenticateAsync();
        HttpResponseMessage response = await m_client.GetAsync("/api/categories/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_SoftDeletesCategory_AndHidesFromQueries()
    {
        await AuthenticateAsAdminAsync();

        CreateCategoryRequestDto request = new() { Name = "À supprimer" };
        HttpResponseMessage createResponse = await m_client.PostAsJsonAsync("/api/categories", request);
        createResponse.EnsureSuccessStatusCode();

        CategoryResponseDto? created = await createResponse.Content.ReadFromJsonAsync<CategoryResponseDto>();
        Assert.NotNull(created);

        HttpResponseMessage deleteResponse = await m_client.DeleteAsync($"/api/categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        HttpResponseMessage getResponse = await m_client.GetAsync($"/api/categories/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        Crystal.Core.Entities.Category? deletedCategory = await context.Categories
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(p_category => p_category.Id == created.Id);

        Assert.NotNull(deletedCategory);
        Assert.True(deletedCategory.IsDeleted);
    }

    [Fact]
    public async Task Delete_Returns404_WhenCategoryDoesNotExist()
    {
        await AuthenticateAsAdminAsync();
        HttpResponseMessage response = await m_client.DeleteAsync("/api/categories/99999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task AuthenticateAsync()
    {
        LoginRequest request = new()
        {
            Email = "gerant@crystal.local",
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

    private async Task AuthenticateAsAdminAsync()
    {
        LoginRequest request = new()
        {
            Email = "admin@crystal.local",
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
