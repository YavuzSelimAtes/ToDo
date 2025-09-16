using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalCreatedTaskCountersToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DailyTasksFailed",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MonthlyTasksFailed",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalDailyTasksCreated",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalMonthlyTasksCreated",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalWeeklyTasksCreated",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WeeklyTasksFailed",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DailyTasksFailed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "MonthlyTasksFailed",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalDailyTasksCreated",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalMonthlyTasksCreated",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "TotalWeeklyTasksCreated",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "WeeklyTasksFailed",
                table: "Users");
        }
    }
}
