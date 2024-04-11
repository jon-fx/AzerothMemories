using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AzerothMemories.WebServer.Migrations
{
    /// <inheritdoc />
    public partial class CreateDatabase4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK__Operations",
                table: "_Operations");

            migrationBuilder.DropIndex(
                name: "IX_CommitTime",
                table: "_Operations");

            migrationBuilder.DropColumn(
                name: "CommitTime",
                table: "_Operations");

            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "_Operations",
                newName: "LoggedAt");

            migrationBuilder.RenameColumn(
                name: "AgentId",
                table: "_Operations",
                newName: "Uuid");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "_Operations",
                newName: "NestedOperations");

            migrationBuilder.RenameIndex(
                name: "IX_StartTime",
                table: "_Operations",
                newName: "IX__Operations_LoggedAt");

            migrationBuilder.AddColumn<long>(
                name: "Index",
                table: "_Operations",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "HostId",
                table: "_Operations",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK__Operations",
                table: "_Operations",
                column: "Index");

            migrationBuilder.CreateTable(
                name: "_Events",
                columns: table => new
                {
                    Index = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Uuid = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    LoggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValueJson = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Events", x => x.Index);
                });

            migrationBuilder.CreateTable(
                name: "_Timers",
                columns: table => new
                {
                    Uuid = table.Column<string>(type: "text", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    FiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValueJson = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Timers", x => x.Uuid);
                });

            migrationBuilder.CreateIndex(
                name: "IX__Operations_Uuid",
                table: "_Operations",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__Events_LoggedAt",
                table: "_Events",
                column: "LoggedAt");

            migrationBuilder.CreateIndex(
                name: "IX__Events_State_LoggedAt",
                table: "_Events",
                columns: new[] { "State", "LoggedAt" });

            migrationBuilder.CreateIndex(
                name: "IX__Events_Uuid",
                table: "_Events",
                column: "Uuid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX__Timers_FiresAt",
                table: "_Timers",
                column: "FiresAt");

            migrationBuilder.CreateIndex(
                name: "IX__Timers_State_FiresAt",
                table: "_Timers",
                columns: new[] { "State", "FiresAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "_Events");

            migrationBuilder.DropTable(
                name: "_Timers");

            migrationBuilder.DropPrimaryKey(
                name: "PK__Operations",
                table: "_Operations");

            migrationBuilder.DropIndex(
                name: "IX__Operations_Uuid",
                table: "_Operations");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "_Operations");

            migrationBuilder.DropColumn(
                name: "HostId",
                table: "_Operations");

            migrationBuilder.RenameColumn(
                name: "Uuid",
                table: "_Operations",
                newName: "AgentId");

            migrationBuilder.RenameColumn(
                name: "NestedOperations",
                table: "_Operations",
                newName: "Id");

            migrationBuilder.RenameColumn(
                name: "LoggedAt",
                table: "_Operations",
                newName: "StartTime");

            migrationBuilder.RenameIndex(
                name: "IX__Operations_LoggedAt",
                table: "_Operations",
                newName: "IX_StartTime");

            migrationBuilder.AddColumn<DateTime>(
                name: "CommitTime",
                table: "_Operations",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddPrimaryKey(
                name: "PK__Operations",
                table: "_Operations",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_CommitTime",
                table: "_Operations",
                column: "CommitTime");
        }
    }
}
