using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidX.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddFullNamepropertyinUserentityandmakeitcomputedcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                schema: "security",
                table: "User",
                type: "nvarchar(max)",
                nullable: false,
                computedColumnSql: "[FirstName] + ' ' + [LastName]",
                stored: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                schema: "security",
                table: "User");
        }
    }
}
