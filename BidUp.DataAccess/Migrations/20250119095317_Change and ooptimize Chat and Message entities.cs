using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidUp.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class ChangeandooptimizeChatandMessageentities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Message_User_SenderId",
                table: "Message");

            migrationBuilder.DropTable(
                name: "UserChat");

            migrationBuilder.DropIndex(
                name: "IX_Message_ChatId",
                table: "Message");

            migrationBuilder.RenameColumn(
                name: "Seen",
                table: "Message",
                newName: "IsRead");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "SentAt",
                table: "Message",
                type: "datetimeoffset",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<int>(
                name: "RecipientId",
                table: "Message",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Chat",
                type: "datetimeoffset",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<int>(
                name: "LastMessageId",
                table: "Chat",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Participant1Id",
                table: "Chat",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Participant2Id",
                table: "Chat",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Message_ChatId_RecipientId_IsRead",
                table: "Message",
                columns: new[] { "ChatId", "RecipientId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Message_RecipientId",
                table: "Message",
                column: "RecipientId");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_LastMessageId",
                table: "Chat",
                column: "LastMessageId",
                unique: true,
                filter: "[LastMessageId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Participant1Id",
                table: "Chat",
                column: "Participant1Id");

            migrationBuilder.CreateIndex(
                name: "IX_Chat_Participant2Id",
                table: "Chat",
                column: "Participant2Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_Message_LastMessageId",
                table: "Chat",
                column: "LastMessageId",
                principalTable: "Message",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_User_Participant1Id",
                table: "Chat",
                column: "Participant1Id",
                principalSchema: "security",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Chat_User_Participant2Id",
                table: "Chat",
                column: "Participant2Id",
                principalSchema: "security",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_User_RecipientId",
                table: "Message",
                column: "RecipientId",
                principalSchema: "security",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Message_User_SenderId",
                table: "Message",
                column: "SenderId",
                principalSchema: "security",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chat_Message_LastMessageId",
                table: "Chat");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_User_Participant1Id",
                table: "Chat");

            migrationBuilder.DropForeignKey(
                name: "FK_Chat_User_Participant2Id",
                table: "Chat");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_User_RecipientId",
                table: "Message");

            migrationBuilder.DropForeignKey(
                name: "FK_Message_User_SenderId",
                table: "Message");

            migrationBuilder.DropIndex(
                name: "IX_Message_ChatId_RecipientId_IsRead",
                table: "Message");

            migrationBuilder.DropIndex(
                name: "IX_Message_RecipientId",
                table: "Message");

            migrationBuilder.DropIndex(
                name: "IX_Chat_LastMessageId",
                table: "Chat");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Participant1Id",
                table: "Chat");

            migrationBuilder.DropIndex(
                name: "IX_Chat_Participant2Id",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "RecipientId",
                table: "Message");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "LastMessageId",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "Participant1Id",
                table: "Chat");

            migrationBuilder.DropColumn(
                name: "Participant2Id",
                table: "Chat");

            migrationBuilder.RenameColumn(
                name: "IsRead",
                table: "Message",
                newName: "Seen");

            migrationBuilder.AlterColumn<DateTime>(
                name: "SentAt",
                table: "Message",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "datetimeoffset");

            migrationBuilder.CreateTable(
                name: "UserChat",
                columns: table => new
                {
                    ChatId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserChat", x => new { x.ChatId, x.UserId });
                    table.ForeignKey(
                        name: "FK_UserChat_Chat_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chat",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserChat_User_UserId",
                        column: x => x.UserId,
                        principalSchema: "security",
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Message_ChatId",
                table: "Message",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_UserChat_UserId",
                table: "UserChat",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Message_User_SenderId",
                table: "Message",
                column: "SenderId",
                principalSchema: "security",
                principalTable: "User",
                principalColumn: "Id");
        }
    }
}
