using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acoustican.Migrations
{
    /// <inheritdoc />
    public partial class AddCourseAndLessonFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPreview",
                table: "Lessons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Requirements",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "WhatYoullLearn",
                table: "Courses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPreview",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "Requirements",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "WhatYoullLearn",
                table: "Courses");
        }
    }
}
