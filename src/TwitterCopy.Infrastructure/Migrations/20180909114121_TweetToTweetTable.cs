using Microsoft.EntityFrameworkCore.Migrations;

namespace TwitterCopy.Infrastructure.Migrations
{
    public partial class TweetToTweetTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TweetToTweet",
                columns: table => new
                {
                    ReplyTweetId = table.Column<int>(nullable: false),
                    ReplyToId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweetToTweet", x => new { x.ReplyTweetId, x.ReplyToId });
                    table.ForeignKey(
                        name: "FK_TweetToTweet_Tweet_ReplyToId",
                        column: x => x.ReplyToId,
                        principalTable: "Tweet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TweetToTweet_Tweet_ReplyTweetId",
                        column: x => x.ReplyTweetId,
                        principalTable: "Tweet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TweetToTweet_ReplyToId",
                table: "TweetToTweet",
                column: "ReplyToId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TweetToTweet");
        }
    }
}
