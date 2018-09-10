using Microsoft.EntityFrameworkCore.Migrations;

namespace TwitterCopy.Infrastructure.Migrations
{
    public partial class RemoveReplyCount : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReplyCount",
                table: "Tweet");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReplyCount",
                table: "Tweet",
                nullable: false,
                defaultValue: 0);
        }
    }
}
