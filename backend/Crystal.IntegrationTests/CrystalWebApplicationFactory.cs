using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Crystal.Core;
using Crystal.Core.Entities;
using Crystal.Infrastructure.Context;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Crystal.IntegrationTests;

public sealed class CrystalWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string m_sqliteDatabaseName = $"crystal-it-{Guid.NewGuid():N}";

    public const string JwtKey = "CleIntegrationTestsAssezLonguePourHmacSha256!!";
    public const string JwtIssuer = "CrystalIntegrationTests";
    public const string JwtAudience = "CrystalIntegrationTestsUsers";

    private static readonly IReadOnlyDictionary<string, string> SeedEmailByRole = new Dictionary<string, string>
    {
        [ApplicationRoles.Admin] = "admin@crystal.local",
        [ApplicationRoles.Gerant] = "gerant@crystal.local",
        [ApplicationRoles.Assistant] = "assistant@crystal.local",
        [ApplicationRoles.Employee] = "employee@crystal.local",
    };

    protected override void ConfigureWebHost(IWebHostBuilder p_builder)
    {
        p_builder.UseEnvironment("Testing");

        p_builder.ConfigureAppConfiguration((p_, p_config) =>
        {
            p_config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = JwtKey,
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience
            });
        });

        p_builder.ConfigureServices(p_services =>
        {
            p_services.RemoveAll<SqliteConnection>();
            p_services.AddSingleton(_ =>
            {
                SqliteConnection connection = new($"Data Source={m_sqliteDatabaseName};Mode=Memory;Cache=Shared");
                connection.Open();
                return connection;
            });

            p_services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, p_options =>
            {
                p_options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey)),
                    ValidateIssuer = true,
                    ValidIssuer = JwtIssuer,
                    ValidateAudience = true,
                    ValidAudience = JwtAudience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    NameClaimType = System.Security.Claims.ClaimTypes.NameIdentifier,
                };
            });
        });
    }

    public static string CreateJwtForUserId(string p_userId)
    {
        List<Claim> claims =
        [
            new Claim(JwtRegisteredClaimNames.Sub, p_userId),
            new Claim(ClaimTypes.NameIdentifier, p_userId),
        ];

        SymmetricSecurityKey key = new(Encoding.UTF8.GetBytes(JwtKey));
        SigningCredentials credentials = new(key, SecurityAlgorithms.HmacSha256);

        JwtSecurityToken token = new(
            issuer: JwtIssuer,
            audience: JwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(1),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static string CreateJwtForUserIdAndRoles(string p_userId, params string[] p_roles)
    {
        return CreateJwtForUserId(p_userId);
    }

    public static string CreateJwtForRoles(params string[] p_roles)
    {
        return CreateJwtForUserId("integration-test-actor");
    }

    public async Task<string> CreateJwtForSeededRoleAsync(string p_role)
    {
        if (!SeedEmailByRole.TryGetValue(p_role, out string? email))
        {
            throw new ArgumentException($"No seed account mapped for role '{p_role}'.", nameof(p_role));
        }

        using IServiceScope scope = Services.CreateScope();
        UserManager<ApplicationUser> userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        CrystalDbContext context = scope.ServiceProvider.GetRequiredService<CrystalDbContext>();

        ApplicationUser? user = await userManager.FindByEmailAsync(email).ConfigureAwait(false);

        if (user is null)
        {
            user = await context.Users
                .Where(p_u => p_u.DynamicRoleId == p_role)
                .OrderByDescending(p_u => p_u.IsActive)
                .ThenBy(p_u => p_u.Id)
                .FirstOrDefaultAsync()
                .ConfigureAwait(false);
        }

        if (user is null)
        {
            throw new InvalidOperationException($"Seed user for role '{p_role}' (expected email '{email}') was not found.");
        }

        return CreateJwtForUserId(user.Id);
    }
}
