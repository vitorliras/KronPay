using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Context;

public sealed class PostgresAppDbContextFactory
    : IDesignTimeDbContextFactory<PostgresAppDbContext>
{
    public PostgresAppDbContext CreateDbContext(string[] args)
    {
        var basePath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "..",
            "Api"
        );

        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddUserSecrets("kronpay-api-secrets")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<PostgresAppDbContext>();

        optionsBuilder.UseNpgsql(
            configuration.GetConnectionString("PostgresConnection")
        );

        return new PostgresAppDbContext(optionsBuilder.Options);
    }
}
