using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Acoustican.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroPreviewVideoId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviewVideoId",
                table: "HeroContents",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewVideoId",
                table: "HeroContents");
        }
    }
}
