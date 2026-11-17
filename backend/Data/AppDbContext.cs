using BigCup.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BigCup.Data
{
    /// <summary>
    /// Modelo do banco, comum aos dois provedores. Não é instanciado diretamente:
    /// use <see cref="SqlServerAppDbContext"/> ou <see cref="SqliteAppDbContext"/>, cada um com suas migrations.
    /// Os controllers injetam <see cref="AppDbContext"/>; o provedor é escolhido por configuração (Database:Provider).
    /// </summary>
    public abstract class AppDbContext : IdentityDbContext<Usuario>
    {
        protected AppDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<Drink> Drinks => Set<Drink>();
        public DbSet<Ingrediente> Ingredientes => Set<Ingrediente>();
        public DbSet<DrinkIngrediente> DrinkIngredientes => Set<DrinkIngrediente>();
        public DbSet<PassoPreparo> PassosPreparo => Set<PassoPreparo>();
        public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();
        public DbSet<Recipiente> Recipientes => Set<Recipiente>();

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            base.ConfigureConventions(configurationBuilder);

            // Todas as datas são gravadas em UTC e voltam marcadas como UTC (JSON com "Z").
            configurationBuilder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); // tabelas do Identity

            builder.Entity<Drink>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_Drinks_TempoPreparo", "TempoPreparoMinutos >= 0"));
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Descricao).HasMaxLength(500);

                entity.HasOne(d => d.Usuario)
                    .WithMany(u => u.Drinks)
                    .HasForeignKey(d => d.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Recipiente)
                    .WithMany(r => r.Drinks)
                    .HasForeignKey(d => d.RecipienteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<Ingrediente>(entity =>
            {
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
                entity.HasIndex(e => e.Nome).IsUnique();
            });

            builder.Entity<DrinkIngrediente>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_DrinkIngredientes_Quantidade", "Quantidade > 0"));
                entity.HasKey(di => new { di.DrinkId, di.IngredienteId });
                entity.Property(di => di.Quantidade).HasPrecision(12, 3);
                entity.Property(di => di.Unidade).HasMaxLength(50);
                entity.HasOne(di => di.Drink)
                    .WithMany(d => d.DrinkIngredientes)
                    .HasForeignKey(di => di.DrinkId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(di => di.Ingrediente)
                    .WithMany(i => i.DrinkIngredientes)
                    .HasForeignKey(di => di.IngredienteId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<PassoPreparo>(entity =>
            {
                entity.Property(e => e.Instrucao).IsRequired().HasMaxLength(1000);
                entity.HasIndex(p => new { p.DrinkId, p.Ordem }).IsUnique();
                entity.HasOne(p => p.Drink)
                    .WithMany(d => d.PassosPreparo)
                    .HasForeignKey(p => p.DrinkId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Avaliacao>(entity =>
            {
                entity.ToTable(t => t.HasCheckConstraint("CK_Avaliacoes_Nota", "Nota BETWEEN 1 AND 5"));
                entity.Property(e => e.Comentario).HasMaxLength(500);
                entity.HasIndex(a => new { a.DrinkId, a.UsuarioId }).IsUnique();
                entity.HasOne(a => a.Drink)
                    .WithMany(d => d.Avaliacoes)
                    .HasForeignKey(a => a.DrinkId)
                    .OnDelete(DeleteBehavior.Cascade);
                // ClientCascade: o SQL Server não aceita dois caminhos de cascata (Usuario -> Drink -> Avaliacao e
                // Usuario -> Avaliacao). O EF apaga as avaliações ao remover o usuário pela aplicação.
                entity.HasOne(a => a.Usuario)
                    .WithMany(u => u.Avaliacoes)
                    .HasForeignKey(a => a.UsuarioId)
                    .OnDelete(DeleteBehavior.ClientCascade);
            });

            builder.Entity<Recipiente>(entity =>
            {
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(100);
                entity.HasData(
                    new Recipiente { Id = 1, Nome = "Shot", CapacidadeMl = 50 },
                    new Recipiente { Id = 2, Nome = "Copo pequeno", CapacidadeMl = 300 },
                    new Recipiente { Id = 3, Nome = "Copo médio", CapacidadeMl = 500 },
                    new Recipiente { Id = 4, Nome = "Copo grande", CapacidadeMl = 700 },
                    new Recipiente { Id = 5, Nome = "Garrafa", CapacidadeMl = 2000 },
                    new Recipiente { Id = 6, Nome = "Galão", CapacidadeMl = 5000 });
            });
        }

        private sealed class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
        {
            public UtcDateTimeConverter()
                : base(
                    v => v.Kind == DateTimeKind.Utc ? v : v.ToUniversalTime(),
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
            {
            }
        }
    }
}
