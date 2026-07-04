using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Topzinto.Erp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddChatUnread : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChatChannelReads",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UserId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ChannelId = table.Column<Guid>(type: "TEXT", nullable: false),
                    LastReadAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatChannelReads", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatChannelReads_ChatChannels_ChannelId",
                        column: x => x.ChannelId,
                        principalTable: "ChatChannels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatChannelReads_ChannelId",
                table: "ChatChannelReads",
                column: "ChannelId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatChannelReads_UserId_ChannelId",
                table: "ChatChannelReads",
                columns: new[] { "UserId", "ChannelId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChatChannelReads");
        }
    }
}
