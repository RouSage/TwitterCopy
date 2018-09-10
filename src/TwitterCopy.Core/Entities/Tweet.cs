using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterCopy.Core.Entities
{
    public class Tweet
    {
        public int Id { get; set; }

        [StringLength(280)]
        public string Text { get; set; }

        public Guid UserId { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime PostedOn { get; set; } = DateTime.UtcNow;

        public int LikeCount { get; set; }

        public int RetweetCount { get; set; }

        public int ReplyCount { get; set; }

        // Relationships

        [ForeignKey("UserId")]
        public TwitterCopyUser User { get; set; }

        public ICollection<Like> Likes { get; set; }

        public ICollection<Retweet> Retweets { get; set; }

        public ICollection<TweetToTweet> RepliesTo { get; set; }

        public ICollection<TweetToTweet> RepliesFrom { get; set; }
    }
}
