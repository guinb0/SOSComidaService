using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOSComida.Migrations
{
    /// <inheritdoc />
    public partial class AddInstituicaoCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AprovadoPorId",
                table: "tb_usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Cnpj",
                table: "tb_usuarios",
                type: "character varying(18)",
                maxLength: 18,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAprovacao",
                table: "tb_usuarios",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescricaoInstituicao",
                table: "tb_usuarios",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MotivoRejeicao",
                table: "tb_usuarios",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NomeInstituicao",
                table: "tb_usuarios",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegiaoAdministrativaId",
                table: "tb_usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusAprovacao",
                table: "tb_usuarios",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_tb_usuarios_AprovadoPorId",
                table: "tb_usuarios",
                column: "AprovadoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_usuarios_RegiaoAdministrativaId",
                table: "tb_usuarios",
                column: "RegiaoAdministrativaId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_usuarios_tb_regioes_administrativas_RegiaoAdministrativa~",
                table: "tb_usuarios",
                column: "RegiaoAdministrativaId",
                principalTable: "tb_regioes_administrativas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_usuarios_tb_usuarios_AprovadoPorId",
                table: "tb_usuarios",
                column: "AprovadoPorId",
                principalTable: "tb_usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_usuarios_tb_regioes_administrativas_RegiaoAdministrativa~",
                table: "tb_usuarios");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_usuarios_tb_usuarios_AprovadoPorId",
                table: "tb_usuarios");

            migrationBuilder.DropIndex(
                name: "IX_tb_usuarios_AprovadoPorId",
                table: "tb_usuarios");

            migrationBuilder.DropIndex(
                name: "IX_tb_usuarios_RegiaoAdministrativaId",
                table: "tb_usuarios");

            migrationBuilder.DropColumn(
                name: "AprovadoPorId",
                table: "tb_usuarios");

            migrationBuilder.DropColumn(
                name: "Cnpj",
                table: "tb_usuarios");

            migrationBuilder.DropColumn(
                name: "DataAprovacao",
                table: "tb_usuarios");

            migrationBuilder.DropColumn(
                name: "DescricaoInstituicao",
                table: "tb_usuarios");

            migrationBuilder.DropColumn(
                name: "MotivoRejeicao",
                table: "tb_usuarios");

            migrationBuilder.DropColumn(
                name: "NomeInstituicao",
                table: "tb_usuarios");

            migrationBuilder.DropColumn(
                name: "RegiaoAdministrativaId",
                table: "tb_usuarios");

            migrationBuilder.DropColumn(
                name: "StatusAprovacao",
                table: "tb_usuarios");
        }
    }
}
