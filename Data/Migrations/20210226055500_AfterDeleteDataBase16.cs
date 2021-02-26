using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace BugTracker.Data.Migrations
{
    public partial class AfterDeleteDataBase16 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inbox_Inbox_InboxId",
                table: "Inbox");

            migrationBuilder.DropIndex(
                name: "IX_Inbox_InboxId",
                table: "Inbox");

            migrationBuilder.DropColumn(
                name: "InboxId",
                table: "Inbox");

            migrationBuilder.CreateTable(
                name: "Reply",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Message = table.Column<string>(type: "text", nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Subject = table.Column<string>(type: "text", nullable: true),
                    IsSeen = table.Column<bool>(type: "boolean", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    SenderId = table.Column<string>(type: "text", nullable: true),
                    ReceiverId = table.Column<string>(type: "text", nullable: true),
                    InboxId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reply", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reply_AspNetUsers_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reply_AspNetUsers_SenderId",
                        column: x => x.SenderId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Reply_Inbox_InboxId",
                        column: x => x.InboxId,
                        principalTable: "Inbox",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reply_InboxId",
                table: "Reply",
                column: "InboxId");

            migrationBuilder.CreateIndex(
                name: "IX_Reply_ReceiverId",
                table: "Reply",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Reply_SenderId",
                table: "Reply",
                column: "SenderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reply");

            migrationBuilder.AddColumn<int>(
                name: "InboxId",
                table: "Inbox",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inbox_InboxId",
                table: "Inbox",
                column: "InboxId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inbox_Inbox_InboxId",
                table: "Inbox",
                column: "InboxId",
                principalTable: "Inbox",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
