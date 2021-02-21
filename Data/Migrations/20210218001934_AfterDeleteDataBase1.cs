using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Data.Migrations
{
    public partial class AfterDeleteDataBase1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Project_AspNetUsers_CustomUserId",
                table: "Project");

            migrationBuilder.DropIndex(
                name: "IX_Project_CustomUserId",
                table: "Project");

            migrationBuilder.DropColumn(
                name: "CustomUserId",
                table: "Project");

            migrationBuilder.CreateTable(
                name: "CustomUserProject",
                columns: table => new
                {
                    CustomUsersId = table.Column<string>(type: "text", nullable: false),
                    ProjectsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomUserProject", x => new { x.CustomUsersId, x.ProjectsId });
                    table.ForeignKey(
                        name: "FK_CustomUserProject_AspNetUsers_CustomUsersId",
                        column: x => x.CustomUsersId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomUserProject_Project_ProjectsId",
                        column: x => x.ProjectsId,
                        principalTable: "Project",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomUserProject_ProjectsId",
                table: "CustomUserProject",
                column: "ProjectsId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomUserProject");

            migrationBuilder.AddColumn<string>(
                name: "CustomUserId",
                table: "Project",
                type: "text",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Project_CustomUserId",
                table: "Project",
                column: "CustomUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_AspNetUsers_CustomUserId",
                table: "Project",
                column: "CustomUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
