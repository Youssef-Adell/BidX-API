using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidUp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MergeProductintoAuctionentityandaddIsAcceptedpropertytoBidentityandaddproperindexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auction_Bid_HighestBidId",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_Auction_Category_CategoryId",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_Auction_City_CityId",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_Auction_User_AuctioneerId",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Product_ProductId",
                table: "ProductImage");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropIndex(
                name: "IX_Bid_AuctionId",
                table: "Bid");

            migrationBuilder.DropIndex(
                name: "IX_Auction_HighestBidId",
                table: "Auction");

            migrationBuilder.DropColumn(
                name: "BidTime",
                table: "Bid");

            migrationBuilder.DropColumn(
                name: "HighestBidId",
                table: "Auction");

            migrationBuilder.RenameIndex(
                name: "RefreshTokenIndex",
                schema: "security",
                table: "User",
                newName: "IX_User_RefreshToken");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ProductImage",
                newName: "AuctionId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_ProductId",
                table: "ProductImage",
                newName: "IX_ProductImage_AuctionId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Bid",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AddColumn<bool>(
                name: "IsAccepted",
                table: "Bid",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PlacedAt",
                table: "Bid",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AlterColumn<decimal>(
                name: "StartingPrice",
                table: "Auction",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "StartTime",
                table: "Auction",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinBidIncrement",
                table: "Auction",
                type: "decimal(18,0)",
                precision: 18,
                scale: 0,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "EndTime",
                table: "Auction",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "ProductCondition",
                table: "Auction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductDescription",
                table: "Auction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProductName",
                table: "Auction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "Auction",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_Bid_AuctionId_Amount",
                table: "Bid",
                columns: new[] { "AuctionId", "Amount" },
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_Auction_EndTime",
                table: "Auction",
                column: "EndTime");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Auction_ProductCondition",
                table: "Auction",
                sql: "ProductCondition IN ('New', 'Used')");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_Category_CategoryId",
                table: "Auction",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_City_CityId",
                table: "Auction",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_User_AuctioneerId",
                table: "Auction",
                column: "AuctioneerId",
                principalSchema: "security",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Auction_AuctionId",
                table: "ProductImage",
                column: "AuctionId",
                principalTable: "Auction",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Auction_Category_CategoryId",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_Auction_City_CityId",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_Auction_User_AuctioneerId",
                table: "Auction");

            migrationBuilder.DropForeignKey(
                name: "FK_ProductImage_Auction_AuctionId",
                table: "ProductImage");

            migrationBuilder.DropIndex(
                name: "IX_Bid_AuctionId_Amount",
                table: "Bid");

            migrationBuilder.DropIndex(
                name: "IX_Auction_EndTime",
                table: "Auction");

            migrationBuilder.DropCheckConstraint(
                name: "CK_Auction_ProductCondition",
                table: "Auction");

            migrationBuilder.DropColumn(
                name: "IsAccepted",
                table: "Bid");

            migrationBuilder.DropColumn(
                name: "PlacedAt",
                table: "Bid");

            migrationBuilder.DropColumn(
                name: "ProductCondition",
                table: "Auction");

            migrationBuilder.DropColumn(
                name: "ProductDescription",
                table: "Auction");

            migrationBuilder.DropColumn(
                name: "ProductName",
                table: "Auction");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "Auction");

            migrationBuilder.RenameIndex(
                name: "IX_User_RefreshToken",
                schema: "security",
                table: "User",
                newName: "RefreshTokenIndex");

            migrationBuilder.RenameColumn(
                name: "AuctionId",
                table: "ProductImage",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImage_AuctionId",
                table: "ProductImage",
                newName: "IX_ProductImage_ProductId");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Bid",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,0)",
                oldPrecision: 18,
                oldScale: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "BidTime",
                table: "Bid",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<decimal>(
                name: "StartingPrice",
                table: "Auction",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,0)",
                oldPrecision: 18,
                oldScale: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartTime",
                table: "Auction",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AlterColumn<decimal>(
                name: "MinBidIncrement",
                table: "Auction",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,0)",
                oldPrecision: 18,
                oldScale: 0);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndTime",
                table: "Auction",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.AddColumn<int>(
                name: "HighestBidId",
                table: "Auction",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuctionId = table.Column<int>(type: "int", nullable: false),
                    Condition = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Product_Auction_AuctionId",
                        column: x => x.AuctionId,
                        principalTable: "Auction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bid_AuctionId",
                table: "Bid",
                column: "AuctionId");

            migrationBuilder.CreateIndex(
                name: "IX_Auction_HighestBidId",
                table: "Auction",
                column: "HighestBidId",
                unique: true,
                filter: "[HighestBidId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Product_AuctionId",
                table: "Product",
                column: "AuctionId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_Bid_HighestBidId",
                table: "Auction",
                column: "HighestBidId",
                principalTable: "Bid",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_Category_CategoryId",
                table: "Auction",
                column: "CategoryId",
                principalTable: "Category",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_City_CityId",
                table: "Auction",
                column: "CityId",
                principalTable: "City",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Auction_User_AuctioneerId",
                table: "Auction",
                column: "AuctioneerId",
                principalSchema: "security",
                principalTable: "User",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImage_Product_ProductId",
                table: "ProductImage",
                column: "ProductId",
                principalTable: "Product",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
