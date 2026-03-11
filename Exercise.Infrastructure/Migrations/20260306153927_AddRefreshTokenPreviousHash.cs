using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Exercise.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshTokenPreviousHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RefreshTokenPreviousHash",
                table: "Users",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RefreshTokenPreviousHash",
                table: "Users");
        }
    }
}
