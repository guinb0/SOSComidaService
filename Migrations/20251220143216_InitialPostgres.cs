using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SOSComida.Migrations
{
    /// <inheritdoc />
    public partial class InitialPostgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tb_usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefone = table.Column<string>(type: "text", nullable: false),
                    Endereco = table.Column<string>(type: "text", nullable: false),
                    Cidade = table.Column<string>(type: "text", nullable: false),
                    Cpf = table.Column<string>(type: "text", nullable: true),
                    Tipo = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_acoes_moderacao",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TipoAcao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    DataAcao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoItem = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    ModeradorId = table.Column<int>(type: "integer", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ImagemUrl = table.Column<string>(type: "text", nullable: true),
                    Localizacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    MetaArrecadacao = table.Column<double>(type: "double precision", nullable: false),
                    ValorArrecadado = table.Column<double>(type: "double precision", nullable: false),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataFim = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_campanhas", x => x.Id);
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Motivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ParecerModerador = table.Column<string>(type: "text", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAnalise = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TipoDenunciado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DenunciadoId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    ModeradorId = table.Column<int>(type: "integer", nullable: true)
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
                name: "tb_avisos_campanha",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampanhaId = table.Column<int>(type: "integer", nullable: false),
                    ModeradorId = table.Column<int>(type: "integer", nullable: false),
                    DestinatarioId = table.Column<int>(type: "integer", nullable: true),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mensagem = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Lido = table.Column<bool>(type: "boolean", nullable: false),
                    DataLeitura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Valor = table.Column<double>(type: "double precision", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ComprovanteUrl = table.Column<string>(type: "text", nullable: true),
                    DataDoacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataConfirmacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    CampanhaId = table.Column<int>(type: "integer", nullable: false)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampanhaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Conteudo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataEdicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Deletada = table.Column<bool>(type: "boolean", nullable: false),
                    Fixada = table.Column<bool>(type: "boolean", nullable: false),
                    FixadaPorId = table.Column<int>(type: "integer", nullable: true)
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
                name: "tb_participantes_campanha",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CampanhaId = table.Column<int>(type: "integer", nullable: false),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DataEntrada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataSaida = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    MotivoSaida = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RemovidoPorId = table.Column<int>(type: "integer", nullable: true)
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
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Descricao = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Localizacao = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Urgencia = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    QuantidadePessoas = table.Column<int>(type: "integer", nullable: false),
                    Observacoes = table.Column<string>(type: "text", nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataAtendimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    CampanhaId = table.Column<int>(type: "integer", nullable: true)
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
                name: "IX_tb_pedidos_ajuda_CampanhaId",
                table: "tb_pedidos_ajuda",
                column: "CampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_pedidos_ajuda_UsuarioId",
                table: "tb_pedidos_ajuda",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_usuarios_Email",
                table: "tb_usuarios",
                column: "Email",
                unique: true);
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
                name: "tb_participantes_campanha");

            migrationBuilder.DropTable(
                name: "tb_pedidos_ajuda");

            migrationBuilder.DropTable(
                name: "tb_campanhas");

            migrationBuilder.DropTable(
                name: "tb_usuarios");
        }
    }
}
