using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Recall.Api.Migrations
{
    /// <inheritdoc />
    public partial class RenameChatTablesToPlural : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessage_Conversation_ConversationId",
                table: "ChatMessage");

            migrationBuilder.DropTable(
                name: "Conversation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatMessage",
                table: "ChatMessage");

            migrationBuilder.RenameTable(
                name: "ChatMessage",
                newName: "ChatMessages");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessage_ConversationId",
                table: "ChatMessages",
                newName: "IX_ChatMessages_ConversationId");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "ChatMessages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatMessages",
                table: "ChatMessages",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Conversations_ConversationId",
                table: "ChatMessages",
                column: "ConversationId",
                principalTable: "Conversations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Conversations_ConversationId",
                table: "ChatMessages");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ChatMessages",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "Content",
                table: "ChatMessages");

            migrationBuilder.RenameTable(
                name: "ChatMessages",
                newName: "ChatMessage");

            migrationBuilder.RenameIndex(
                name: "IX_ChatMessages_ConversationId",
                table: "ChatMessage",
                newName: "IX_ChatMessage_ConversationId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChatMessage",
                table: "ChatMessage",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Conversation",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversation", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessage_Conversation_ConversationId",
                table: "ChatMessage",
                column: "ConversationId",
                principalTable: "Conversation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
