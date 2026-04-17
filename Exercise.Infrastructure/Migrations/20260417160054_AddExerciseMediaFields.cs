using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exercise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseMediaFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediaKind",
                table: "Exercises",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaUrl",
                table: "Exercises",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaKind",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "MediaUrl",
                table: "Exercises");
        }
    }
}
