using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FileAnalysisService.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWordCloudImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "WordCloudImage",
                table: "Analyses",
                type: "bytea",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WordCloudImage",
                table: "Analyses");
        }
    }
}
