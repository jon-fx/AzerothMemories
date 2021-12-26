using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzerothMemories.WebServer.Migrations
{
    public partial class AddAccounts3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BattleNetToken",
                table: "Accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "BattleNetTokenExpiresAt",
                table: "Accounts",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "BattleTag",
                table: "Accounts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<long>(
                name: "BlizzardId",
                table: "Accounts",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "BlizzardRegion",
                table: "Accounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BattleNetToken",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "BattleNetTokenExpiresAt",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "BattleTag",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "BlizzardId",
                table: "Accounts");

            migrationBuilder.DropColumn(
                name: "BlizzardRegion",
                table: "Accounts");
        }
    }
}
