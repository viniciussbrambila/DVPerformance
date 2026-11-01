using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DVPerformance.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarStatusTreino : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "treinos",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Rascunho");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "status",
                table: "treinos");
        }
    }
}
