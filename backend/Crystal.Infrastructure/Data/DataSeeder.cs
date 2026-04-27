using Crystal.Core;
using Crystal.Core.Entities;
using Crystal.Infrastructure.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;

namespace Crystal.Infrastructure.Data;

public static class DataSeeder
{
    private const string DefaultPassword = "ValidPass1!a";

    public static async Task SeedRolesAndUsersAsync(IServiceProvider p_serviceProvider)
    {
        RoleManager<IdentityRole> roleManager = p_serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        UserManager<ApplicationUser> userManager = p_serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (string roleName in ApplicationRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(roleName).ConfigureAwait(false))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName)).ConfigureAwait(false);
            }
        }

        (string Email, string Role)[] testAccounts =
        [
            ("admin@crystal.local", ApplicationRoles.Admin),
            ("gerant@crystal.local", ApplicationRoles.Gerant),
            ("assistant@crystal.local", ApplicationRoles.Assistant),
            ("employee@crystal.local", ApplicationRoles.Employee),
        ];

        foreach ((string Email, string Role) account in testAccounts)
        {
            string email = account.Email;

            ApplicationUser? user = await userManager.FindByEmailAsync(email).ConfigureAwait(false)
                ?? await userManager.FindByNameAsync(email).ConfigureAwait(false);

            if (user is null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true,
                };

                Console.WriteLine($"[DataSeeder] CreateAsync - before user creation: {email}");
                IdentityResult result = await userManager.CreateAsync(user, DefaultPassword).ConfigureAwait(false);
                Console.WriteLine($"[DataSeeder] CreateAsync - after user creation: {email}, Succeeded={result.Succeeded}");

                if (!result.Succeeded)
                {
                    foreach (IdentityError err in result.Errors)
                    {
                        Console.WriteLine($"[DataSeeder] Identity error: Code={err.Code}, Description={err.Description}");
                    }

                    continue;
                }
            }

            if (!await userManager.IsInRoleAsync(user, account.Role).ConfigureAwait(false))
            {
                await userManager.AddToRoleAsync(user, account.Role).ConfigureAwait(false);
            }
        }
    }

    public static async Task SeedLocationsAsync(IServiceProvider p_serviceProvider)
    {
        CrystalDbContext context = p_serviceProvider.GetRequiredService<CrystalDbContext>();

        if (await context.Locations.AnyAsync().ConfigureAwait(false))
        {
            return;
        }

        Location[] locations =
        [
            new Location
            {
                Title = "Succursale Québec",
                Address = "123 Rue Saint-Jean, Québec, QC",
                Description = "Succursale principale"
            },
            new Location
            {
                Title = "Succursale Sainte-Foy",
                Address = "2450 Boulevard Laurier, Québec, QC",
                Description = "Succursale secondaire"
            }
        ];

        await context.Locations.AddRangeAsync(locations).ConfigureAwait(false);
        await context.SaveChangesAsync().ConfigureAwait(false);
    }

    public static async Task SeedAllAsync(IServiceProvider p_serviceProvider)
    {
        await SeedRolesAndUsersAsync(p_serviceProvider).ConfigureAwait(false);
        await SeedLocationsAsync(p_serviceProvider).ConfigureAwait(false);
    }
}