using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOSComida.Migrations
{
    /// <inheritdoc />
    public partial class AddFotoUrlUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FotoUrl",
                table: "tb_usuarios",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FotoUrl",
                table: "tb_usuarios");
        }
    }
}
