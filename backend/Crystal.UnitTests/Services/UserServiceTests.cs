using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace Crystal.UnitTests.Services;

public class UserServiceTests
{
    [Fact]
    public async Task UpdateUserAsync_UserNotFound_ReturnsNull()
    {
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = "updated@example.com",
            UserName = "updated-user",
            DynamicRoleId = "Gerant",
        };

        userManagerMock
            .Setup(p_m => p_m.FindByIdAsync("missing-id"))
            .ReturnsAsync((ApplicationUser?)null);

        UserResponse? result = await service.UpdateUserAsync("missing-id", request, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateUserAsync_UpdateFails_ThrowsInvalidOperationException()
    {
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        ApplicationUser user = new ApplicationUser
        {
            Id = "user-1",
            Email = "old@example.com",
            UserName = "old-user",
            DynamicRoleId = "Employee",
        };

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = "new@example.com",
            UserName = "new-user",
            DynamicRoleId = "Admin",
        };

        IdentityResult failedUpdate = IdentityResult.Failed(new IdentityError { Description = "Update failed" });

        userManagerMock
            .Setup(p_m => p_m.FindByIdAsync("user-1"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(p_m => p_m.UpdateAsync(user))
            .ReturnsAsync(failedUpdate);

        InvalidOperationException exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateUserAsync("user-1", request, CancellationToken.None));

        Assert.Equal("Update failed", exception.Message);
    }

    [Fact]
    public async Task UpdateUserAsync_AllStepsSucceed_ReturnsUserResponse()
    {
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        ApplicationUser user = new ApplicationUser
        {
            Id = "user-4",
            Email = "old4@example.com",
            UserName = "old4-user",
            DynamicRoleId = "Employee",
        };

        UpdateUserRequest request = new UpdateUserRequest
        {
            Email = "new4@example.com",
            UserName = "new4-user",
            DynamicRoleId = "Gerant",
        };

        IdentityResult success = IdentityResult.Success;

        userManagerMock
            .Setup(p_m => p_m.FindByIdAsync("user-4"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(p_m => p_m.UpdateAsync(user))
            .ReturnsAsync(success);

        UserResponse? result = await service.UpdateUserAsync("user-4", request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("user-4", result.Id);
        Assert.Equal("new4@example.com", result.Email);
        Assert.Equal("new4-user", result.UserName);
        Assert.Equal("Gerant", result.DynamicRoleId);
    }

    [Fact]
    public async Task UpdateProfileAsync_UserNotFound_ReturnsNull()
    {
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        UpdateProfileRequest request = new UpdateProfileRequest
        {
            Email = "updated-profile@example.com",
            UserName = "updated-profile-user"
        };

        userManagerMock
            .Setup(p_m => p_m.FindByIdAsync("missing-profile-id"))
            .ReturnsAsync((ApplicationUser?)null);

        UserResponse? result = await service.UpdateProfileAsync("missing-profile-id", request, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateProfileAsync_UpdateFails_ThrowsArgumentException()
    {
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        ApplicationUser user = new ApplicationUser
        {
            Id = "profile-user-1",
            Email = "old-profile@example.com",
            UserName = "old-profile-user",
            DynamicRoleId = "Employee",
        };

        UpdateProfileRequest request = new UpdateProfileRequest
        {
            Email = "new-profile@example.com",
            UserName = "new-profile-user"
        };

        IdentityResult failedUpdate = IdentityResult.Failed(new IdentityError { Description = "Profile update failed" });

        userManagerMock
            .Setup(p_m => p_m.FindByIdAsync("profile-user-1"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(p_m => p_m.UpdateAsync(user))
            .ReturnsAsync(failedUpdate);

        ArgumentException exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateProfileAsync("profile-user-1", request, CancellationToken.None));

        Assert.Equal("Profile update failed", exception.Message);
    }

    [Fact]
    public async Task UpdateProfileAsync_Success_ReturnsUpdatedUserResponse()
    {
        UserService service = CreateService(out Mock<UserManager<ApplicationUser>> userManagerMock);
        ApplicationUser user = new ApplicationUser
        {
            Id = "profile-user-2",
            Email = "old2-profile@example.com",
            UserName = "old2-profile-user",
            DynamicRoleId = "Employee",
        };

        UpdateProfileRequest request = new UpdateProfileRequest
        {
            Email = "new2-profile@example.com",
            UserName = "new2-profile-user"
        };

        IdentityResult success = IdentityResult.Success;

        userManagerMock
            .Setup(p_m => p_m.FindByIdAsync("profile-user-2"))
            .ReturnsAsync(user);
        userManagerMock
            .Setup(p_m => p_m.UpdateAsync(user))
            .ReturnsAsync(success);

        UserResponse? result = await service.UpdateProfileAsync("profile-user-2", request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("profile-user-2", result.Id);
        Assert.Equal("new2-profile@example.com", result.Email);
        Assert.Equal("new2-profile-user", result.UserName);
        Assert.Equal("Employee", result.DynamicRoleId);
    }

    private static UserService CreateService(out Mock<UserManager<ApplicationUser>> p_userManagerMock)
    {
        Mock<IUserStore<ApplicationUser>> userStoreMock = new Mock<IUserStore<ApplicationUser>>();
        Mock<UserManager<ApplicationUser>> userManagerMock =
            new Mock<UserManager<ApplicationUser>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);
        Mock<IDynamicRoleRepository> dynamicRoleRepositoryMock = new Mock<IDynamicRoleRepository>();
        dynamicRoleRepositoryMock
            .Setup(p_repository => p_repository.ExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(true);

        p_userManagerMock = userManagerMock;

        return new UserService(userManagerMock.Object, dynamicRoleRepositoryMock.Object);
    }
}
