using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIdentities.Chat.Migrations;

 /// <inheritdoc />
 public partial class AddAudioFieldAndLazyLoad : Migration
 {
     /// <inheritdoc />
     protected override void Up(MigrationBuilder migrationBuilder)
     {
         migrationBuilder.DropForeignKey(
             name: "FK_ConversationMessage_Conversations_ConversationId",
             table: "ConversationMessage");

         migrationBuilder.AddColumn<byte[]>(
             name: "Audio",
             table: "ConversationMessage",
             type: "BLOB",
             nullable: true);

         migrationBuilder.AddForeignKey(
             name: "FK_ConversationMessage_Conversations_ConversationId",
             table: "ConversationMessage",
             column: "ConversationId",
             principalTable: "Conversations",
             principalColumn: "Id",
             onDelete: ReferentialAction.Cascade);
     }

     /// <inheritdoc />
     protected override void Down(MigrationBuilder migrationBuilder)
     {
         migrationBuilder.DropForeignKey(
             name: "FK_ConversationMessage_Conversations_ConversationId",
             table: "ConversationMessage");

         migrationBuilder.DropColumn(
             name: "Audio",
             table: "ConversationMessage");

         migrationBuilder.AddForeignKey(
             name: "FK_ConversationMessage_Conversations_ConversationId",
             table: "ConversationMessage",
             column: "ConversationId",
             principalTable: "Conversations",
             principalColumn: "Id");
     }
 }
