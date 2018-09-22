using Microsoft.EntityFrameworkCore.Migrations;

namespace TwitterCopy.Infrastructure.Migrations
{
    public partial class RetweetsOnDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Retweets_Tweet_TweetId",
                table: "Retweets");

            migrationBuilder.AddForeignKey(
                name: "FK_Retweets_Tweet_TweetId",
                table: "Retweets",
                column: "TweetId",
                principalTable: "Tweet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Retweets_Tweet_TweetId",
                table: "Retweets");

            migrationBuilder.AddForeignKey(
                name: "FK_Retweets_Tweet_TweetId",
                table: "Retweets",
                column: "TweetId",
                principalTable: "Tweet",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
