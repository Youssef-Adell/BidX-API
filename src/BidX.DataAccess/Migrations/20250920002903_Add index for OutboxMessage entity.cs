using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BidX.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddindexforOutboxMessageentity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_CreatedAt_ProcessedAt",
                table: "OutboxMessage",
                columns: new[] { "CreatedAt", "ProcessedAt" },
                filter: "[ProcessedAt] IS NULL")
                .Annotation("SqlServer:Include", new[] { "Id", "Type", "Content" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OutboxMessage_CreatedAt_ProcessedAt",
                table: "OutboxMessage");
        }
    }
}
