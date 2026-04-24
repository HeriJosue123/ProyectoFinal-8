using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactManagerWeb.Migrations
{
    public partial class AgregarColumnaEmojiReal : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Escribimos manualmente la instrucción para agregar la columna
            migrationBuilder.AddColumn<string>(
                name: "Emoji",
                table: "Categorias",
                type: "nvarchar(max)",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Instrucción para deshacer el cambio si fuera necesario
            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "Categorias");
        }
    }
}