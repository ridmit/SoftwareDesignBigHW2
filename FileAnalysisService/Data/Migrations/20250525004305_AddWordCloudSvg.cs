using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileAnalysisService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWordCloudSvg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WordCloudImage",
                table: "Analyses");

            migrationBuilder.AddColumn<string>(
                name: "WordCloudSvg",
                table: "Analyses",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WordCloudSvg",
                table: "Analyses");

            migrationBuilder.AddColumn<byte[]>(
                name: "WordCloudImage",
                table: "Analyses",
                type: "bytea",
                nullable: true);
        }
    }
}
