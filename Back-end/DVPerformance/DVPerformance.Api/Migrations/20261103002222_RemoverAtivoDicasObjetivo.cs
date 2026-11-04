using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DVPerformance.Api.Migrations
{
    /// <inheritdoc />
    public partial class RemoverAtivoDicasObjetivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_dicas_objetivo_objetivo_ativo",
                table: "dicas_objetivo");

            migrationBuilder.DropColumn(
                name: "ativo",
                table: "dicas_objetivo");

            migrationBuilder.CreateIndex(
                name: "IX_dicas_objetivo_objetivo",
                table: "dicas_objetivo",
                column: "objetivo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_dicas_objetivo_objetivo",
                table: "dicas_objetivo");

            migrationBuilder.AddColumn<bool>(
                name: "ativo",
                table: "dicas_objetivo",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 1,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 2,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 3,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 4,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 5,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 6,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 7,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 8,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 9,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 10,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 11,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 12,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 13,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 14,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 15,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 16,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 17,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 18,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 19,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 20,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 21,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 22,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 23,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 24,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 25,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 26,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 27,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 28,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 29,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 30,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 31,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 32,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 33,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 34,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 35,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 36,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 37,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 38,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 39,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 40,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 41,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 42,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 43,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 44,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 45,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 46,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 47,
                column: "ativo",
                value: true);

            migrationBuilder.UpdateData(
                table: "dicas_objetivo",
                keyColumn: "id",
                keyValue: 48,
                column: "ativo",
                value: true);

            migrationBuilder.CreateIndex(
                name: "IX_dicas_objetivo_objetivo_ativo",
                table: "dicas_objetivo",
                columns: new[] { "objetivo", "ativo" });
        }
    }
}
