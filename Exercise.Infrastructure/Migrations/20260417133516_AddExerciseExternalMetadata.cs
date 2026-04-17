using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exercise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseExternalMetadata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalId",
                table: "Exercises",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InstructionsJson",
                table: "Exercises",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecondaryMusclesJson",
                table: "Exercises",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourcePayloadJson",
                table: "Exercises",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceProvider",
                table: "Exercises",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exercises_ExternalId",
                table: "Exercises",
                column: "ExternalId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Exercises_ExternalId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ExternalId",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "InstructionsJson",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SecondaryMusclesJson",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SourcePayloadJson",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "SourceProvider",
                table: "Exercises");
        }
    }
}
