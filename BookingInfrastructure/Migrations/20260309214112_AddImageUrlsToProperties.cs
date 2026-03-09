using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddImageUrlsToProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImageUrls",
                table: "Properties",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageUrls",
                table: "Properties");
        }
    }
}
