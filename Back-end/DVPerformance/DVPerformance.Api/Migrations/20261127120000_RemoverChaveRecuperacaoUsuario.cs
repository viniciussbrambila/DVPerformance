using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DVPerformance.Api.Migrations;

public partial class RemoverChaveRecuperacaoUsuario : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(name: "chave_recuperacao_cifrada", table: "usuarios");
        migrationBuilder.DropColumn(name: "chave_recuperacao_hash", table: "usuarios");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "chave_recuperacao_cifrada", table: "usuarios",
            type: "character varying(200)", maxLength: 200, nullable: true);
        migrationBuilder.AddColumn<string>(
            name: "chave_recuperacao_hash", table: "usuarios",
            type: "character varying(100)", maxLength: 100, nullable: true);
    }
}
