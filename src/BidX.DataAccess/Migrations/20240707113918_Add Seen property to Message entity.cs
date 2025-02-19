using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidX.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddSeenpropertytoMessageentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Seen",
                table: "Message",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Seen",
                table: "Message");
        }
    }
}
