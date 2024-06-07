using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidUp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddProfilePictureUrlpropertytouserentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProfilePictureUrl",
                schema: "security",
                table: "Users",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ProfilePictureUrl",
                schema: "security",
                table: "Users");
        }
    }
}
