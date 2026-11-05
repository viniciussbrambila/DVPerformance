using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DVPerformance.Api.Migrations
{
    /// <inheritdoc />
    public partial class ProvisionarCatalogoPadraoPorPersonal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_maquinas_usuario_id",
                table: "maquinas");

            migrationBuilder.DropIndex(
                name: "IX_exercicios_usuario_id",
                table: "exercicios");

            migrationBuilder.AddColumn<bool>(
                name: "catalogo_inicializado",
                table: "usuarios",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_maquinas_usuario_id_ativo",
                table: "maquinas",
                columns: new[] { "usuario_id", "ativo" });

            migrationBuilder.CreateIndex(
                name: "IX_exercicios_usuario_id_ativo",
                table: "exercicios",
                columns: new[] { "usuario_id", "ativo" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_maquinas_usuario_id_ativo",
                table: "maquinas");

            migrationBuilder.DropIndex(
                name: "IX_exercicios_usuario_id_ativo",
                table: "exercicios");

            migrationBuilder.DropColumn(
                name: "catalogo_inicializado",
                table: "usuarios");

            migrationBuilder.CreateIndex(
                name: "IX_maquinas_usuario_id",
                table: "maquinas",
                column: "usuario_id");

            migrationBuilder.CreateIndex(
                name: "IX_exercicios_usuario_id",
                table: "exercicios",
                column: "usuario_id");
        }
    }
}
