using Crystal.Core.Constants;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
using Crystal.Core.Interfaces.Repositories;
using Crystal.Core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Crystal.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> m_userManager;
    private readonly SignInManager<ApplicationUser> m_signInManager;
    private readonly IConfiguration m_configuration;
    private readonly IDynamicRoleRepository m_dynamicRoleRepository;

    public AuthService(
        UserManager<ApplicationUser> p_userManager,
        SignInManager<ApplicationUser> p_signInManager,
        IConfiguration p_configuration,
        IDynamicRoleRepository p_dynamicRoleRepository)
    {
        m_userManager = p_userManager;
        m_signInManager = p_signInManager;
        m_configuration = p_configuration;
        m_dynamicRoleRepository = p_dynamicRoleRepository;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest p_request, CancellationToken p_cancellationToken = default)
    {
        string loginIdentifier = p_request.GetLoginIdentifier();
        ApplicationUser? user = await m_userManager.FindByNameAsync(loginIdentifier).ConfigureAwait(false);
        user ??= await m_userManager.FindByEmailAsync(loginIdentifier).ConfigureAwait(false);

        if (user is null)
        {
            return null;
        }

        SignInResult signIn = await m_signInManager.CheckPasswordSignInAsync(user, p_request.Password, lockoutOnFailure: true)
            .ConfigureAwait(false);

        if (!signIn.Succeeded)
        {
            return null;
        }

        string token = CreateJwtToken(user);

        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            UserName = user.UserName ?? user.Email ?? string.Empty,
            DynamicRoleId = user.DynamicRoleId,
        };
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest p_request, CancellationToken p_cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(p_request.DynamicRoleId))
        {
            return new RegisterResult
            {
                Succeeded = false,
                Errors = new[] { ErrorMessages.User.RoleRequired }
            };
        }

        bool roleExists = await m_dynamicRoleRepository.ExistsAsync(p_request.DynamicRoleId).ConfigureAwait(false);
        if (!roleExists)
        {
            return new RegisterResult
            {
                Succeeded = false,
                Errors = new[] { ErrorMessages.User.RoleNotFound }
            };
        }

        ApplicationUser user = new()
        {
            UserName = p_request.UserName,
            Email = p_request.Email,
            DynamicRoleId = p_request.DynamicRoleId,
        };

        IdentityResult createResult = await m_userManager.CreateAsync(user, p_request.Password).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            return new RegisterResult
            {
                Succeeded = false,
                Errors = createResult.Errors.Select(p_e => p_e.Description).ToList()
            };
        }

        return new RegisterResult { Succeeded = true };
    }

    private string CreateJwtToken(ApplicationUser p_user)
    {
        IConfigurationSection jwtSettings = m_configuration.GetRequiredSection("Jwt");
        string key = jwtSettings["Key"] ?? throw new InvalidOperationException(ErrorMessages.Auth.JwtKeyMissing);
        string? issuer = jwtSettings["Issuer"];
        string? audience = jwtSettings["Audience"];

        SymmetricSecurityKey securityKey = new(Encoding.UTF8.GetBytes(key));
        SigningCredentials credentials = new(securityKey, SecurityAlgorithms.HmacSha256);

        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, p_user.Id),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, p_user.Id),
            new Claim(ClaimTypes.Name, p_user.UserName ?? string.Empty),
        ];

        if (!string.IsNullOrEmpty(p_user.Email))
        {
            claims.Add(new Claim(JwtRegisteredClaimNames.Email, p_user.Email));
            claims.Add(new Claim(ClaimTypes.Email, p_user.Email));
        }

        JwtSecurityToken token = new(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(2),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
