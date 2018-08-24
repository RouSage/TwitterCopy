using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterCopy.Core.Entities.TweetAggregate
{
    public class Like
    {
        public int Id { get; private set; }

        public int TweetId { get; private set; }

        public Guid UserId { get; private set; }

        [DataType(DataType.DateTime)]
        public DateTime DateLiked { get; private set; } = DateTime.UtcNow;

        // Relationships

        [ForeignKey("TweetId")]
        public Tweet Tweet { get; private set; }

        [ForeignKey("UserId")]
        public TwitterCopyUser User { get; private set; }

        // Ctors

        private Like() { }

        public Like(Tweet tweet, TwitterCopyUser user)
        {
            Tweet = tweet;
            User = user;
        }
    }
}
