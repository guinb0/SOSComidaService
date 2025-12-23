using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOSComida.Migrations
{
    /// <inheritdoc />
    public partial class AddImagensCampanha : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Imagens",
                table: "tb_campanhas",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Imagens",
                table: "tb_campanhas");
        }
    }
}
