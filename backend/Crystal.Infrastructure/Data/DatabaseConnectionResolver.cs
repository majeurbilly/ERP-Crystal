using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Crystal.Infrastructure.Data;

public static class DatabaseConnectionResolver
{
    private const string DefaultHost = "localhost";
    private const string DefaultPort = "5433";
    private const string DefaultDatabase = "bd-erp-crystal";
    private const string DefaultUsername = "stringempty";
    private const string DefaultPassword = "Pizzapizza123";

    public static string Resolve(IConfiguration p_configuration)
    {
        string? configuredConnection = p_configuration.GetConnectionString("DefaultConnection");

        if (!string.IsNullOrWhiteSpace(configuredConnection))
        {
            return NormalizeConnectionString(configuredConnection);
        }

        return BuildFromEnvironmentVariables();
    }

    private static string NormalizeConnectionString(string p_connectionString)
    {
        NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder(p_connectionString);

        if (string.IsNullOrWhiteSpace(builder.Host))
        {
            builder.Host = GetEnvironmentValue("DB_HOST", DefaultHost);
        }

        if (builder.Port <= 0)
        {
            string portValue = GetEnvironmentValue("DB_PORT", DefaultPort);
            builder.Port = int.Parse(portValue);
        }

        if (string.IsNullOrWhiteSpace(builder.Database))
        {
            builder.Database = GetEnvironmentValue("DB_NAME", DefaultDatabase);
        }

        if (string.IsNullOrWhiteSpace(builder.Username))
        {
            builder.Username = GetEnvironmentValue("DB_USER", DefaultUsername);
        }

        if (string.IsNullOrWhiteSpace(builder.Password))
        {
            builder.Password = GetEnvironmentValue("DB_PASSWORD", DefaultPassword);
        }

        return builder.ConnectionString;
    }

    private static string BuildFromEnvironmentVariables()
    {
        NpgsqlConnectionStringBuilder builder = new NpgsqlConnectionStringBuilder
        {
            Host = GetEnvironmentValue("DB_HOST", DefaultHost),
            Port = int.Parse(GetEnvironmentValue("DB_PORT", DefaultPort)),
            Database = GetEnvironmentValue("DB_NAME", DefaultDatabase),
            Username = GetEnvironmentValue("DB_USER", DefaultUsername),
            Password = GetEnvironmentValue("DB_PASSWORD", DefaultPassword)
        };

        return builder.ConnectionString;
    }

    private static string GetEnvironmentValue(string p_key, string p_fallback)
    {
        string? value = Environment.GetEnvironmentVariable(p_key);
        return string.IsNullOrWhiteSpace(value) ? p_fallback : value;
    }
}
