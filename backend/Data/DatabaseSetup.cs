using BigCup.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace BigCup.Data
{
    /// <summary>
    /// Configuração central do banco: escolhe o provedor (Database:Provider = SqlServer | Sqlite),
    /// monta a connection string e registra o seed. Usado pela API (Program.cs) e pelo `dotnet ef` (DesignTimeFactories).
    /// </summary>
    public static class DatabaseSetup
    {
        public const string SqlServer = "SqlServer";
        public const string Sqlite = "Sqlite";

        public static string GetProvider(IConfiguration configuration)
        {
            var provider = configuration["Database:Provider"];
            if (string.Equals(provider, SqlServer, StringComparison.OrdinalIgnoreCase)) return SqlServer;
            if (string.Equals(provider, Sqlite, StringComparison.OrdinalIgnoreCase)) return Sqlite;

            throw new InvalidOperationException(
                $"Database:Provider inválido ('{provider}'). Use '{SqlServer}' ou '{Sqlite}' no appsettings " +
                "ou na variável de ambiente Database__Provider.");
        }

        public static void AddBigCupDatabase(this IServiceCollection services, IConfiguration configuration, string contentRoot)
        {
            var provider = GetProvider(configuration);
            var connectionString = GetConnectionString(configuration, provider, contentRoot);

            if (provider == SqlServer)
            {
                services.AddDbContext<AppDbContext, SqlServerAppDbContext>(o => ConfigureSqlServer(o, connectionString));
            }
            else
            {
                services.AddDbContext<AppDbContext, SqliteAppDbContext>(o => ConfigureSqlite(o, connectionString));
            }
        }

        public static string GetConnectionString(IConfiguration configuration, string provider, string contentRoot)
        {
            var connectionString = configuration.GetConnectionString(provider);
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException($"ConnectionStrings:{provider} não configurada.");
            }

            if (provider == Sqlite)
            {
                // Caminho relativo é resolvido a partir da pasta do backend, não do diretório de onde a API foi iniciada.
                var csb = new SqliteConnectionStringBuilder(connectionString);
                if (!Path.IsPathRooted(csb.DataSource) && csb.DataSource != ":memory:")
                {
                    csb.DataSource = Path.GetFullPath(Path.Combine(contentRoot, csb.DataSource));
                }
                Directory.CreateDirectory(Path.GetDirectoryName(csb.DataSource)!);
                connectionString = csb.ToString();
            }

            return connectionString;
        }

        public static void ConfigureSqlServer(DbContextOptionsBuilder options, string connectionString) =>
            options
                .UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())
                .UseSeeding((ctx, _) => SeedData.Seed(ctx))
                .UseAsyncSeeding((ctx, _, ct) => SeedData.SeedAsync(ctx, ct));

        public static void ConfigureSqlite(DbContextOptionsBuilder options, string connectionString) =>
            options
                .UseSqlite(connectionString)
                .UseSeeding((ctx, _) => SeedData.Seed(ctx))
                .UseAsyncSeeding((ctx, _, ct) => SeedData.SeedAsync(ctx, ct));
    }
}
