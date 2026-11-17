using BigCup.Domain;
using Microsoft.EntityFrameworkCore;

namespace BigCup.Data
{
    /// <summary>Contexto para SQL Server. Migrations em Migrations/SqlServer.</summary>
    public sealed class SqlServerAppDbContext : AppDbContext
    {
        public SqlServerAppDbContext(DbContextOptions<SqlServerAppDbContext> options)
            : base(options)
        {
        }
    }

    /// <summary>Contexto para SQLite. Migrations em Migrations/Sqlite.</summary>
    public sealed class SqliteAppDbContext : AppDbContext
    {
        public SqliteAppDbContext(DbContextOptions<SqliteAppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // SQLite diferencia maiúsculas por padrão; NOCASE deixa "Tequila" e "tequila" iguais, como no SQL Server.
            builder.Entity<Ingrediente>().Property(i => i.Nome).UseCollation("NOCASE");

            // SQLite guarda decimal como TEXT (comparações e o CHECK "Quantidade > 0" não funcionariam); REAL resolve.
            builder.Entity<DrinkIngrediente>().Property(di => di.Quantidade).HasConversion<double>();
        }
    }
}
