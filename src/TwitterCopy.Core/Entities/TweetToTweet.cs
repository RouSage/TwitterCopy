using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterCopy.Core.Entities
{
    public class TweetToTweet
    {
        public int ReplyTweetId { get; set; }

        public int ReplyToId { get; set; }

        // Relationships

        [ForeignKey(nameof(ReplyTweetId))]
        public Tweet ReplyTweet { get; set; }

        [ForeignKey(nameof(ReplyToId))]
        public Tweet ReplyTo { get; set; }
    }
}
