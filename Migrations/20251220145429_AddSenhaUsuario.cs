using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOSComida.Migrations
{
    /// <inheritdoc />
    public partial class AddSenhaUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataCriacao",
                table: "tb_usuarios",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Senha",
                table: "tb_usuarios",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "SenhaTemporaria",
                table: "tb_usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataCriacao",
                table: "tb_usuarios");

            migrationBuilder.DropColumn(
                name: "Senha",
                table: "tb_usuarios");

            migrationBuilder.DropColumn(
                name: "SenhaTemporaria",
                table: "tb_usuarios");
        }
    }
}
