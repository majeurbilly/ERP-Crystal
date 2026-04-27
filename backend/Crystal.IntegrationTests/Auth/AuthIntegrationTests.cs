using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Crystal.Core;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Microsoft.IdentityModel.Tokens;

namespace Crystal.IntegrationTests.Auth;

public sealed class AuthIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{

    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public AuthIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task Register_Then_Login_Returns_A_Cryptographically_Valid_JWT()
    {
        // Préparation
        const string userName = "integration_user";
        const string email = "integration@test.local";
        const string password = "ValidPass1!a";

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        HttpResponseMessage registerResponse = await m_client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest
            {
                UserName = userName,
                Email = email,
                Password = password,
                Role = ApplicationRoles.Employee
            });

        m_client.DefaultRequestHeaders.Remove("Authorization");

        Assert.Equal(HttpStatusCode.OK, registerResponse.StatusCode);

        // Exécution
        HttpResponseMessage loginResponse = await m_client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest
            {
                Username = userName,
                Password = password
            });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        // Vérification
        LoginResponse? body = await loginResponse.Content.ReadFromJsonAsync<LoginResponse>();

        Assert.NotNull(body);

        Assert.False(string.IsNullOrWhiteSpace(body.Token));

        TokenValidationParameters parameters = new()
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(CrystalWebApplicationFactory.JwtKey)),
            ValidateIssuer = true,
            ValidIssuer = CrystalWebApplicationFactory.JwtIssuer,
            ValidateAudience = true,
            ValidAudience = CrystalWebApplicationFactory.JwtAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        JwtSecurityTokenHandler handler = new();

        ClaimsPrincipal principal = handler.ValidateToken(body.Token, parameters, out SecurityToken validatedToken);

        JwtSecurityToken jwt = Assert.IsType<JwtSecurityToken>(validatedToken);

        Assert.Equal(CrystalWebApplicationFactory.JwtIssuer, jwt.Issuer);

        Assert.Contains(jwt.Audiences, a => a == CrystalWebApplicationFactory.JwtAudience);

        string? sub = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        Assert.Equal(body.UserId, sub);

        Assert.Contains(principal.Claims, c => c.Type == ClaimTypes.Role && c.Value == ApplicationRoles.Employee);
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_Returns401()
    {
        HttpResponseMessage loginResponse = await m_client.PostAsJsonAsync(
            "/api/auth/login",
            new LoginRequest
            {
                Username = $"integration_user_{Guid.NewGuid():N}",
                Password = "WrongPassword1!a"
            });

        Assert.Equal(HttpStatusCode.Unauthorized, loginResponse.StatusCode);
    }

    [Fact]
    public async Task Register_WithExistingEmail_Returns400()
    {
        string stamp = Guid.NewGuid().ToString("N");
        string email = $"integration_{stamp}@test.local";

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        HttpResponseMessage firstRegisterResponse = await m_client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest
            {
                UserName = $"integration_user_{stamp}_1",
                Email = email,
                Password = "ValidPass1!a",
                Role = ApplicationRoles.Employee
            });

        Assert.Equal(HttpStatusCode.OK, firstRegisterResponse.StatusCode);

        HttpResponseMessage secondRegisterResponse = await m_client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest
            {
                UserName = $"integration_user_{stamp}_2",
                Email = email,
                Password = "ValidPass1!a",
                Role = ApplicationRoles.Employee
            });

        Assert.Equal(HttpStatusCode.BadRequest, secondRegisterResponse.StatusCode);

        m_client.DefaultRequestHeaders.Remove("Authorization");
    }

    [Fact]
    public async Task Register_WithWeakPassword_Returns400()
    {
        string stamp = Guid.NewGuid().ToString("N");

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        HttpResponseMessage registerResponse = await m_client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest
            {
                UserName = $"integration_user_{stamp}",
                Email = $"integration_{stamp}@test.local",
                Password = "123",
                Role = ApplicationRoles.Employee
            });

        Assert.Equal(HttpStatusCode.BadRequest, registerResponse.StatusCode);

        m_client.DefaultRequestHeaders.Remove("Authorization");
    }

    public void Dispose()
    {
        m_client.Dispose();
    }
}
