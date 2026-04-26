using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TradingAlpha.Infrastructure.Data;

/// <summary>
/// Design-time factory for AppDbContext.
/// 
/// Used ONLY by EF Core CLI tools (dotnet ef migrations, dotnet ef database update).
/// At runtime, the normal DI container creates AppDbContext via AddDbContext in
/// DependencyInjection.cs — this factory is never called during app execution.
/// 
/// The factory reads the connection string from Api's appsettings.json
/// so migrations use the same database as the running application.
/// </summary>
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Build configuration by reading appsettings.json from the Api project
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(),
                "..", "TradingAlpha.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}