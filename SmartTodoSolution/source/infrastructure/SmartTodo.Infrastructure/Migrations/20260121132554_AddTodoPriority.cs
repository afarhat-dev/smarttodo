using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartTodo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTodoPriority : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Priority",
                table: "TodoItems",
                type: "text",
                nullable: false,
                defaultValue: "Medium");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_Priority",
                table: "TodoItems",
                column: "Priority");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoItems_Priority",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "Priority",
                table: "TodoItems");
        }
    }
}
