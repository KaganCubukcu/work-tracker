using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddBreakSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BreakSlots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Label = table.Column<string>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "TEXT", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakSlots", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BreakSlots");
        }
    }
}
