using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzerothMemories.WebServer.Migrations
{
    /// <inheritdoc />
    public partial class CreateDatabase7 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX__Events_Uuid",
                table: "_Events");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX__Events_Uuid",
                table: "_Events",
                column: "Uuid",
                unique: true);
        }
    }
}
