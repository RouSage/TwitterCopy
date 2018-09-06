using Microsoft.EntityFrameworkCore.Migrations;

namespace TwitterCopy.Infrastructure.Migrations
{
    public partial class UserBanner : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Banner",
                table: "AspNetUsers",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Banner",
                table: "AspNetUsers");
        }
    }
}
