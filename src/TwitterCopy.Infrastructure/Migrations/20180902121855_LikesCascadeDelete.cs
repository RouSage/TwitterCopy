using Microsoft.EntityFrameworkCore.Migrations;

namespace TwitterCopy.Infrastructure.Migrations
{
    public partial class LikesCascadeDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Tweet_TweetId",
                table: "Likes");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Tweet_TweetId",
                table: "Likes",
                column: "TweetId",
                principalTable: "Tweet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Tweet_TweetId",
                table: "Likes");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Tweet_TweetId",
                table: "Likes",
                column: "TweetId",
                principalTable: "Tweet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
