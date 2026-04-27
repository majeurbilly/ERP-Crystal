using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Crystal.Core;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.Entities;
using Crystal.Core.DTOs.Responses;
using Crystal.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Crystal.IntegrationTests.Users;

public sealed class UserControllerIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private readonly HttpClient m_client;
    private readonly CrystalWebApplicationFactory m_factory;

    public UserControllerIntegrationTests(CrystalWebApplicationFactory p_factory)
    {
        m_factory = p_factory;
        m_client = p_factory.CreateClient();
    }

    [Fact]
    public async Task GetUsers_WithoutAuthorizationHeader_Returns401()
    {
        // Arrange
        m_client.DefaultRequestHeaders.Remove("Authorization");

        // Act
        HttpResponseMessage response = await m_client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithEmployeeRole_Returns403()
    {
        // Arrange
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        // Act
        HttpResponseMessage response = await m_client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithAdminRole_Returns200AndContent()
    {
        // Arrange
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        // Act
        HttpResponseMessage response = await m_client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<UserResponse>? body = await response.Content.ReadFromJsonAsync<List<UserResponse>>();

        Assert.NotNull(body);
    }

    [Fact]
    public async Task GetHrMetrics_WithAdminRole_Returns200Ok()
    {
        // Arrange
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        // Act
        HttpResponseMessage response = await m_client.GetAsync("/api/users/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        HrMetricsResponse? body = await response.Content.ReadFromJsonAsync<HrMetricsResponse>();
        Assert.NotNull(body);
    }

    [Fact]
    public async Task GetHrMetrics_WithEmployeeRole_Returns403Forbidden()
    {
        // Arrange
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        // Act
        HttpResponseMessage response = await m_client.GetAsync("/api/users/metrics");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_WithAdminRole_AndValidId_Returns200AndUser()
    {
        // Arrange
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        HttpResponseMessage usersResponse = await m_client.GetAsync("/api/users");
        Assert.Equal(HttpStatusCode.OK, usersResponse.StatusCode);

        List<UserResponse>? users = await usersResponse.Content.ReadFromJsonAsync<List<UserResponse>>();
        Assert.NotNull(users);
        Assert.NotEmpty(users);

        string userId = users[0].Id;

        // Act
        HttpResponseMessage response = await m_client.GetAsync($"/api/users/{userId}");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserResponse? body = await response.Content.ReadFromJsonAsync<UserResponse>();

        Assert.NotNull(body);
        Assert.Equal(userId, body.Id);
    }

    [Fact]
    public async Task GetUserById_WithAdminRole_AndInvalidId_Returns404()
    {
        // Arrange
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));
        const string invalidId = "un-id-qui-n-existe-pas-123";

        // Act
        HttpResponseMessage response = await m_client.GetAsync($"/api/users/{invalidId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserById_WithEmployeeRole_Returns403()
    {
        // Arrange
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        // Act
        HttpResponseMessage response = await m_client.GetAsync("/api/users/nimporte-quel-id");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetMyProfile_WithoutAuthorizationHeader_Returns401()
    {
        // Préparation
        m_client.DefaultRequestHeaders.Remove("Authorization");

        // Action
        HttpResponseMessage response = await m_client.GetAsync("/api/users/me");

        // Vérification
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetMyProfile_WithEmployeeRole_Returns200AndUser()
    {
        // Préparation
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext dbContext = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        ApplicationUser? user = await dbContext.Users
            .Where(p_user => p_user.IsActive)
            .OrderBy(p_user => p_user.Id)
            .FirstOrDefaultAsync();
        Assert.NotNull(user);

        string userId = user.Id;

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateJwtForUserIdAndRoles(userId, ApplicationRoles.Employee));

        // Action
        HttpResponseMessage response = await m_client.GetAsync("/api/users/me");

        // Vérification
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserResponse? body = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(body);
        Assert.Equal(userId, body.Id);
    }

    [Fact]
    public async Task UpdateMyProfile_WithoutAuth_Returns401Unauthorized()
    {
        // Préparation
        m_client.DefaultRequestHeaders.Remove("Authorization");

        UpdateProfileRequest request = new UpdateProfileRequest
        {
            Email = $"integration-update-me-noauth-{Guid.NewGuid():N}@example.com",
            UserName = $"noauth-{Guid.NewGuid():N}"
        };

        // Action
        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/users/me", request);

        // Vérification
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMyProfile_WithValidData_Returns200Ok()
    {
        // Préparation : récupération d'un utilisateur existant en base de test.
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext dbContext = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        ApplicationUser? user = await dbContext.Users
            .Where(p_user => p_user.IsActive)
            .OrderBy(p_user => p_user.Id)
            .FirstOrDefaultAsync();
        Assert.NotNull(user);

        string userId = user.Id;

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateJwtForUserIdAndRoles(userId, ApplicationRoles.Employee));

        UpdateProfileRequest request = new UpdateProfileRequest
        {
            Email = $"integration-update-me-{Guid.NewGuid():N}@example.com",
            UserName = $"updated-me-{Guid.NewGuid():N}"
        };

        // Action
        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/users/me", request);

        // Vérification
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserResponse? body = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(body);
        Assert.Equal(request.UserName, body.UserName);
        Assert.Equal(request.Email, body.Email);
    }

    [Fact]
    public async Task UpdateMyProfile_WithInvalidEmail_Returns400BadRequest()
    {
        // Préparation : récupération d'un utilisateur existant pour générer un JWT valide.
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext dbContext = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        ApplicationUser? user = await dbContext.Users
            .Where(p_user => p_user.IsActive)
            .OrderBy(p_user => p_user.Id)
            .FirstOrDefaultAsync();
        Assert.NotNull(user);

        string userId = user.Id;

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateJwtForUserIdAndRoles(userId, ApplicationRoles.Employee));

        UpdateProfileRequest request = new UpdateProfileRequest
        {
            Email = "invalid-email-format",
            UserName = $"invalid-email-{Guid.NewGuid():N}"
        };

        // Action
        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/users/me", request);

        // Vérification
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMyProfile_WithDuplicateEmail_Returns400BadRequest()
    {
        // Préparation : récupération de deux utilisateurs distincts en base de test.
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext dbContext = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        List<ApplicationUser> users = await dbContext.Users
            .Where(p_user => p_user.IsActive)
            .OrderBy(p_user => p_user.Id)
            .Take(2)
            .ToListAsync();

        Assert.True(users.Count >= 2);

        ApplicationUser user1 = users[0];
        ApplicationUser user2 = users[1];

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateJwtForUserIdAndRoles(user1.Id, ApplicationRoles.Employee));

        UpdateProfileRequest request = new UpdateProfileRequest
        {
            Email = user2.Email ?? "duplicate-email@example.com",
            UserName = $"duplicate-email-{Guid.NewGuid():N}"
        };

        // Action
        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/users/me", request);

        // Vérification
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateMyProfile_WithDuplicateUserName_Returns400BadRequest()
    {
        // Préparation : récupération de deux utilisateurs distincts en base de test.
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext dbContext = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        List<ApplicationUser> users = await dbContext.Users
            .Where(p_user => p_user.IsActive)
            .OrderBy(p_user => p_user.Id)
            .Take(2)
            .ToListAsync();

        Assert.True(users.Count >= 2);

        ApplicationUser user1 = users[0];
        ApplicationUser user2 = users[1];

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CreateJwtForUserIdAndRoles(user1.Id, ApplicationRoles.Employee));

        UpdateProfileRequest request = new UpdateProfileRequest
        {
            Email = user1.Email ?? $"user1-valid-{Guid.NewGuid():N}@example.com",
            UserName = user2.UserName ?? $"duplicate-username-{Guid.NewGuid():N}"
        };

        // Action
        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/users/me", request);

        // Vérification
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithAdminRole_AndValidData_Returns201Created()
    {
        // Arrange
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        string uniqueEmail = $"integration-create-{Guid.NewGuid():N}@example.com";
        CreateUserRequest request = new CreateUserRequest
        {
            Email = uniqueEmail,
            UserName = $"user-{Guid.NewGuid():N}",
            Password = "Password123!",
            Role = ApplicationRoles.Employee
        };

        // Act
        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/users", request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        UserResponse? body = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(body);
        Assert.Equal(request.UserName, body.UserName);
        Assert.NotNull(response.Headers.Location);
    }

    [Fact]
    public async Task CreateUser_WithAdminRole_AndInvalidPassword_Returns400BadRequest()
    {
        // Arrange
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        string uniqueEmail = $"integration-invalid-password-{Guid.NewGuid():N}@example.com";
        CreateUserRequest request = new CreateUserRequest
        {
            Email = uniqueEmail,
            UserName = $"user-{Guid.NewGuid():N}",
            Password = "123",
            Role = ApplicationRoles.Employee
        };

        // Act
        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/users", request);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_WithEmployeeRole_Returns403Forbidden()
    {
        // Arrange
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        string uniqueEmail = $"integration-forbidden-{Guid.NewGuid():N}@example.com";
        CreateUserRequest request = new CreateUserRequest
        {
            Email = uniqueEmail,
            UserName = $"user-{Guid.NewGuid():N}",
            Password = "Password123!",
            Role = ApplicationRoles.Employee
        };

        // Act
        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/users", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WithAdminRole_AndValidData_Returns200Ok()
    {
        // Préparation : on récupère un utilisateur existant depuis la base de test.
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext dbContext = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        ApplicationUser? user = await dbContext.Users
            .Where(p_user => p_user.IsActive)
            .OrderBy(p_user => p_user.Id)
            .FirstOrDefaultAsync();
        Assert.NotNull(user);

        string userId = user.Id;

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = $"integration-update-{Guid.NewGuid():N}@example.com",
            UserName = $"updated-{Guid.NewGuid():N}",
            Role = ApplicationRoles.Employee
        };

        // Action
        HttpResponseMessage response = await m_client.PutAsJsonAsync($"/api/users/{userId}", request);

        // Vérification
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        UserResponse? body = await response.Content.ReadFromJsonAsync<UserResponse>();
        Assert.NotNull(body);
        Assert.Equal(request.UserName, body.UserName);
    }

    [Fact]
    public async Task UpdateUser_WithAdminRole_AndInvalidId_Returns404NotFound()
    {
        // Préparation
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = $"integration-update-notfound-{Guid.NewGuid():N}@example.com",
            UserName = $"updated-{Guid.NewGuid():N}",
            Role = ApplicationRoles.Employee
        };
        const string invalidId = "faux-id-123";

        // Action
        HttpResponseMessage response = await m_client.PutAsJsonAsync($"/api/users/{invalidId}", request);

        // Vérification
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WithAdminRole_AndInvalidEmail_Returns400BadRequest()
    {
        // Préparation
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = "mauvais-email",
            UserName = $"updated-{Guid.NewGuid():N}",
            Role = ApplicationRoles.Employee
        };

        // Action : l'ID importe peu ici car la validation du body échoue.
        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/users/any-id", request);

        // Vérification
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WithEmployeeRole_Returns403Forbidden()
    {
        // Préparation
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = $"integration-update-forbidden-{Guid.NewGuid():N}@example.com",
            UserName = $"updated-{Guid.NewGuid():N}",
            Role = ApplicationRoles.Employee
        };

        // Action
        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/users/nimporte-quel-id", request);

        // Vérification
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WithAdminRole_AndValidId_Returns204NoContent()
    {
        // Préparation : récupération d'un utilisateur actif en base de test.
        using (IServiceScope scope = m_factory.Services.CreateScope())
        {
            CrystalDbContext dbContext = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

            ApplicationUser? user = await dbContext.Users
                .Where(p_user => p_user.IsActive)
                .OrderBy(p_user => p_user.Id)
                .FirstOrDefaultAsync();
            Assert.NotNull(user);

            string userId = user.Id;

            m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                "Bearer",
                CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

            // Action
            HttpResponseMessage response = await m_client.DeleteAsync($"/api/users/{userId}");

            // Vérification HTTP
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

            // Vérification base de données : l'utilisateur est soft deleted.
            using IServiceScope verificationScope = m_factory.Services.CreateScope();
            CrystalDbContext verificationDbContext = verificationScope.ServiceProvider.GetRequiredService<CrystalDbContext>();

            ApplicationUser? deletedUser = await verificationDbContext.Users
                .FirstOrDefaultAsync(p_user => p_user.Id == userId);

            Assert.NotNull(deletedUser);
            Assert.False(deletedUser.IsActive);
        }
    }

    [Fact]
    public async Task DeleteUser_WithAdminRole_AndInvalidId_Returns404NotFound()
    {
        // Préparation
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        const string invalidId = "invalid-user-id-for-delete-123";

        // Action
        HttpResponseMessage response = await m_client.DeleteAsync($"/api/users/{invalidId}");

        // Vérification
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WithEmployeeRole_Returns403Forbidden()
    {
        // Préparation
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Employee));

        const string userId = "any-user-id";

        // Action
        HttpResponseMessage response = await m_client.DeleteAsync($"/api/users/{userId}");

        // Vérification
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithAdminRole_DoesNotReturnInactiveUsers()
    {
        // Préparation : on soft delete un utilisateur actif.
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext dbContext = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        ApplicationUser? user = await dbContext.Users
            .Where(p_user => p_user.IsActive)
            .OrderBy(p_user => p_user.Id)
            .FirstOrDefaultAsync();
        Assert.NotNull(user);

        user.IsActive = false;
        await dbContext.SaveChangesAsync();

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForRoles(ApplicationRoles.Admin));

        // Action
        HttpResponseMessage response = await m_client.GetAsync("/api/users");

        // Vérification
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<UserResponse>? users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();
        Assert.NotNull(users);
        Assert.DoesNotContain(users, p_user => p_user.Id == user.Id);
    }

    public void Dispose()
    {
        m_client.Dispose();
    }

    private static string CreateJwtForUserIdAndRoles(string p_userId, params string[] p_roles)
    {
        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, p_userId),
            new Claim(ClaimTypes.NameIdentifier, p_userId),
        ];

        foreach (string role in p_roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(CrystalWebApplicationFactory.JwtKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: CrystalWebApplicationFactory.JwtIssuer,
            audience: CrystalWebApplicationFactory.JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
