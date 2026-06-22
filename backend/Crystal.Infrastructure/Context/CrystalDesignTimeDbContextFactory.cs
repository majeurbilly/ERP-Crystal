using Crystal.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Crystal.Infrastructure.Context;

/// <summary>
/// Factory utilisée par les outils EF (dotnet ef) au design-time pour charger la configuration
/// depuis Crystal.API et appliquer la même résolution de chaîne de connexion que l'application.
/// </summary>
public class CrystalDesignTimeDbContextFactory : IDesignTimeDbContextFactory<CrystalDbContext>
{
    public CrystalDbContext CreateDbContext(string[] args)
    {
        string apiProjectDirectory = ResolveApiProjectDirectory();
        string environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{environmentName}.json", optional: true, reloadOnChange: false)
            .AddEnvironmentVariables()
            .Build();

        string connectionString = DatabaseConnectionResolver.Resolve(configuration);

        DbContextOptionsBuilder<CrystalDbContext> optionsBuilder = new DbContextOptionsBuilder<CrystalDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new CrystalDbContext(optionsBuilder.Options);
    }

    private static string ResolveApiProjectDirectory()
    {
        string currentDirectory = Directory.GetCurrentDirectory();

        string siblingApiPath = Path.GetFullPath(Path.Combine(currentDirectory, "..", "Crystal.API"));
        if (File.Exists(Path.Combine(siblingApiPath, "appsettings.json")))
        {
            return siblingApiPath;
        }

        string childApiPath = Path.GetFullPath(Path.Combine(currentDirectory, "Crystal.API"));
        if (File.Exists(Path.Combine(childApiPath, "appsettings.json")))
        {
            return childApiPath;
        }

        throw new InvalidOperationException(
            "Impossible de localiser Crystal.API/appsettings.json pour les outils EF. " +
            "Exécutez la commande depuis le dossier backend/ ou précisez --connection.");
    }
}
