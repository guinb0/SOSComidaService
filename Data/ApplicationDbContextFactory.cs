using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace SOSComida.Data;

/// <summary>
/// Garante que <c>dotnet ef</c> carrega os mesmos appsettings que a API (incl. appsettings.Development.local.json).
/// Se a rede não tiver IPv6, a conexão direta ao host <c>db.*.supabase.co</c> pode dar timeout — use a string
/// "Session pooler" (IPv4) copiada do painel Supabase → Connect, ou ative IPv6 na rede.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var projectDir = FindProjectDirectory();
        var config = new ConfigurationBuilder()
            .SetBasePath(projectDir)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddJsonFile("appsettings.Development.local.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Defina ConnectionStrings:DefaultConnection (appsettings ou appsettings.Development.local.json).");

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsql =>
        {
            // Evita NpgsqlExecutionStrategy + pooler Supavisor (ObjectDisposedException em migrations).
            npgsql.ExecutionStrategy(d => new NonRetryingExecutionStrategy(d));
        });

        return new ApplicationDbContext(optionsBuilder.Options);
    }

    private static string FindProjectDirectory()
    {
        var dir = new DirectoryInfo(Directory.GetCurrentDirectory());
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "SOSComida.csproj")))
                return dir.FullName;
            dir = dir.Parent;
        }

        var assemblyDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (!string.IsNullOrEmpty(assemblyDir))
        {
            var fromBin = new DirectoryInfo(assemblyDir);
            while (fromBin != null)
            {
                if (File.Exists(Path.Combine(fromBin.FullName, "SOSComida.csproj")))
                    return fromBin.FullName;
                fromBin = fromBin.Parent;
            }
        }

        return Directory.GetCurrentDirectory();
    }
}
