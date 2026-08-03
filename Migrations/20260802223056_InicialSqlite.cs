using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOSComida.Migrations
{
    /// <inheritdoc />
    public partial class InicialSqlite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_regioes_administrativas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Sigla = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    Cidade = table.Column<string>(type: "TEXT", maxLength: 200, nullable: true),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_regioes_administrativas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nome = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Senha = table.Column<string>(type: "TEXT", nullable: false),
                    Telefone = table.Column<string>(type: "TEXT", nullable: false),
                    Endereco = table.Column<string>(type: "TEXT", nullable: false),
                    Cidade = table.Column<string>(type: "TEXT", nullable: false),
                    Cpf = table.Column<string>(type: "TEXT", nullable: true),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    SenhaTemporaria = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FotoUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Cnpj = table.Column<string>(type: "TEXT", maxLength: 18, nullable: true),
                    RegiaoAdministrativaId = table.Column<int>(type: "INTEGER", nullable: true),
                    NomeInstituicao = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    DescricaoInstituicao = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: true),
                    StatusAprovacao = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DataAprovacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    AprovadoPorId = table.Column<int>(type: "INTEGER", nullable: true),
                    MotivoRejeicao = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_usuarios_tb_regioes_administrativas_RegiaoAdministrativaId",
                        column: x => x.RegiaoAdministrativaId,
                        principalTable: "tb_regioes_administrativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_usuarios_tb_usuarios_AprovadoPorId",
                        column: x => x.AprovadoPorId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "tb_acoes_moderacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TipoAcao = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Motivo = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true),
                    DataAcao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    TipoItem = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ItemId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModeradorId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_acoes_moderacao", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_acoes_moderacao_tb_usuarios_ModeradorId",
                        column: x => x.ModeradorId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_campanhas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    ImagemUrl = table.Column<string>(type: "TEXT", nullable: true),
                    Imagens = table.Column<string>(type: "TEXT", nullable: true),
                    Localizacao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    MetaArrecadacao = table.Column<double>(type: "REAL", nullable: false),
                    ValorArrecadado = table.Column<double>(type: "REAL", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataFim = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Ativa = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    InstituicaoId = table.Column<int>(type: "INTEGER", nullable: true),
                    StatusDelegacao = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DataDelegacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    RegiaoId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_campanhas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_campanhas_tb_regioes_administrativas_RegiaoId",
                        column: x => x.RegiaoId,
                        principalTable: "tb_regioes_administrativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_campanhas_tb_usuarios_InstituicaoId",
                        column: x => x.InstituicaoId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_campanhas_tb_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_denuncias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Motivo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ParecerModerador = table.Column<string>(type: "TEXT", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAnalise = table.Column<DateTime>(type: "TEXT", nullable: true),
                    TipoDenunciado = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DenunciadoId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModeradorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_denuncias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_denuncias_tb_usuarios_ModeradorId",
                        column: x => x.ModeradorId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_denuncias_tb_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_moderador_regioes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ModeradorId = table.Column<int>(type: "INTEGER", nullable: false),
                    RegiaoId = table.Column<int>(type: "INTEGER", nullable: false),
                    DataAtribuicao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ativo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_moderador_regioes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_moderador_regioes_tb_regioes_administrativas_RegiaoId",
                        column: x => x.RegiaoId,
                        principalTable: "tb_regioes_administrativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_moderador_regioes_tb_usuarios_ModeradorId",
                        column: x => x.ModeradorId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_avisos_campanha",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CampanhaId = table.Column<int>(type: "INTEGER", nullable: false),
                    ModeradorId = table.Column<int>(type: "INTEGER", nullable: false),
                    DestinatarioId = table.Column<int>(type: "INTEGER", nullable: true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Mensagem = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Lido = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataLeitura = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_avisos_campanha", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_avisos_campanha_tb_campanhas_CampanhaId",
                        column: x => x.CampanhaId,
                        principalTable: "tb_campanhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_avisos_campanha_tb_usuarios_DestinatarioId",
                        column: x => x.DestinatarioId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_avisos_campanha_tb_usuarios_ModeradorId",
                        column: x => x.ModeradorId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_doacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Valor = table.Column<double>(type: "REAL", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ComprovanteUrl = table.Column<string>(type: "TEXT", nullable: true),
                    DataDoacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataConfirmacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    CampanhaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_doacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_doacoes_tb_campanhas_CampanhaId",
                        column: x => x.CampanhaId,
                        principalTable: "tb_campanhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_tb_doacoes_tb_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_mensagens_chat",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CampanhaId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Conteudo = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataEdicao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Deletada = table.Column<bool>(type: "INTEGER", nullable: false),
                    Fixada = table.Column<bool>(type: "INTEGER", nullable: false),
                    FixadaPorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_mensagens_chat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_mensagens_chat_tb_campanhas_CampanhaId",
                        column: x => x.CampanhaId,
                        principalTable: "tb_campanhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_mensagens_chat_tb_usuarios_FixadaPorId",
                        column: x => x.FixadaPorId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_mensagens_chat_tb_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_notas_fiscais",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImagemUrl = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    TextoOcr = table.Column<string>(type: "TEXT", maxLength: 10000, nullable: true),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    ValorTotal = table.Column<decimal>(type: "TEXT", precision: 12, scale: 2, nullable: true),
                    CnpjEmitente = table.Column<string>(type: "TEXT", maxLength: 18, nullable: true),
                    NomeEmitente = table.Column<string>(type: "TEXT", maxLength: 300, nullable: true),
                    NumeroNota = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DataEmissao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ChaveAcesso = table.Column<string>(type: "TEXT", maxLength: 44, nullable: true),
                    ConfiancaOcr = table.Column<float>(type: "REAL", nullable: true),
                    Revisado = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataRevisao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    CampanhaId = table.Column<int>(type: "INTEGER", nullable: false),
                    EnviadoPorId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_notas_fiscais", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_notas_fiscais_tb_campanhas_CampanhaId",
                        column: x => x.CampanhaId,
                        principalTable: "tb_campanhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_notas_fiscais_tb_usuarios_EnviadoPorId",
                        column: x => x.EnviadoPorId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_notificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Mensagem = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Lida = table.Column<bool>(type: "INTEGER", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CampanhaId = table.Column<int>(type: "INTEGER", nullable: true),
                    StatusDelegacao = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_notificacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_notificacoes_tb_campanhas_CampanhaId",
                        column: x => x.CampanhaId,
                        principalTable: "tb_campanhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_notificacoes_tb_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "tb_participantes_campanha",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CampanhaId = table.Column<int>(type: "INTEGER", nullable: false),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    DataEntrada = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataSaida = table.Column<DateTime>(type: "TEXT", nullable: true),
                    MotivoSaida = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                    RemovidoPorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_participantes_campanha", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_participantes_campanha_tb_campanhas_CampanhaId",
                        column: x => x.CampanhaId,
                        principalTable: "tb_campanhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_tb_participantes_campanha_tb_usuarios_RemovidoPorId",
                        column: x => x.RemovidoPorId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_participantes_campanha_tb_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "tb_pedidos_ajuda",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Titulo = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    Localizacao = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    Urgencia = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    QuantidadePessoas = table.Column<int>(type: "INTEGER", nullable: false),
                    Observacoes = table.Column<string>(type: "TEXT", nullable: true),
                    Telefone = table.Column<string>(type: "TEXT", maxLength: 20, nullable: true),
                    TipoAjuda = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DataAtendimento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    UsuarioId = table.Column<int>(type: "INTEGER", nullable: false),
                    CampanhaId = table.Column<int>(type: "INTEGER", nullable: true),
                    RegiaoId = table.Column<int>(type: "INTEGER", nullable: true),
                    AtendidoPorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_pedidos_ajuda", x => x.Id);
                    table.ForeignKey(
                        name: "FK_tb_pedidos_ajuda_tb_campanhas_CampanhaId",
                        column: x => x.CampanhaId,
                        principalTable: "tb_campanhas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_pedidos_ajuda_tb_regioes_administrativas_RegiaoId",
                        column: x => x.RegiaoId,
                        principalTable: "tb_regioes_administrativas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_pedidos_ajuda_tb_usuarios_AtendidoPorId",
                        column: x => x.AtendidoPorId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_tb_pedidos_ajuda_tb_usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "tb_usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_tb_acoes_moderacao_ModeradorId",
                table: "tb_acoes_moderacao",
                column: "ModeradorId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_avisos_campanha_CampanhaId",
                table: "tb_avisos_campanha",
                column: "CampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_avisos_campanha_DestinatarioId",
                table: "tb_avisos_campanha",
                column: "DestinatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_avisos_campanha_ModeradorId",
                table: "tb_avisos_campanha",
                column: "ModeradorId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_campanhas_InstituicaoId",
                table: "tb_campanhas",
                column: "InstituicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_campanhas_RegiaoId",
                table: "tb_campanhas",
                column: "RegiaoId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_campanhas_UsuarioId",
                table: "tb_campanhas",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_denuncias_ModeradorId",
                table: "tb_denuncias",
                column: "ModeradorId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_denuncias_UsuarioId",
                table: "tb_denuncias",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_doacoes_CampanhaId",
                table: "tb_doacoes",
                column: "CampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_doacoes_UsuarioId",
                table: "tb_doacoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mensagens_chat_CampanhaId",
                table: "tb_mensagens_chat",
                column: "CampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mensagens_chat_FixadaPorId",
                table: "tb_mensagens_chat",
                column: "FixadaPorId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_mensagens_chat_UsuarioId",
                table: "tb_mensagens_chat",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_moderador_regioes_ModeradorId_RegiaoId",
                table: "tb_moderador_regioes",
                columns: new[] { "ModeradorId", "RegiaoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_moderador_regioes_RegiaoId",
                table: "tb_moderador_regioes",
                column: "RegiaoId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_notas_fiscais_CampanhaId",
                table: "tb_notas_fiscais",
                column: "CampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_notas_fiscais_EnviadoPorId",
                table: "tb_notas_fiscais",
                column: "EnviadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_notificacoes_CampanhaId",
                table: "tb_notificacoes",
                column: "CampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_notificacoes_UsuarioId",
                table: "tb_notificacoes",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_participantes_campanha_CampanhaId_UsuarioId",
                table: "tb_participantes_campanha",
                columns: new[] { "CampanhaId", "UsuarioId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_participantes_campanha_RemovidoPorId",
                table: "tb_participantes_campanha",
                column: "RemovidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_participantes_campanha_UsuarioId",
                table: "tb_participantes_campanha",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_pedidos_ajuda_AtendidoPorId",
                table: "tb_pedidos_ajuda",
                column: "AtendidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_pedidos_ajuda_CampanhaId",
                table: "tb_pedidos_ajuda",
                column: "CampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_pedidos_ajuda_RegiaoId",
                table: "tb_pedidos_ajuda",
                column: "RegiaoId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_pedidos_ajuda_UsuarioId",
                table: "tb_pedidos_ajuda",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_regioes_administrativas_Sigla",
                table: "tb_regioes_administrativas",
                column: "Sigla",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_usuarios_AprovadoPorId",
                table: "tb_usuarios",
                column: "AprovadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_usuarios_Email",
                table: "tb_usuarios",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_usuarios_RegiaoAdministrativaId",
                table: "tb_usuarios",
                column: "RegiaoAdministrativaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_acoes_moderacao");

            migrationBuilder.DropTable(
                name: "tb_avisos_campanha");

            migrationBuilder.DropTable(
                name: "tb_denuncias");

            migrationBuilder.DropTable(
                name: "tb_doacoes");

            migrationBuilder.DropTable(
                name: "tb_mensagens_chat");

            migrationBuilder.DropTable(
                name: "tb_moderador_regioes");

            migrationBuilder.DropTable(
                name: "tb_notas_fiscais");

            migrationBuilder.DropTable(
                name: "tb_notificacoes");

            migrationBuilder.DropTable(
                name: "tb_participantes_campanha");

            migrationBuilder.DropTable(
                name: "tb_pedidos_ajuda");

            migrationBuilder.DropTable(
                name: "tb_campanhas");

            migrationBuilder.DropTable(
                name: "tb_usuarios");

            migrationBuilder.DropTable(
                name: "tb_regioes_administrativas");
        }
    }
}
