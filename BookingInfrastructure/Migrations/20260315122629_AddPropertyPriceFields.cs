using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BookingInfrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPropertyPriceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CleaningFreePerDay",
                table: "Properties",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PricePerDay",
                table: "Properties",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CleaningFreePerDay",
                table: "Properties");

            migrationBuilder.DropColumn(
                name: "PricePerDay",
                table: "Properties");
        }
    }
}
