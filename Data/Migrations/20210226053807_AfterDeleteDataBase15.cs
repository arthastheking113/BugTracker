using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Data.Migrations
{
    public partial class AfterDeleteDataBase15 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

        protected override void Down(MigrationBuilder migrationBuilder)
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
        }
    }
}
