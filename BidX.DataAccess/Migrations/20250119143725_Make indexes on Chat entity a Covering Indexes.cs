using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidX.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class MakeindexesonChatentityaCoveringIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Participant1Id",
                table: "Chat");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Participant2Id",
                table: "Chat");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Participant1Id",
                table: "Chat",
                column: "Participant1Id")
                .Annotation("SqlServer:Include", new[] { "Participant2Id", "LastMessageId" });

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Participant2Id",
                table: "Chat",
                column: "Participant2Id")
                .Annotation("SqlServer:Include", new[] { "Participant1Id", "LastMessageId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Chat_Participant1Id",
                table: "Chat");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Participant2Id",
                table: "Chat");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Participant1Id",
                table: "Chat",
                column: "Participant1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Participant2Id",
                table: "Chat",
                column: "Participant2Id");
        }
    }
}
