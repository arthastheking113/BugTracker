using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Data.Migrations
{
    public partial class AfterDeleteDataBase7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_AspNetUsers_CustomUserId",
                table: "Ticket");

            migrationBuilder.RenameColumn(
                name: "CustomUserId",
                table: "Ticket",
                newName: "OwnnerId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_CustomUserId",
                table: "Ticket",
                newName: "IX_Ticket_OwnnerId");

            migrationBuilder.AddColumn<string>(
                name: "DeveloperId",
                table: "Ticket",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ticket_DeveloperId",
                table: "Ticket",
                column: "DeveloperId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_AspNetUsers_DeveloperId",
                table: "Ticket",
                column: "DeveloperId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_AspNetUsers_OwnnerId",
                table: "Ticket",
                column: "OwnnerId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_AspNetUsers_DeveloperId",
                table: "Ticket");

            migrationBuilder.DropForeignKey(
                name: "FK_Ticket_AspNetUsers_OwnnerId",
                table: "Ticket");

            migrationBuilder.DropIndex(
                name: "IX_Ticket_DeveloperId",
                table: "Ticket");

            migrationBuilder.DropColumn(
                name: "DeveloperId",
                table: "Ticket");

            migrationBuilder.RenameColumn(
                name: "OwnnerId",
                table: "Ticket",
                newName: "CustomUserId");

            migrationBuilder.RenameIndex(
                name: "IX_Ticket_OwnnerId",
                table: "Ticket",
                newName: "IX_Ticket_CustomUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Ticket_AspNetUsers_CustomUserId",
                table: "Ticket",
                column: "CustomUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
