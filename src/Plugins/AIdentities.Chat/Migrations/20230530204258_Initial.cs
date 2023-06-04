using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIdentities.Chat.Migrations;

 /// <inheritdoc />
 public partial class Initial : Migration
 {
     /// <inheritdoc />
     protected override void Up(MigrationBuilder migrationBuilder)
     {
         migrationBuilder.CreateTable(
             name: "Conversations",
             columns: table => new
             {
                 Id = table.Column<string>(type: "TEXT", nullable: false),
                 CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                 Humans = table.Column<string>(type: "TEXT", nullable: false),
                 AIdentityIds = table.Column<string>(type: "TEXT", nullable: false),
                 Title = table.Column<string>(type: "TEXT", nullable: false),
                 UpdatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                 MessageCount = table.Column<int>(type: "INTEGER", nullable: false)
             },
             constraints: table =>
             {
                 table.PrimaryKey("PK_Conversations", x => x.Id);
             });

         migrationBuilder.CreateTable(
             name: "ConversationMessage",
             columns: table => new
             {
                 Id = table.Column<string>(type: "TEXT", nullable: false),
                 CreationDate = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                 IsAIGenerated = table.Column<bool>(type: "INTEGER", nullable: false),
                 AuthorId = table.Column<string>(type: "TEXT", nullable: false),
                 AuthorName = table.Column<string>(type: "TEXT", nullable: false),
                 Text = table.Column<string>(type: "TEXT", nullable: false),
                 ConversationId = table.Column<string>(type: "TEXT", nullable: true)
             },
             constraints: table =>
             {
                 table.PrimaryKey("PK_ConversationMessage", x => x.Id);
                 table.ForeignKey(
                     name: "FK_ConversationMessage_Conversations_ConversationId",
                     column: x => x.ConversationId,
                     principalTable: "Conversations",
                     principalColumn: "Id");
             });

         migrationBuilder.CreateIndex(
             name: "IX_ConversationMessage_ConversationId",
             table: "ConversationMessage",
             column: "ConversationId");
     }

     /// <inheritdoc />
     protected override void Down(MigrationBuilder migrationBuilder)
     {
         migrationBuilder.DropTable(
             name: "ConversationMessage");

         migrationBuilder.DropTable(
             name: "Conversations");
     }
 }
