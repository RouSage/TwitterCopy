using Microsoft.EntityFrameworkCore.Migrations;

namespace TwitterCopy.Infrastructure.Migrations
{
    public partial class Replies : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReplyCount",
                table: "Tweet",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TweetToTweet",
                columns: table => new
                {
                    ReplyToId = table.Column<int>(nullable: false),
                    ReplyFromId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TweetToTweet", x => new { x.ReplyToId, x.ReplyFromId });
                    table.ForeignKey(
                        name: "FK_TweetToTweet_Tweet_ReplyFromId",
                        column: x => x.ReplyFromId,
                        principalTable: "Tweet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TweetToTweet_Tweet_ReplyToId",
                        column: x => x.ReplyToId,
                        principalTable: "Tweet",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TweetToTweet_ReplyFromId",
                table: "TweetToTweet",
                column: "ReplyFromId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TweetToTweet");

            migrationBuilder.DropColumn(
                name: "ReplyCount",
                table: "Tweet");
        }
    }
}
