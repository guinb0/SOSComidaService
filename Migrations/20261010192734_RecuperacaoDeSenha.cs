using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOSComida.Migrations
{
    /// <inheritdoc />
    public partial class RecuperacaoDeSenha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "SenhaRecuperacaoExpiraEm",
                table: "tb_usuarios",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SenhaRecuperacaoHash",
                table: "tb_usuarios",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SenhaRecuperacaoExpiraEm",
                table: "tb_usuarios");

            migrationBuilder.DropColumn(
                name: "SenhaRecuperacaoHash",
                table: "tb_usuarios");
        }
    }
}
