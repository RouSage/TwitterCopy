using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterCopy.Core.Entities.TweetAggregate
{
    public class Retweet
    {
        public int Id { get; set; }

        public int TweetId { get; set; }

        public Guid UserId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime RetweetDate { get; set; } = DateTime.UtcNow;

        // Relationships

        [ForeignKey("TweetId")]
        public Tweet Tweet { get; set; }

        [ForeignKey("UserId")]
        public TwitterCopyUser User { get; set; }
    }
}
