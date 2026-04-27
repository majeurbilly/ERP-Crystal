using Crystal.Core;
using Crystal.Core.DTOs.Requests;
using Crystal.Core.DTOs.Responses;
using Crystal.Core.Entities;
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

    public AuthService(
        UserManager<ApplicationUser> p_userManager,
        SignInManager<ApplicationUser> p_signInManager,
        IConfiguration p_configuration)
    {
        m_userManager = p_userManager;
        m_signInManager = p_signInManager;
        m_configuration = p_configuration;
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

        IList<string> roles = await m_userManager.GetRolesAsync(user).ConfigureAwait(false);
        string token = CreateJwtToken(user, roles);

        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            UserName = user.UserName ?? user.Email ?? string.Empty,
            Roles = roles.ToList()
        };
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest p_request, CancellationToken p_cancellationToken = default)
    {
        if (!ApplicationRoles.All.Contains(p_request.Role))
        {
            return new RegisterResult
            {
                Succeeded = false,
                Errors = new[] { $"Role must be one of: {string.Join(", ", ApplicationRoles.All)}." }
            };
        }

        ApplicationUser user = new()
        {
            UserName = p_request.UserName,
            Email = p_request.Email
        };

        IdentityResult createResult = await m_userManager.CreateAsync(user, p_request.Password).ConfigureAwait(false);
        if (!createResult.Succeeded)
        {
            return new RegisterResult
            {
                Succeeded = false,
                Errors = createResult.Errors.Select(e => e.Description).ToList()
            };
        }

        IdentityResult roleResult = await m_userManager.AddToRoleAsync(user, p_request.Role).ConfigureAwait(false);
        if (!roleResult.Succeeded)
        {
            await m_userManager.DeleteAsync(user).ConfigureAwait(false);
            return new RegisterResult
            {
                Succeeded = false,
                Errors = roleResult.Errors.Select(e => e.Description).ToList()
            };
        }

        return new RegisterResult { Succeeded = true };
    }

    private string CreateJwtToken(ApplicationUser p_user, IList<string> p_roles)
    {
        IConfigurationSection jwtSettings = m_configuration.GetRequiredSection("Jwt");
        string key = jwtSettings["Key"] ?? throw new InvalidOperationException("Configuration Jwt:Key manquante.");
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

        foreach (string role in p_roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
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
