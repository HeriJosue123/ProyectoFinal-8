using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContactManagerWeb.Migrations
{
    /// <inheritdoc />
    public partial class SincronizacionTablas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Emoji",
                table: "Categorias");
        }
    }
}
