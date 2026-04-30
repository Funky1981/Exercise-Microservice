using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exercise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseMediaSourceMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MediaSourcePageUrl",
                table: "Exercises",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaSourcePayloadJson",
                table: "Exercises",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaSourceProvider",
                table: "Exercises",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MediaThumbnailUrl",
                table: "Exercises",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MediaSourcePageUrl",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "MediaSourcePayloadJson",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "MediaSourceProvider",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "MediaThumbnailUrl",
                table: "Exercises");
        }
    }
}
