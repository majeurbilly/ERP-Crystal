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
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Crystal.IntegrationTests.Users;

public sealed class UserControllerIntegrationTests : IClassFixture<CrystalWebApplicationFactory>, IDisposable
{
    private const string SeedEmailDomain = "@crystal.local";

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
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Employee));

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
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Admin));

        // Act
        HttpResponseMessage response = await m_client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<UserResponse>? body = await response.Content.ReadFromJsonAsync<List<UserResponse>>();

        Assert.NotNull(body);
    }

    [Fact]
    public async Task GetUserById_WithAdminRole_AndValidId_Returns200AndUser()
    {
        // Arrange
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Admin));

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
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Admin));
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
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Employee));

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
        ApplicationUser user = await GetOrCreateDisposableUserAsync();
        string userId = user.Id;

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForUserIdAndRoles(userId, ApplicationRoles.Employee));

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
        // Préparation : utilisateur jetable pour ne pas altérer les comptes seed.
        ApplicationUser user = await GetOrCreateDisposableUserAsync();
        string userId = user.Id;

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForUserIdAndRoles(userId, ApplicationRoles.Employee));

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
        // Préparation : utilisateur jetable pour ne pas altérer les comptes seed.
        ApplicationUser user = await GetOrCreateDisposableUserAsync();
        string userId = user.Id;

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForUserIdAndRoles(userId, ApplicationRoles.Employee));

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
        // Préparation : deux utilisateurs jetables distincts.
        ApplicationUser user1 = await CreateDisposableUserAsync();
        ApplicationUser user2 = await CreateDisposableUserAsync();

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForUserIdAndRoles(user1.Id, ApplicationRoles.Employee));

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
        // Préparation : deux utilisateurs jetables distincts.
        ApplicationUser user1 = await CreateDisposableUserAsync();
        ApplicationUser user2 = await CreateDisposableUserAsync();

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            CrystalWebApplicationFactory.CreateJwtForUserIdAndRoles(user1.Id, ApplicationRoles.Employee));

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
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Admin));

        string uniqueEmail = $"integration-create-{Guid.NewGuid():N}@example.com";
        CreateUserRequest request = new CreateUserRequest
        {
            Email = uniqueEmail,
            UserName = $"user-{Guid.NewGuid():N}",
            Password = "Password123!",
            DynamicRoleId = ApplicationRoles.Employee
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
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Admin));

        string uniqueEmail = $"integration-invalid-password-{Guid.NewGuid():N}@example.com";
        CreateUserRequest request = new CreateUserRequest
        {
            Email = uniqueEmail,
            UserName = $"user-{Guid.NewGuid():N}",
            Password = "123",
            DynamicRoleId = ApplicationRoles.Employee
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
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Employee));

        string uniqueEmail = $"integration-forbidden-{Guid.NewGuid():N}@example.com";
        CreateUserRequest request = new CreateUserRequest
        {
            Email = uniqueEmail,
            UserName = $"user-{Guid.NewGuid():N}",
            Password = "Password123!",
            DynamicRoleId = ApplicationRoles.Employee
        };

        // Act
        HttpResponseMessage response = await m_client.PostAsJsonAsync("/api/users", request);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateUser_WithAdminRole_AndValidData_Returns200Ok()
    {
        // Préparation : utilisateur jetable pour ne pas altérer les comptes seed.
        ApplicationUser user = await GetOrCreateDisposableUserAsync();
        string userId = user.Id;

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Admin));

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = $"integration-update-{Guid.NewGuid():N}@example.com",
            UserName = $"updated-{Guid.NewGuid():N}",
            DynamicRoleId = ApplicationRoles.Employee
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
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Admin));

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = $"integration-update-notfound-{Guid.NewGuid():N}@example.com",
            UserName = $"updated-{Guid.NewGuid():N}",
            DynamicRoleId = ApplicationRoles.Employee
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
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Admin));

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = "mauvais-email",
            UserName = $"updated-{Guid.NewGuid():N}",
            DynamicRoleId = ApplicationRoles.Employee
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
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Employee));

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = $"integration-update-forbidden-{Guid.NewGuid():N}@example.com",
            UserName = $"updated-{Guid.NewGuid():N}",
            DynamicRoleId = ApplicationRoles.Employee
        };

        // Action
        HttpResponseMessage response = await m_client.PutAsJsonAsync("/api/users/nimporte-quel-id", request);

        // Vérification
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task DeleteUser_WithAdminRole_AndValidId_Returns204NoContent()
    {
        // Préparation : utilisateur jetable pour ne pas altérer les comptes seed.
        ApplicationUser user = await CreateDisposableUserAsync();
        string userId = user.Id;

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Admin));

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

    [Fact]
    public async Task DeleteUser_WithAdminRole_AndInvalidId_Returns404NotFound()
    {
        // Préparation
        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Admin));

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
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Employee));

        const string userId = "any-user-id";

        // Action
        HttpResponseMessage response = await m_client.DeleteAsync($"/api/users/{userId}");

        // Vérification
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetUsers_WithAdminRole_DoesNotReturnInactiveUsers()
    {
        // Préparation : on désactive un utilisateur jetable (pas un compte seed).
        ApplicationUser user = await CreateDisposableUserAsync();

        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext dbContext = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        ApplicationUser? trackedUser = await dbContext.Users
            .FirstOrDefaultAsync(p_u => p_u.Id == user.Id);
        Assert.NotNull(trackedUser);

        trackedUser.IsActive = false;
        await dbContext.SaveChangesAsync();

        m_client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            "Bearer",
            await m_factory.CreateJwtForSeededRoleAsync(ApplicationRoles.Admin));

        // Action
        HttpResponseMessage response = await m_client.GetAsync("/api/users");

        // Vérification
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        List<UserResponse>? users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();
        Assert.NotNull(users);
        Assert.DoesNotContain(users, p_user => p_user.Id == trackedUser.Id);
    }

    public void Dispose()
    {
        m_client.Dispose();
    }

    private async Task<ApplicationUser> CreateDisposableUserAsync(string p_dynamicRoleId = ApplicationRoles.Employee)
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        string uniqueSuffix = Guid.NewGuid().ToString("N");
        ApplicationUser user = new ApplicationUser
        {
            UserName = $"integration-disposable-{uniqueSuffix}",
            Email = $"integration-disposable-{uniqueSuffix}@example.com",
            EmailConfirmed = true,
            DynamicRoleId = p_dynamicRoleId,
        };

        IdentityResult result = await userManager.CreateAsync(user, "Password123!").ConfigureAwait(false);
        Assert.True(result.Succeeded);

        return user;
    }

    private async Task<ApplicationUser> GetOrCreateDisposableUserAsync()
    {
        using IServiceScope scope = m_factory.Services.CreateScope();
        CrystalDbContext dbContext = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        ApplicationUser? existingUser = await dbContext.Users
            .Where(p_user => p_user.IsActive && p_user.Email != null && !p_user.Email.EndsWith(SeedEmailDomain))
            .OrderBy(p_user => p_user.Id)
            .FirstOrDefaultAsync()
            .ConfigureAwait(false);

        if (existingUser is not null)
        {
            return existingUser;
        }

        return await CreateDisposableUserAsync().ConfigureAwait(false);
    }
}
