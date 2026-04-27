using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Crystal.UnitTests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task GetHrMetricsAsync_ReturnsCorrectCounts()
    {
        // Arrange
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        List<ApplicationUser> users =
        [
            new ApplicationUser { Id = "user-1", IsActive = true },
            new ApplicationUser { Id = "user-2", IsActive = true },
            new ApplicationUser { Id = "user-3", IsActive = false },
            new ApplicationUser { Id = "user-4", IsActive = false },
            new ApplicationUser { Id = "user-5", IsActive = false }
        ];

        userManagerMock
            .SetupGet(m => m.Users)
            .Returns(users.AsQueryable());

        // Act
        HrMetricsResponse result = await service.GetHrMetricsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalActiveUsers);
        Assert.Equal(3, result.TotalInactiveUsers);
    }

    [Fact]
    public async Task UpdateUserAsync_UserNotFound_ReturnsNull()
    {
        // Arrange
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = "updated@example.com",
            UserName = "updated-user",
            Role = "Gerant"
        };

        userManagerMock
            .Setup(m => m.FindByIdAsync("missing-id"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        UserResponse? result = await service.UpdateUserAsync("missing-id", request, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdateFails_ThrowsInvalidOperationException()
    {
        // Arrange
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        ApplicationUser user = new ApplicationUser
        {
            Id = "user-1",
            Email = "old@example.com",
            UserName = "old-user"
        };

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = "new@example.com",
            UserName = "new-user",
            Role = "Admin"
        };

        IdentityResult failedUpdate = IdentityResult.Failed(new IdentityError { Description = "Update failed" });

        userManagerMock
            .Setup(m => m.FindByIdAsync("user-1"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(failedUpdate);

        // Act + Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateUserAsync("user-1", request, CancellationToken.None));

        Assert.Equal("Update failed", exception.Message);
    }

    [Fact]
    public async Task UpdateUserAsync_RemoveRolesFails_ThrowsInvalidOperationException()
    {
        // Arrange
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        ApplicationUser user = new ApplicationUser
        {
            Id = "user-2",
            Email = "old2@example.com",
            UserName = "old2-user"
        };

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = "new2@example.com",
            UserName = "new2-user",
            Role = "Gerant"
        };

        IdentityResult success = IdentityResult.Success;
        IdentityResult failedRemove = IdentityResult.Failed(new IdentityError { Description = "Remove role failed" });
        List<string> currentRoles = new List<string> { "Employee" };

        userManagerMock
            .Setup(m => m.FindByIdAsync("user-2"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(success);
        userManagerMock
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(currentRoles);
        userManagerMock
            .Setup(m => m.RemoveFromRolesAsync(user, currentRoles))
            .ReturnsAsync(failedRemove);

        // Act + Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateUserAsync("user-2", request, CancellationToken.None));

        Assert.Equal("Remove role failed", exception.Message);
    }

    [Fact]
    public async Task UpdateUserAsync_AddRoleFails_ThrowsInvalidOperationException()
    {
        // Arrange
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        ApplicationUser user = new ApplicationUser
        {
            Id = "user-3",
            Email = "old3@example.com",
            UserName = "old3-user"
        };

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = "new3@example.com",
            UserName = "new3-user",
            Role = "Admin"
        };

        IdentityResult success = IdentityResult.Success;
        IdentityResult failedAdd = IdentityResult.Failed(new IdentityError { Description = "Add role failed" });
        List<string> currentRoles = new List<string> { "Employee" };

        userManagerMock
            .Setup(m => m.FindByIdAsync("user-3"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(success);
        userManagerMock
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(currentRoles);
        userManagerMock
            .Setup(m => m.RemoveFromRolesAsync(user, currentRoles))
            .ReturnsAsync(success);
        userManagerMock
            .Setup(m => m.AddToRoleAsync(user, "Admin"))
            .ReturnsAsync(failedAdd);

        // Act + Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateUserAsync("user-3", request, CancellationToken.None));

        Assert.Equal("Add role failed", exception.Message);
    }

    [Fact]
    public async Task UpdateUserAsync_AllStepsSucceed_ReturnsUserResponse()
    {
        // Arrange
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        ApplicationUser user = new ApplicationUser
        {
            Id = "user-4",
            Email = "old4@example.com",
            UserName = "old4-user"
        };

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = "new4@example.com",
            UserName = "new4-user",
            Role = "Gerant"
        };

        IdentityResult success = IdentityResult.Success;
        List<string> currentRoles = new List<string> { "Employee" };

        userManagerMock
            .Setup(m => m.FindByIdAsync("user-4"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(success);
        userManagerMock
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(currentRoles);
        userManagerMock
            .Setup(m => m.RemoveFromRolesAsync(user, currentRoles))
            .ReturnsAsync(success);
        userManagerMock
            .Setup(m => m.AddToRoleAsync(user, "Gerant"))
            .ReturnsAsync(success);

        // Act
        UserResponse? result = await service.UpdateUserAsync("user-4", request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("user-4", result.Id);
        Assert.Equal("new4@example.com", result.Email);
        Assert.Equal("new4-user", result.UserName);
        Assert.Single(result.Roles);
        Assert.Equal("Gerant", result.Roles[0]);
    }

    [Fact]
    public async Task UpdateProfileAsync_UserNotFound_ReturnsNull()
    {
        // Arrange
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        UpdateProfileRequest request = new UpdateProfileRequest
        {
            Email = "updated-profile@example.com",
            UserName = "updated-profile-user"
        };

        userManagerMock
            .Setup(m => m.FindByIdAsync("missing-profile-id"))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        UserResponse? result = await service.UpdateProfileAsync("missing-profile-id", request, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProfileAsync_UpdateFails_ThrowsInvalidOperationException()
    {
        // Arrange
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        ApplicationUser user = new ApplicationUser
        {
            Id = "profile-user-1",
            Email = "old-profile@example.com",
            UserName = "old-profile-user"
        };

        UpdateProfileRequest request = new UpdateProfileRequest
        {
            Email = "new-profile@example.com",
            UserName = "new-profile-user"
        };

        IdentityResult failedUpdate = IdentityResult.Failed(new IdentityError { Description = "Profile update failed" });

        userManagerMock
            .Setup(m => m.FindByIdAsync("profile-user-1"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(failedUpdate);

        // Act + Assert
        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateProfileAsync("profile-user-1", request, CancellationToken.None));

        Assert.Equal("Profile update failed", exception.Message);
    }

    [Fact]
    public async Task UpdateProfileAsync_Success_ReturnsUpdatedUserResponse()
    {
        // Arrange
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        ApplicationUser user = new ApplicationUser
        {
            Id = "profile-user-2",
            Email = "old2-profile@example.com",
            UserName = "old2-profile-user"
        };

        UpdateProfileRequest request = new UpdateProfileRequest
        {
            Email = "new2-profile@example.com",
            UserName = "new2-profile-user"
        };

        IdentityResult success = IdentityResult.Success;
        List<string> currentRoles = new List<string> { "Employee" };

        userManagerMock
            .Setup(m => m.FindByIdAsync("profile-user-2"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(m => m.UpdateAsync(user))
            .ReturnsAsync(success);
        userManagerMock
            .Setup(m => m.GetRolesAsync(user))
            .ReturnsAsync(currentRoles);

        // Act
        UserResponse? result = await service.UpdateProfileAsync("profile-user-2", request, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("profile-user-2", result.Id);
        Assert.Equal("new2-profile@example.com", result.Email);
        Assert.Equal("new2-profile-user", result.UserName);
        Assert.Single(result.Roles);
        Assert.Equal("Employee", result.Roles[0]);
    }

    private static UserService CreateService(out Mock<UserManager<ApplicationUser>> p_userManagerMock)
    {
        Mock<IUserStore<ApplicationUser>> userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        Mock<UserManager<ApplicationUser>> userManagerMock =
            new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        p_userManagerMock = userManagerMock;

        return new UserService(userManagerMock.Object);
    }
}
