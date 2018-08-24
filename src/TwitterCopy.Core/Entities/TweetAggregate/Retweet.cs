using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterCopy.Core.Entities.TweetAggregate
{
    public class Retweet
    {
        public int Id { get; private set; }

        public int TweetId { get; private set; }

        public Guid UserId { get; private set; }

        [DataType(DataType.DateTime)]
        public DateTime RetweetDate { get; private set; } = DateTime.UtcNow;

        // Relationships

        [ForeignKey("TweetId")]
        public Tweet Tweet { get; set; }

        [ForeignKey("UserId")]
        public TwitterCopyUser User { get; private set; }

        // Ctors

        private Retweet() { }

        public Retweet(Tweet tweet, TwitterCopyUser user)
        {
            Tweet = tweet;
            User = user;
        }
    }
}
