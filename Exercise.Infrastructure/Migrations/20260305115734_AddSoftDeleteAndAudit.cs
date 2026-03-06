using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exercise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSoftDeleteAndAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Workouts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Workouts",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "WorkoutPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "WorkoutPlans",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Exercises",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Exercises",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "ExerciseLogs",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "ExerciseLogs",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Workouts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "WorkoutPlans");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "WorkoutPlans");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Exercises");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "ExerciseLogs");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "ExerciseLogs");
        }
    }
}
