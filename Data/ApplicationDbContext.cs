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
    public DbSet<Notificacao> Notificacoes { get; set; }
    public DbSet<RegiaoAdministrativa> RegioesAdministrativas { get; set; }
    public DbSet<ModeradorRegiao> ModeradorRegioes { get; set; }

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
            entity.Property(e => e.Cnpj).HasMaxLength(18);
            entity.Property(e => e.NomeInstituicao).HasMaxLength(300);
            entity.Property(e => e.DescricaoInstituicao).HasMaxLength(2000);
            entity.Property(e => e.StatusAprovacao).HasMaxLength(50);
            entity.Property(e => e.MotivoRejeicao).HasMaxLength(1000);
            
            // Relacionamento com Região Administrativa (para instituições)
            entity.HasOne(e => e.RegiaoAdministrativa)
                  .WithMany()
                  .HasForeignKey(e => e.RegiaoAdministrativaId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            // Relacionamento com quem aprovou
            entity.HasOne(e => e.AprovadoPor)
                  .WithMany()
                  .HasForeignKey(e => e.AprovadoPorId)
                  .OnDelete(DeleteBehavior.SetNull);
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
            entity.Property(e => e.StatusDelegacao).HasMaxLength(50);
            
            // Relacionamento com criador
            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
                  
            // Relacionamento com instituição delegada
            entity.HasOne(e => e.Instituicao)
                  .WithMany()
                  .HasForeignKey(e => e.InstituicaoId)
                  .OnDelete(DeleteBehavior.SetNull);
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
            entity.Property(e => e.TipoAjuda).HasMaxLength(50);
            entity.Property(e => e.Telefone).HasMaxLength(20);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Campanha)
                  .WithMany()
                  .HasForeignKey(e => e.CampanhaId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.Regiao)
                  .WithMany()
                  .HasForeignKey(e => e.RegiaoId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasOne(e => e.AtendidoPor)
                  .WithMany()
                  .HasForeignKey(e => e.AtendidoPorId)
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

        // Configurar Notificacao
        modelBuilder.Entity<Notificacao>(entity =>
        {
            entity.ToTable("tb_notificacoes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Tipo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Mensagem).HasMaxLength(2000);
            entity.Property(e => e.StatusDelegacao).HasMaxLength(50);

            entity.HasOne(e => e.Usuario)
                  .WithMany()
                  .HasForeignKey(e => e.UsuarioId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Campanha)
                  .WithMany()
                  .HasForeignKey(e => e.CampanhaId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Configurar RegiaoAdministrativa
        modelBuilder.Entity<RegiaoAdministrativa>(entity =>
        {
            entity.ToTable("tb_regioes_administrativas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Sigla).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Estado).HasMaxLength(50);
            entity.Property(e => e.Cidade).HasMaxLength(200);
            entity.HasIndex(e => e.Sigla).IsUnique();
        });

        // Configurar ModeradorRegiao (muitos-para-muitos)
        modelBuilder.Entity<ModeradorRegiao>(entity =>
        {
            entity.ToTable("tb_moderador_regioes");
            entity.HasKey(e => e.Id);

            entity.HasOne(e => e.Moderador)
                  .WithMany()
                  .HasForeignKey(e => e.ModeradorId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Regiao)
                  .WithMany(r => r.ModeradorRegioes)
                  .HasForeignKey(e => e.RegiaoId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => new { e.ModeradorId, e.RegiaoId }).IsUnique();
        });

        // Configurar relacionamento Campanha-Regiao
        modelBuilder.Entity<Campanha>()
            .HasOne(c => c.Regiao)
            .WithMany()
            .HasForeignKey(c => c.RegiaoId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
