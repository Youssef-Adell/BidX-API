using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidUp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangetypeofRatingpropertytodecimalinReviewentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Rating",
                table: "Review",
                type: "decimal(2,1)",
                precision: 2,
                scale: 1,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Rating",
                table: "Review",
                type: "int",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(2,1)",
                oldPrecision: 2,
                oldScale: 1);
        }
    }
}
