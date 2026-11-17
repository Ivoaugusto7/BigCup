using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BigCup.Data
{
    // Usadas apenas pelo `dotnet ef` (migrations e database update). Leem appsettings.json + variáveis de ambiente,
    // então ConnectionStrings__SqlServer / ConnectionStrings__Sqlite permitem apontar para outro banco.

    public sealed class SqlServerDesignTimeFactory : IDesignTimeDbContextFactory<SqlServerAppDbContext>
    {
        public SqlServerAppDbContext CreateDbContext(string[] args)
        {
            var (configuration, contentRoot) = DesignTimeConfig.Load();
            var options = new DbContextOptionsBuilder<SqlServerAppDbContext>();
            DatabaseSetup.ConfigureSqlServer(options,
                DatabaseSetup.GetConnectionString(configuration, DatabaseSetup.SqlServer, contentRoot));
            return new SqlServerAppDbContext(options.Options);
        }
    }

    public sealed class SqliteDesignTimeFactory : IDesignTimeDbContextFactory<SqliteAppDbContext>
    {
        public SqliteAppDbContext CreateDbContext(string[] args)
        {
            var (configuration, contentRoot) = DesignTimeConfig.Load();
            var options = new DbContextOptionsBuilder<SqliteAppDbContext>();
            DatabaseSetup.ConfigureSqlite(options,
                DatabaseSetup.GetConnectionString(configuration, DatabaseSetup.Sqlite, contentRoot));
            return new SqliteAppDbContext(options.Options);
        }
    }

    internal static class DesignTimeConfig
    {
        public static (IConfiguration Configuration, string ContentRoot) Load()
        {
            // O `dotnet ef` executa com o diretório atual = pasta do projeto (backend/).
            var contentRoot = Directory.GetCurrentDirectory();
            var configuration = new ConfigurationBuilder()
                .SetBasePath(contentRoot)
                .AddJsonFile("appsettings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();
            return (configuration, contentRoot);
        }
    }
}
