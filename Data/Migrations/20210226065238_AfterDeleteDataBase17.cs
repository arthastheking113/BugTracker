using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker.Data.Migrations
{
    public partial class AfterDeleteDataBase17 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "IsSeen",
                table: "Inbox",
                newName: "IsSeenBySender");

            migrationBuilder.AddColumn<bool>(
                name: "IsSeenByReceiver",
                table: "Inbox",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSeenByReceiver",
                table: "Inbox");

            migrationBuilder.RenameColumn(
                name: "IsSeenBySender",
                table: "Inbox",
                newName: "IsSeen");
        }
    }
}
