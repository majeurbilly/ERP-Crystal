using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Crystal.IntegrationTests.Authors;

public sealed class AuthorsIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;

    public AuthorsIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_Returns200_WhenAuthenticatedAsEmployee()
    {
        await AuthenticateAsEmployeeAsync();

        HttpResponseMessage response = await m_client.GetAsync("/api/authors");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Create_Returns201_WithAdminToken()
    {
        await AuthenticateAsAdminAsync();

        CreateAuthorRequest request = new() { Name = $"Auteur-{Guid.NewGuid()}" };
        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/authors", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        AuthorResponseDto? body = await response.Content.ReadFromJsonAsync<AuthorResponseDto>();
        Assert.NotNull(body);
        Assert.True(body.Id > 0);
        Assert.Equal(request.Name, body.Name);
    }

    [Fact]
    public async Task Create_Returns403_WithEmployeeToken()
    {
        await AuthenticateAsEmployeeAsync();

        CreateAuthorRequest request = new() { Name = $"Auteur-{Guid.NewGuid()}" };
        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/authors", request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
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
