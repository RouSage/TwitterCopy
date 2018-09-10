using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterCopy.Core.Entities
{
    public class TweetToTweet
    {
        public int ReplyToId { get; set; }

        public int ReplyFromId { get; set; }

        // Relationships

        [ForeignKey(nameof(ReplyToId))]
        public Tweet ReplyTo { get; set; }

        [ForeignKey(nameof(ReplyFromId))]
        public Tweet ReplyFrom { get; set; }
    }
}
