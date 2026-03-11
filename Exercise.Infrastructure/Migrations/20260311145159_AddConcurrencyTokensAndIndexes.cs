using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exercise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyTokensAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutExercises_Exercises__exercisesId",
                table: "WorkoutExercises");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutPlanWorkouts_Workouts__workoutsId",
                table: "WorkoutPlanWorkouts");

            migrationBuilder.DropIndex(
                name: "IX_Workouts_UserId",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutPlans_UserId",
                table: "WorkoutPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkoutExercises",
                table: "WorkoutExercises");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutExercises__exercisesId",
                table: "WorkoutExercises");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseLogs_UserId",
                table: "ExerciseLogs");

            migrationBuilder.RenameColumn(
                name: "_workoutsId",
                table: "WorkoutPlanWorkouts",
                newName: "WorkoutsId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkoutPlanWorkouts__workoutsId",
                table: "WorkoutPlanWorkouts",
                newName: "IX_WorkoutPlanWorkouts_WorkoutsId");

            migrationBuilder.RenameColumn(
                name: "_exercisesId",
                table: "WorkoutExercises",
                newName: "ExercisesId");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyToken",
                table: "Workouts",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyToken",
                table: "WorkoutPlans",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyToken",
                table: "Users",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Role",
                table: "Users",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "User");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyToken",
                table: "Exercises",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ConcurrencyToken",
                table: "ExerciseLogs",
                type: "nvarchar(32)",
                maxLength: 32,
                nullable: true,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkoutExercises",
                table: "WorkoutExercises",
                columns: new[] { "ExercisesId", "WorkoutId" });

            migrationBuilder.CreateTable(
                name: "Analytics",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AnalyticsType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConcurrencyToken = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: true, defaultValue: ""),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Analytics", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_UserId_Date",
                table: "Workouts",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_UserId_StartDate",
                table: "WorkoutPlans",
                columns: new[] { "UserId", "StartDate" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutExercises_WorkoutId",
                table: "WorkoutExercises",
                column: "WorkoutId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseLogs_UserId_Date",
                table: "ExerciseLogs",
                columns: new[] { "UserId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Analytics_UserId",
                table: "Analytics",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutExercises_Exercises_ExercisesId",
                table: "WorkoutExercises",
                column: "ExercisesId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutPlanWorkouts_Workouts_WorkoutsId",
                table: "WorkoutPlanWorkouts",
                column: "WorkoutsId",
                principalTable: "Workouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutExercises_Exercises_ExercisesId",
                table: "WorkoutExercises");

            migrationBuilder.DropForeignKey(
                name: "FK_WorkoutPlanWorkouts_Workouts_WorkoutsId",
                table: "WorkoutPlanWorkouts");

            migrationBuilder.DropTable(
                name: "Analytics");

            migrationBuilder.DropIndex(
                name: "IX_Workouts_UserId_Date",
                table: "Workouts");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutPlans_UserId_StartDate",
                table: "WorkoutPlans");

            migrationBuilder.DropPrimaryKey(
                name: "PK_WorkoutExercises",
                table: "WorkoutExercises");

            migrationBuilder.DropIndex(
                name: "IX_WorkoutExercises_WorkoutId",
                table: "WorkoutExercises");

            migrationBuilder.DropIndex(
                name: "IX_ExerciseLogs_UserId_Date",
                table: "ExerciseLogs");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "WorkoutPlans");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "ConcurrencyToken",
                table: "ExerciseLogs");

            migrationBuilder.RenameColumn(
                name: "WorkoutsId",
                table: "WorkoutPlanWorkouts",
                newName: "_workoutsId");

            migrationBuilder.RenameIndex(
                name: "IX_WorkoutPlanWorkouts_WorkoutsId",
                table: "WorkoutPlanWorkouts",
                newName: "IX_WorkoutPlanWorkouts__workoutsId");

            migrationBuilder.RenameColumn(
                name: "ExercisesId",
                table: "WorkoutExercises",
                newName: "_exercisesId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_WorkoutExercises",
                table: "WorkoutExercises",
                columns: new[] { "WorkoutId", "_exercisesId" });

            migrationBuilder.CreateIndex(
                name: "IX_Workouts_UserId",
                table: "Workouts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutPlans_UserId",
                table: "WorkoutPlans",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkoutExercises__exercisesId",
                table: "WorkoutExercises",
                column: "_exercisesId");

            migrationBuilder.CreateIndex(
                name: "IX_ExerciseLogs_UserId",
                table: "ExerciseLogs",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutExercises_Exercises__exercisesId",
                table: "WorkoutExercises",
                column: "_exercisesId",
                principalTable: "Exercises",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkoutPlanWorkouts_Workouts__workoutsId",
                table: "WorkoutPlanWorkouts",
                column: "_workoutsId",
                principalTable: "Workouts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
