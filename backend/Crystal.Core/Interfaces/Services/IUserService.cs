using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IUserService
{
    Task<IEnumerable<UserResponse>> GetAllUsersAsync(CancellationToken p_cancellationToken = default);
    Task<UserResponse?> GetUserByIdAsync(string p_id, CancellationToken p_cancellationToken = default);
    Task<UserResponse> CreateUserAsync(CreateUserRequest p_request, CancellationToken p_cancellationToken = default);
    Task<UserResponse?> UpdateUserAsync(string p_id, UpdateUserRequest p_request, CancellationToken p_cancellationToken = default);
    Task<UserResponse?> UpdateProfileAsync(string p_userId, UpdateProfileRequest p_request, CancellationToken p_cancellationToken = default);
    Task<bool> DeleteUserAsync(string p_id, CancellationToken p_cancellationToken = default);
}
