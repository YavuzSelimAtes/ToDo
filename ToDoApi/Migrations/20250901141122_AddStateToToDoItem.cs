using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoApi.Migrations
{
    /// <inheritdoc />
    public partial class AddStateToToDoItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "ToDoItems");

            migrationBuilder.AddColumn<int>(
                name: "State",
                table: "ToDoItems",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "State",
                table: "ToDoItems");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "ToDoItems",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
