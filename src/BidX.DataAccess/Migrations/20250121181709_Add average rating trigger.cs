using BidX.DataAccess.Utils;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidX.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class Addaverageratingtrigger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var sql = SqlFileReader.ReadSql("average_rating_trigger_up.sql");
            migrationBuilder.Sql(sql);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var sql = SqlFileReader.ReadSql("average_rating_trigger_down.sql");
            migrationBuilder.Sql(sql);
        }
    }
}
