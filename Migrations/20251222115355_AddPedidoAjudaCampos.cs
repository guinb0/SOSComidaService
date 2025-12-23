using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SOSComida.Migrations
{
    /// <inheritdoc />
    public partial class AddPedidoAjudaCampos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AtendidoPorId",
                table: "tb_pedidos_ajuda",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RegiaoId",
                table: "tb_pedidos_ajuda",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Telefone",
                table: "tb_pedidos_ajuda",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TipoAjuda",
                table: "tb_pedidos_ajuda",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_tb_pedidos_ajuda_AtendidoPorId",
                table: "tb_pedidos_ajuda",
                column: "AtendidoPorId");

            migrationBuilder.CreateIndex(
                name: "IX_tb_pedidos_ajuda_RegiaoId",
                table: "tb_pedidos_ajuda",
                column: "RegiaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_tb_pedidos_ajuda_tb_regioes_administrativas_RegiaoId",
                table: "tb_pedidos_ajuda",
                column: "RegiaoId",
                principalTable: "tb_regioes_administrativas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_tb_pedidos_ajuda_tb_usuarios_AtendidoPorId",
                table: "tb_pedidos_ajuda",
                column: "AtendidoPorId",
                principalTable: "tb_usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tb_pedidos_ajuda_tb_regioes_administrativas_RegiaoId",
                table: "tb_pedidos_ajuda");

            migrationBuilder.DropForeignKey(
                name: "FK_tb_pedidos_ajuda_tb_usuarios_AtendidoPorId",
                table: "tb_pedidos_ajuda");

            migrationBuilder.DropIndex(
                name: "IX_tb_pedidos_ajuda_AtendidoPorId",
                table: "tb_pedidos_ajuda");

            migrationBuilder.DropIndex(
                name: "IX_tb_pedidos_ajuda_RegiaoId",
                table: "tb_pedidos_ajuda");

            migrationBuilder.DropColumn(
                name: "AtendidoPorId",
                table: "tb_pedidos_ajuda");

            migrationBuilder.DropColumn(
                name: "RegiaoId",
                table: "tb_pedidos_ajuda");

            migrationBuilder.DropColumn(
                name: "Telefone",
                table: "tb_pedidos_ajuda");

            migrationBuilder.DropColumn(
                name: "TipoAjuda",
                table: "tb_pedidos_ajuda");
        }
    }
}
