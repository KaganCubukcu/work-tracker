using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddDailyLogDisplayTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DisplayTime",
                table: "DailyLogs",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayTime",
                table: "DailyLogs");
        }
    }
}
