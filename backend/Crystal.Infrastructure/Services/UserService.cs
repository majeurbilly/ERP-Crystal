using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System.Linq;

namespace Crystal.Infrastructure.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> m_userManager;

    public UserService(UserManager<ApplicationUser> p_userManager)
    {
        m_userManager = p_userManager;
    }

    public async Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken p_cancellationToken = default)
    {
        List<ApplicationUser> users = await m_userManager.Users
            .Where(p_user => p_user.IsActive)
            .ToListAsync(p_cancellationToken)
            .ConfigureAwait(false);

        List<UserResponse> responses = new(users.Count);

        foreach (ApplicationUser user in users)
        {
            IList<string> roles = await m_userManager.GetRolesAsync(user).ConfigureAwait(false);

            responses.Add(new UserResponse
            {
                Id = user.Id,
                Email = user.Email ?? string.Empty,
                UserName = user.UserName ?? string.Empty,
                Roles = roles.ToList()
            });
        }

        return responses;
    }

    public async Task<HrMetricsResponse> GetHrMetricsAsync(CancellationToken p_cancellationToken = default)
    {
        IQueryable<ApplicationUser> users = m_userManager.Users;
        int totalActiveUsers;
        int totalInactiveUsers;

        if (users.Provider is IAsyncQueryProvider)
        {
            totalActiveUsers = await users
                .CountAsync(p_user => p_user.IsActive, p_cancellationToken)
                .ConfigureAwait(false);

            totalInactiveUsers = await users
                .CountAsync(p_user => p_user.IsActive == false, p_cancellationToken)
                .ConfigureAwait(false);
        }
        else
        {
            totalActiveUsers = users.Count(p_user => p_user.IsActive);
            totalInactiveUsers = users.Count(p_user => p_user.IsActive == false);
        }

        return new HrMetricsResponse
        {
            TotalActiveUsers = totalActiveUsers,
            TotalInactiveUsers = totalInactiveUsers
        };
    }

    public async Task<UserResponse?> GetUserByIdAsync(string p_id, CancellationToken p_cancellationToken = default)
    {
        ApplicationUser? user = await m_userManager.Users
            .FirstOrDefaultAsync(p_user => p_user.Id == p_id && p_user.IsActive, p_cancellationToken)
            .ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        IList<string> roles = await m_userManager.GetRolesAsync(user).ConfigureAwait(false);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            Roles = roles.ToList()
        };
    }

    public async Task<UserResponse> CreateUserAsync(CreateUserRequest p_request, CancellationToken p_cancellationToken = default)
    {
        ApplicationUser user = new ApplicationUser
        {
            UserName = p_request.UserName,
            Email = p_request.Email
        };

        IdentityResult result = await m_userManager.CreateAsync(user, p_request.Password).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            IdentityError? firstError = result.Errors.FirstOrDefault();
            string errorMessage = firstError?.Description ?? "Unable to create user.";
            throw new InvalidOperationException(errorMessage);
        }

        IdentityResult roleResult = await m_userManager.AddToRoleAsync(user, p_request.Role).ConfigureAwait(false);
        if (!roleResult.Succeeded)
        {
            IdentityError? firstRoleError = roleResult.Errors.FirstOrDefault();
            string roleErrorMessage = firstRoleError?.Description ?? "Unable to assign role to user.";
            throw new InvalidOperationException(roleErrorMessage);
        }

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            Roles = new List<string> { p_request.Role }
        };
    }

    public async Task<UserResponse?> UpdateUserAsync(string p_id, UpdateUserRequest p_request, CancellationToken p_cancellationToken = default)
    {
        ApplicationUser? user = await m_userManager.FindByIdAsync(p_id).ConfigureAwait(false);
        if (user is null)
        {
            return null;
        }

        user.Email = p_request.Email;
        user.UserName = p_request.UserName;

        IdentityResult updateResult = await m_userManager.UpdateAsync(user).ConfigureAwait(false);
        if (!updateResult.Succeeded)
        {
            IdentityError? firstUpdateError = updateResult.Errors.FirstOrDefault();
            string updateErrorMessage = firstUpdateError?.Description ?? "Unable to update user.";
            throw new InvalidOperationException(updateErrorMessage);
        }

        IList<string> currentRoles = await m_userManager.GetRolesAsync(user).ConfigureAwait(false);
        IdentityResult removeResult = await m_userManager.RemoveFromRolesAsync(user, currentRoles).ConfigureAwait(false);
        if (!removeResult.Succeeded)
        {
            IdentityError? firstRemoveError = removeResult.Errors.FirstOrDefault();
            string removeErrorMessage = firstRemoveError?.Description ?? "Unable to remove current roles.";
            throw new InvalidOperationException(removeErrorMessage);
        }

        IdentityResult addResult = await m_userManager.AddToRoleAsync(user, p_request.Role).ConfigureAwait(false);
        if (!addResult.Succeeded)
        {
            IdentityError? firstAddError = addResult.Errors.FirstOrDefault();
            string addErrorMessage = firstAddError?.Description ?? "Unable to assign new role.";
            throw new InvalidOperationException(addErrorMessage);
        }

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            Roles = new List<string> { p_request.Role }
        };
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

        IdentityResult result = await m_userManager.UpdateAsync(user).ConfigureAwait(false);
        if (!result.Succeeded)
        {
            IdentityError? firstError = result.Errors.FirstOrDefault();
            string errorMessage = firstError?.Description ?? "Unable to update profile.";
            throw new InvalidOperationException(errorMessage);
        }

        IList<string> currentRoles = await m_userManager.GetRolesAsync(user).ConfigureAwait(false);

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email ?? string.Empty,
            UserName = user.UserName ?? string.Empty,
            Roles = currentRoles.ToList()
        };
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
            string errorMessage = firstError?.Description ?? "Unable to delete user.";
            throw new InvalidOperationException(errorMessage);
        }

        return true;
    }
}
