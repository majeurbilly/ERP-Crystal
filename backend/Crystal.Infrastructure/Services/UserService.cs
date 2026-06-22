using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> m_userManager;
    private readonly IDynamicRoleRepository m_dynamicRoleRepository;

    public UserService(
        UserManager<ApplicationUser> p_userManager,
        IDynamicRoleRepository p_dynamicRoleRepository)
    {
        m_userManager = p_userManager;
        m_dynamicRoleRepository = p_dynamicRoleRepository;
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken p_cancellationToken = default)
    {
        List<ApplicationUser> users = await m_userManager.Users
            .Where(p_user => p_user.IsActive)
            .Include(p_user => p_user.DynamicRole)
            .ToListAsync(p_cancellationToken)
            .ConfigureAwait(false);

        List<UserResponse> responses = new(users.Count);

        foreach (ApplicationUser user in users)
        {
            responses.Add(await MapUserResponseAsync(user));
        }

        return responses;
    }

    public async Task<UserResponse?> GetUserByIdAsync(string p_id, CancellationToken p_cancellationToken = default)
    {
        ApplicationUser? user = await m_userManager.FindByIdAsync(p_id).ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        await AttachDynamicRoleAsync(user);
        return await MapUserResponseAsync(user);
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest p_request, CancellationToken p_cancellationToken = default)
    {
        await ValidateDynamicRoleIdAsync(p_request.DynamicRoleId);

        ApplicationUser user = new ApplicationUser
        {
            UserName = p_request.UserName,
            Email = p_request.Email,
            DynamicRoleId = p_request.DynamicRoleId,
        };

        IdentityResult result = await m_userManager.CreateAsync(user, p_request.Password).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            IdentityError? firstError = result.Errors.FirstOrDefault();
            string errorMessage = firstError?.Description ?? ErrorMessages.User.UnableToCreateUser;
            throw new InvalidOperationException(errorMessage);
        }

        await AttachDynamicRoleAsync(user);
        return await MapUserResponseAsync(user);
    }

    public async Task<UserResponse?> UpdateUserAsync(string p_id, UpdateUserRequest p_request, CancellationToken p_cancellationToken = default)
    {
        ApplicationUser? user = await m_userManager.FindByIdAsync(p_id).ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        await ValidateDynamicRoleIdAsync(p_request.DynamicRoleId);

        user.Email = p_request.Email;
        user.UserName = p_request.UserName;
        user.DynamicRoleId = p_request.DynamicRoleId;

        await UpdatePasswordIfProvidedAsync(user, p_request.Password).ConfigureAwait(false);

        IdentityResult updateResult = await m_userManager.UpdateAsync(user).ConfigureAwait(false);
        if (!updateResult.Succeeded)
        {
            IdentityError? firstUpdateError = updateResult.Errors.FirstOrDefault();
            string updateErrorMessage = firstUpdateError?.Description ?? ErrorMessages.User.UnableToUpdateUser;
            throw new InvalidOperationException(updateErrorMessage);
        }

        await AttachDynamicRoleAsync(user);
        return await MapUserResponseAsync(user);
    }

    public async Task<UserResponse?> UpdateProfileAsync(string p_userId, UpdateProfileRequest p_request, CancellationToken p_cancellationToken = default)
    {
        ApplicationUser? user = await m_userManager.FindByIdAsync(p_userId).ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        user.Email = p_request.Email;
        user.UserName = p_request.UserName;

        await UpdatePasswordIfProvidedAsync(user, p_request.Password).ConfigureAwait(false);

        IdentityResult result = await m_userManager.UpdateAsync(user).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            IdentityError? firstError = result.Errors.FirstOrDefault();
            string errorMessage = firstError?.Description ?? ErrorMessages.User.UnableToUpdateProfile;
            throw new ArgumentException(errorMessage);
        }

        await AttachDynamicRoleAsync(user);
        return await MapUserResponseAsync(user);
    }

    public async Task<bool> DeleteUserAsync(string p_id, CancellationToken p_cancellationToken = default)
    {
        ApplicationUser? user = await m_userManager.FindByIdAsync(p_id).ConfigureAwait(false);
        if (user is null || user.IsActive == false)
        {
            return false;
        }

        user.IsActive = false;

        IdentityResult result = await m_userManager.UpdateAsync(user).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            IdentityError? firstError = result.Errors.FirstOrDefault();
            string errorMessage = firstError?.Description ?? ErrorMessages.User.UnableToDeleteUser;
            throw new InvalidOperationException(errorMessage);
        }

        return true;
    }

    private async Task UpdatePasswordIfProvidedAsync(ApplicationUser p_user, string? p_password)
    {
        if (string.IsNullOrWhiteSpace(p_password))
        {
            return;
        }

        string resetToken = await m_userManager.GeneratePasswordResetTokenAsync(p_user).ConfigureAwait(false);
        IdentityResult passwordResult = await m_userManager
            .ResetPasswordAsync(p_user, resetToken, p_password)
            .ConfigureAwait(false);

        if (!passwordResult.Succeeded)
        {
            IdentityError? firstError = passwordResult.Errors.FirstOrDefault();
            string errorMessage = firstError?.Description ?? ErrorMessages.User.UnableToUpdatePassword;
            throw new ArgumentException(errorMessage);
        }
    }

    private async Task AttachDynamicRoleAsync(ApplicationUser p_user)
    {
        if (!string.IsNullOrWhiteSpace(p_user.DynamicRoleId) && p_user.DynamicRole is null)
        {
            p_user.DynamicRole = await m_dynamicRoleRepository.GetByIdAsync(p_user.DynamicRoleId);
        }
    }

    private async Task ValidateDynamicRoleIdAsync(string? p_dynamicRoleId)
    {
        if (string.IsNullOrWhiteSpace(p_dynamicRoleId))
        {
            throw new ArgumentException(ErrorMessages.User.RoleRequired);
        }

        bool exists = await m_dynamicRoleRepository.ExistsAsync(p_dynamicRoleId);
        if (!exists)
        {
            throw new KeyNotFoundException(ErrorMessages.User.RoleNotFound);
        }
    }

    private async Task<UserResponse> MapUserResponseAsync(ApplicationUser p_user)
    {
        string? dynamicRoleName = p_user.DynamicRole?.Name;

        if (string.IsNullOrWhiteSpace(dynamicRoleName) && !string.IsNullOrWhiteSpace(p_user.DynamicRoleId))
        {
            DynamicRole? dynamicRole = await m_dynamicRoleRepository.GetByIdAsync(p_user.DynamicRoleId);
            dynamicRoleName = dynamicRole?.Name;
        }

        return new UserResponse
        {
            Id = p_user.Id,
            Email = p_user.Email ?? string.Empty,
            UserName = p_user.UserName ?? string.Empty,
            DynamicRoleId = p_user.DynamicRoleId,
            DynamicRoleName = dynamicRoleName,
        };
    }
}
