using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exercise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddExerciseMediaCandidates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ExerciseMediaCandidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExerciseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MediaKind = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourcePageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    SourceProvider = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SourcePayloadJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SourceTitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    MatchScore = table.Column<decimal>(type: "decimal(5,4)", precision: 5, scale: 4, nullable: false),
                    ReviewStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReviewNotes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsSelected = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExerciseMediaCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExerciseMediaCandidates_Exercises_ExerciseId",
                        column: x => x.ExerciseId,
                        principalTable: "Exercises",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMediaCandidates_ExerciseId",
                table: "ExerciseMediaCandidates",
                column: "ExerciseId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseMediaCandidates_ExerciseId_SourceProvider_MediaUrl",
                table: "ExerciseMediaCandidates",
                columns: new[] { "ExerciseId", "SourceProvider", "MediaUrl" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExerciseMediaCandidates");
        }
    }
}
