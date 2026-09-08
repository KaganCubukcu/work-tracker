using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WorkTracker.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixPomodoroSettingsDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE UserSettings SET PomodoroWorkMinutes = 25, PomodoroShortBreakMinutes = 5, PomodoroLongBreakMinutes = 15, PomodoroRoundsBeforeLongBreak = 4 " +
                "WHERE PomodoroWorkMinutes = 0 AND PomodoroShortBreakMinutes = 0 AND PomodoroLongBreakMinutes = 0 AND PomodoroRoundsBeforeLongBreak = 0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
