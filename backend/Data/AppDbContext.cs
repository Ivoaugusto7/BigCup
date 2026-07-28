using BigCup.Domain;
using Microsoft.EntityFrameworkCore;

namespace BigCup.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Drink> Drinks => Set<Drink>();
        public DbSet<Ingrediente> Ingredientes => Set<Ingrediente>();
        public DbSet<DrinkIngrediente> DrinkIngredientes => Set<DrinkIngrediente>();
        public DbSet<PassoPreparo> PassosPreparo => Set<PassoPreparo>();
        public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Drink>(entity =>
            {
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Descricao).HasMaxLength(500);
            });

            builder.Entity<Ingrediente>(entity =>
            {
                entity.Property(e => e.Nome).IsRequired().HasMaxLength(150);
                entity.Property(e => e.Unidade).HasMaxLength(50);
            });

            builder.Entity<DrinkIngrediente>(entity =>
            {
                entity.HasKey(di => new { di.DrinkId, di.IngredienteId });
                entity.Property(di => di.Quantidade).HasPrecision(12, 3);
                entity.Property(di => di.Unidade).HasMaxLength(50);
                entity.HasOne(di => di.Drink)
                    .WithMany(d => d.DrinkIngredientes)
                    .HasForeignKey(di => di.DrinkId);
                entity.HasOne(di => di.Ingrediente)
                    .WithMany(i => i.DrinkIngredientes)
                    .HasForeignKey(di => di.IngredienteId);
            });

            builder.Entity<PassoPreparo>(entity =>
            {
                entity.Property(e => e.Instrucao).IsRequired().HasMaxLength(1000);
                entity.HasOne(p => p.Drink)
                    .WithMany(d => d.PassosPreparo)
                    .HasForeignKey(p => p.DrinkId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Avaliacao>(entity =>
            {
                entity.Property(e => e.Nota).IsRequired();
                entity.Property(e => e.Comentario).HasMaxLength(500);
                entity.Property(e => e.DataAvaliacao).IsRequired();
                entity.HasOne(a => a.Drink)
                    .WithMany(d => d.Avaliacoes)
                    .HasForeignKey(a => a.DrinkId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
