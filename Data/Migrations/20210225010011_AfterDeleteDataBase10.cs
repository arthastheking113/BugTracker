using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Data.Migrations
{
    public partial class AfterDeleteDataBase10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_AspNetUsers_CustomUserId1",
                table: "Attachment");

            migrationBuilder.DropIndex(
                name: "IX_Attachment_CustomUserId1",
                table: "Attachment");

            migrationBuilder.DropColumn(
                name: "CustomUserId1",
                table: "Attachment");

            migrationBuilder.AlterColumn<string>(
                name: "CustomUserId",
                table: "Attachment",
                type: "text",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.CreateIndex(
                name: "IX_Attachment_CustomUserId",
                table: "Attachment",
                column: "CustomUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_AspNetUsers_CustomUserId",
                table: "Attachment",
                column: "CustomUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_AspNetUsers_CustomUserId",
                table: "Attachment");

            migrationBuilder.DropIndex(
                name: "IX_Attachment_CustomUserId",
                table: "Attachment");

            migrationBuilder.AlterColumn<int>(
                name: "CustomUserId",
                table: "Attachment",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CustomUserId1",
                table: "Attachment",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Attachment_CustomUserId1",
                table: "Attachment",
                column: "CustomUserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_AspNetUsers_CustomUserId1",
                table: "Attachment",
                column: "CustomUserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
