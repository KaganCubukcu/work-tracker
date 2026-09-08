using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPomodoro : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PomodoroLongBreakMinutes",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 15);

            migrationBuilder.AddColumn<int>(
                name: "PomodoroRoundsBeforeLongBreak",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.AddColumn<int>(
                name: "PomodoroShortBreakMinutes",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 5);

            migrationBuilder.AddColumn<int>(
                name: "PomodoroWorkMinutes",
                table: "UserSettings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 25);

            migrationBuilder.CreateTable(
                name: "PomodoroSessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    StartedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "TEXT", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PomodoroSessions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PomodoroSessions");

            migrationBuilder.DropColumn(
                name: "PomodoroLongBreakMinutes",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "PomodoroRoundsBeforeLongBreak",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "PomodoroShortBreakMinutes",
                table: "UserSettings");

            migrationBuilder.DropColumn(
                name: "PomodoroWorkMinutes",
                table: "UserSettings");
        }
    }
}
