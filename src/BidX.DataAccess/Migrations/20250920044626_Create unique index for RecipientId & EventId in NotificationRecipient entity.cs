using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidX.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class CreateuniqueindexforRecipientIdEventIdinNotificationRecipiententity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EventId",
                table: "NotificationRecipient",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_NotificationRecipient_RecipientId_EventId",
                table: "NotificationRecipient",
                columns: new[] { "RecipientId", "EventId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_NotificationRecipient_RecipientId_EventId",
                table: "NotificationRecipient");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "NotificationRecipient");
        }
    }
}
