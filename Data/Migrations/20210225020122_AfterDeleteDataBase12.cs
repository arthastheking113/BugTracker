using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Data.Migrations
{
    public partial class AfterDeleteDataBase12 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Attachment_Project_ProjectId",
                table: "Attachment");

            migrationBuilder.DropIndex(
                name: "IX_Attachment_ProjectId",
                table: "Attachment");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "Attachment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "Attachment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Attachment_ProjectId",
                table: "Attachment",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Attachment_Project_ProjectId",
                table: "Attachment",
                column: "ProjectId",
                principalTable: "Project",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
