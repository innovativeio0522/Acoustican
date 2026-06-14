using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acoustican.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InstructorName",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsBestseller",
                table: "Courses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LectureCount",
                table: "Courses",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "OriginalPrice",
                table: "Courses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "InstructorName",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "IsBestseller",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "LectureCount",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "OriginalPrice",
                table: "Courses");
        }
    }
}
