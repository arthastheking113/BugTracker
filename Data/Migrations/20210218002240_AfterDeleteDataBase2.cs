using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Data.Migrations
{
    public partial class AfterDeleteDataBase2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_Status_StatusId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_StatusId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Project");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Project",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Project_StatusId",
                table: "Project",
                column: "StatusId");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Status_StatusId",
                table: "Project",
                column: "StatusId",
                principalTable: "Status",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
