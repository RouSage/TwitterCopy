using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TwitterCopy.Migrations
{
    public partial class FollowersAndFollowing : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserToUser",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    FollowerId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserToUser", x => new { x.UserId, x.FollowerId });
                    table.ForeignKey(
                        name: "FK_UserToUser_AspNetUsers_FollowerId",
                        column: x => x.FollowerId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserToUser_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserToUser_FollowerId",
                table: "UserToUser",
                column: "FollowerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserToUser");
        }
    }
}
