using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AzerothMemories.WebServer.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "_KeyValues",
                columns: table => new
                {
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__KeyValues", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "_Operations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    AgentId = table.Column<string>(type: "text", nullable: false),
                    StartTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommitTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CommandJson = table.Column<string>(type: "text", nullable: false),
                    ItemsJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Operations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "_Sessions",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastSeenAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IPAddress = table.Column<string>(type: "text", nullable: false),
                    UserAgent = table.Column<string>(type: "text", nullable: false),
                    AuthenticatedIdentity = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    IsSignOutForced = table.Column<bool>(type: "boolean", nullable: false),
                    OptionsJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Sessions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    ClaimsJson = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserIdentities",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    Secret = table.Column<string>(type: "text", nullable: false),
                    DbUserstringId = table.Column<string>(name: "DbUser<string>Id", type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserIdentities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserIdentities_Users_DbUser<string>Id",
                        column: x => x.DbUserstringId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX__KeyValues_ExpiresAt",
                table: "_KeyValues",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_CommitTime",
                table: "_Operations",
                column: "CommitTime");

            migrationBuilder.CreateIndex(
                name: "IX_StartTime",
                table: "_Operations",
                column: "StartTime");

            migrationBuilder.CreateIndex(
                name: "IX__Sessions_CreatedAt_IsSignOutForced",
                table: "_Sessions",
                columns: new[] { "CreatedAt", "IsSignOutForced" });

            migrationBuilder.CreateIndex(
                name: "IX__Sessions_IPAddress_IsSignOutForced",
                table: "_Sessions",
                columns: new[] { "IPAddress", "IsSignOutForced" });

            migrationBuilder.CreateIndex(
                name: "IX__Sessions_LastSeenAt_IsSignOutForced",
                table: "_Sessions",
                columns: new[] { "LastSeenAt", "IsSignOutForced" });

            migrationBuilder.CreateIndex(
                name: "IX__Sessions_UserId_IsSignOutForced",
                table: "_Sessions",
                columns: new[] { "UserId", "IsSignOutForced" });

            migrationBuilder.CreateIndex(
                name: "IX_UserIdentities_DbUser<string>Id",
                table: "UserIdentities",
                column: "DbUser<string>Id");

            migrationBuilder.CreateIndex(
                name: "IX_UserIdentities_Id",
                table: "UserIdentities",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Name",
                table: "Users",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_KeyValues");

            migrationBuilder.DropTable(
                name: "_Operations");

            migrationBuilder.DropTable(
                name: "_Sessions");

            migrationBuilder.DropTable(
                name: "UserIdentities");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
