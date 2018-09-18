using Microsoft.EntityFrameworkCore.Migrations;

namespace TwitterCopy.Infrastructure.Migrations
{
    public partial class RemoveCounts : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LikeCount",
                table: "Tweet");

            migrationBuilder.DropColumn(
                name: "RetweetCount",
                table: "Tweet");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LikeCount",
                table: "Tweet",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "RetweetCount",
                table: "Tweet",
                nullable: false,
                defaultValue: 0);
        }
    }
}
