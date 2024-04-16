using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AzerothMemories.WebServer.Migrations
{
    /// <inheritdoc />
    public partial class CreateDatabase5 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_Timers");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Events",
                table: "_Events");

            migrationBuilder.DropIndex(
                name: "IX__Events_LoggedAt",
                table: "_Events");

            migrationBuilder.DropIndex(
                name: "IX__Events_State_LoggedAt",
                table: "_Events");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "_Events");

            migrationBuilder.AddColumn<DateTime>(
                name: "DelayUntil",
                table: "_Events",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK__Events",
                table: "_Events",
                column: "Uuid");

            migrationBuilder.CreateIndex(
                name: "IX__Events_DelayUntil",
                table: "_Events",
                column: "DelayUntil");

            migrationBuilder.CreateIndex(
                name: "IX__Events_State_DelayUntil",
                table: "_Events",
                columns: new[] { "State", "DelayUntil" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK__Events",
                table: "_Events");

            migrationBuilder.DropIndex(
                name: "IX__Events_DelayUntil",
                table: "_Events");

            migrationBuilder.DropIndex(
                name: "IX__Events_State_DelayUntil",
                table: "_Events");

            migrationBuilder.DropColumn(
                name: "DelayUntil",
                table: "_Events");

            migrationBuilder.AddColumn<long>(
                name: "Index",
                table: "_Events",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddPrimaryKey(
                name: "PK__Events",
                table: "_Events",
                column: "Index");

            migrationBuilder.CreateTable(
                name: "_Timers",
                columns: table => new
                {
                    Uuid = table.Column<string>(type: "text", nullable: false),
                    FiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    ValueJson = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Timers", x => x.Uuid);
                });

            migrationBuilder.CreateIndex(
                name: "IX__Events_LoggedAt",
                table: "_Events",
                column: "LoggedAt");

            migrationBuilder.CreateIndex(
                name: "IX__Events_State_LoggedAt",
                table: "_Events",
                columns: new[] { "State", "LoggedAt" });

            migrationBuilder.CreateIndex(
                name: "IX__Timers_FiresAt",
                table: "_Timers",
                column: "FiresAt");

            migrationBuilder.CreateIndex(
                name: "IX__Timers_State_FiresAt",
                table: "_Timers",
                columns: new[] { "State", "FiresAt" });
        }
    }
}
