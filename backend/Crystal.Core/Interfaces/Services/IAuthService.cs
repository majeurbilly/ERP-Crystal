using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;

namespace Crystal.Core.Interfaces.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest p_request, CancellationToken p_cancellationToken = default);

    Task<RegisterResult> RegisterAsync(RegisterRequest p_request, CancellationToken p_cancellationToken = default);
}
