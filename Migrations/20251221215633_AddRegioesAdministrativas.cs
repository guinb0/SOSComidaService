using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SOSComida.Migrations
{
    /// <inheritdoc />
    public partial class AddRegioesAdministrativas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "RegiaoId",
                table: "tb_campanhas",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "tb_regioes_administrativas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Sigla = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Cidade = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Ativa = table.Column<bool>(type: "boolean", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_regioes_administrativas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_moderador_regioes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ModeradorId = table.Column<int>(type: "integer", nullable: false),
                    RegiaoId = table.Column<int>(type: "integer", nullable: false),
                    DataAtribuicao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Ativo = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateIndex(
                name: "IX_tb_campanhas_RegiaoId",
                table: "tb_campanhas",
                column: "RegiaoId");

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
                name: "IX_tb_regioes_administrativas_Sigla",
                table: "tb_regioes_administrativas",
                column: "Sigla",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_campanhas_tb_regioes_administrativas_RegiaoId",
                table: "tb_campanhas",
                column: "RegiaoId",
                principalTable: "tb_regioes_administrativas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_campanhas_tb_regioes_administrativas_RegiaoId",
                table: "tb_campanhas");

            migrationBuilder.DropTable(
                name: "tb_moderador_regioes");

            migrationBuilder.DropTable(
                name: "tb_regioes_administrativas");

            migrationBuilder.DropIndex(
                name: "IX_tb_campanhas_RegiaoId",
                table: "tb_campanhas");

            migrationBuilder.DropColumn(
                name: "RegiaoId",
                table: "tb_campanhas");
        }
    }
}
