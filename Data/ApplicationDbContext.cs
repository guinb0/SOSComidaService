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
    public DbSet<PedidoAjuda> PedidosAjuda { get; set; }
    public DbSet<Doacao> Doacoes { get; set; }
    public DbSet<Denuncia> Denuncias { get; set; }
    public DbSet<AcaoModeracao> AcoesModeracao { get; set; }
    public DbSet<ParticipanteCampanha> ParticipantesCampanha { get; set; }
    public DbSet<MensagemChat> MensagensChat { get; set; }
    public DbSet<AvisoCampanha> AvisosCampanha { get; set; }

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
            entity.Property(e => e.MetaArrecadacao).HasConversion<double>();
            entity.Property(e => e.ValorArrecadado).HasConversion<double>();
            entity.Property(e => e.Status).HasMaxLength(50);
            
            // Relacionamento
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurar PedidoAjuda
        modelBuilder.Entity<PedidoAjuda>(entity =>
        {
            entity.ToTable("tb_pedidos_ajuda");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(300);
            entity.Property(e => e.Descricao).HasMaxLength(2000);
            entity.Property(e => e.Localizacao).HasMaxLength(200);
            entity.Property(e => e.Urgencia).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(50);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Campanha)
                  .WithMany()
                  .HasForeignKey(e => e.CampanhaId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configurar Doacao
        modelBuilder.Entity<Doacao>(entity =>
        {
            entity.ToTable("tb_doacoes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Valor).HasConversion<double>();
            entity.Property(e => e.Tipo).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.Descricao).HasMaxLength(500);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Campanha)
                  .WithMany()
                  .HasForeignKey(e => e.CampanhaId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurar Denuncia
        modelBuilder.Entity<Denuncia>(entity =>
        {
            entity.ToTable("tb_denuncias");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Motivo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descricao).HasMaxLength(2000);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.TipoDenunciado).HasMaxLength(50);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Moderador)
                  .WithMany()
                  .HasForeignKey(e => e.ModeradorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configurar AcaoModeracao
        modelBuilder.Entity<AcaoModeracao>(entity =>
        {
            entity.ToTable("tb_acoes_moderacao");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TipoAcao).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Motivo).IsRequired().HasMaxLength(500);
            entity.Property(e => e.TipoItem).HasMaxLength(50);

            entity.HasOne(e => e.Moderador)
                  .WithMany()
                  .HasForeignKey(e => e.ModeradorId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // Configurar ParticipanteCampanha
        modelBuilder.Entity<ParticipanteCampanha>(entity =>
        {
            entity.ToTable("tb_participantes_campanha");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipo).HasMaxLength(50);
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.MotivoSaida).HasMaxLength(500);

            entity.HasOne(e => e.Campanha)
                  .WithMany()
                  .HasForeignKey(e => e.CampanhaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.RemovidoPor)
                  .WithMany()
                  .HasForeignKey(e => e.RemovidoPorId)
                  .OnDelete(DeleteBehavior.SetNull);

            entity.HasIndex(e => new { e.CampanhaId, e.UsuarioId }).IsUnique();
        });

        // Configurar MensagemChat
        modelBuilder.Entity<MensagemChat>(entity =>
        {
            entity.ToTable("tb_mensagens_chat");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Conteudo).IsRequired().HasMaxLength(2000);

            entity.HasOne(e => e.Campanha)
                  .WithMany()
                  .HasForeignKey(e => e.CampanhaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.FixadaPor)
                  .WithMany()
                  .HasForeignKey(e => e.FixadaPorId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configurar AvisoCampanha
        modelBuilder.Entity<AvisoCampanha>(entity =>
        {
            entity.ToTable("tb_avisos_campanha");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Mensagem).IsRequired().HasMaxLength(2000);
            entity.Property(e => e.Tipo).HasMaxLength(50);

            entity.HasOne(e => e.Campanha)
                  .WithMany()
                  .HasForeignKey(e => e.CampanhaId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Moderador)
                  .WithMany()
                  .HasForeignKey(e => e.ModeradorId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Destinatario)
                  .WithMany()
                  .HasForeignKey(e => e.DestinatarioId)
                  .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
