using Microsoft.EntityFrameworkCore;
using SOSComida.Models;

namespace SOSComida.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Campanha> Campanhas { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar Usuario
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("tb_usuarios");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configurar Campanha
        modelBuilder.Entity<Campanha>(entity =>
        {
            entity.ToTable("tb_campanhas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Descricao).HasMaxLength(2000);
            entity.Property(e => e.Localizacao).HasMaxLength(200);
            entity.Property(e => e.MetaArrecadacao).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ValorArrecadado).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Status).HasMaxLength(50);
            
            // Relacionamento
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
