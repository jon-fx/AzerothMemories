using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzerothMemories.WebServer.Migrations
{
    /// <inheritdoc />
    public partial class CreateDatabase3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserIdentities_Users_DbUser<string>Id",
                table: "UserIdentities");

            migrationBuilder.DropIndex(
                name: "IX_UserIdentities_DbUser<string>Id",
                table: "UserIdentities");

            migrationBuilder.DropColumn(
                name: "DbUser<string>Id",
                table: "UserIdentities");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "_Sessions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateIndex(
                name: "IX_UserIdentities_UserId",
                table: "UserIdentities",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserIdentities_Users_UserId",
                table: "UserIdentities",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserIdentities_Users_UserId",
                table: "UserIdentities");

            migrationBuilder.DropIndex(
                name: "IX_UserIdentities_UserId",
                table: "UserIdentities");

            migrationBuilder.AddColumn<string>(
                name: "DbUser<string>Id",
                table: "UserIdentities",
                type: "text",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "_Sessions",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserIdentities_DbUser<string>Id",
                table: "UserIdentities",
                column: "DbUser<string>Id");

            migrationBuilder.AddForeignKey(
                name: "FK_UserIdentities_Users_DbUser<string>Id",
                table: "UserIdentities",
                column: "DbUser<string>Id",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
