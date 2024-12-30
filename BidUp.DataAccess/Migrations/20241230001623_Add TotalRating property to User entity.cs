using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidUp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddTotalRatingpropertytoUserentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TotalRating",
                schema: "security",
                table: "User",
                type: "decimal(2,1)",
                precision: 2,
                scale: 1,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalRating",
                schema: "security",
                table: "User");
        }
    }
}
