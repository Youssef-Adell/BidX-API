using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidX.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddIsAcceptedtothecompositeindexatBidentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bid_AuctionId_Amount",
                table: "Bid");

            migrationBuilder.CreateIndex(
                name: "IX_Bid_AuctionId_Amount_IsAccepted",
                table: "Bid",
                columns: new[] { "AuctionId", "Amount", "IsAccepted" },
                descending: new bool[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bid_AuctionId_Amount_IsAccepted",
                table: "Bid");

            migrationBuilder.CreateIndex(
                name: "IX_Bid_AuctionId_Amount",
                table: "Bid",
                columns: new[] { "AuctionId", "Amount" },
                descending: new bool[0]);
        }
    }
}
