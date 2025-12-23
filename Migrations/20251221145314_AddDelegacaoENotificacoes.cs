using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SOSComida.Migrations
{
    /// <inheritdoc />
    public partial class AddDelegacaoENotificacoes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataDelegacao",
                table: "tb_campanhas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstituicaoId",
                table: "tb_campanhas",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusDelegacao",
                table: "tb_campanhas",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tb_notificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mensagem = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Lida = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CampanhaId = table.Column<int>(type: "integer", nullable: true),
                    StatusDelegacao = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
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

            migrationBuilder.CreateIndex(
                name: "IX_tb_campanhas_InstituicaoId",
                table: "tb_campanhas",
                column: "InstituicaoId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_notificacoes_CampanhaId",
                table: "tb_notificacoes",
                column: "CampanhaId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_notificacoes_UsuarioId",
                table: "tb_notificacoes",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_campanhas_tb_usuarios_InstituicaoId",
                table: "tb_campanhas",
                column: "InstituicaoId",
                principalTable: "tb_usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_campanhas_tb_usuarios_InstituicaoId",
                table: "tb_campanhas");

            migrationBuilder.DropTable(
                name: "tb_notificacoes");

            migrationBuilder.DropIndex(
                name: "IX_tb_campanhas_InstituicaoId",
                table: "tb_campanhas");

            migrationBuilder.DropColumn(
                name: "DataDelegacao",
                table: "tb_campanhas");

            migrationBuilder.DropColumn(
                name: "InstituicaoId",
                table: "tb_campanhas");

            migrationBuilder.DropColumn(
                name: "StatusDelegacao",
                table: "tb_campanhas");
        }
    }
}
