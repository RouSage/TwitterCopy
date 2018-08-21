using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterCopy.Core.Entities.TweetAggregate
{
    public class Tweet : BaseEntity
    {
        [StringLength(280)]
        public string Text { get; private set; }

        public Guid UserId { get; private set; }

        [DataType(DataType.DateTime)]
        public DateTime PostedOn { get; private set; } = DateTime.UtcNow;

        public int LikeCount { get; private set; }

        public int RetweetCount { get; private set; }

        // Relationships

        [ForeignKey("UserId")]
        public TwitterCopyUser User { get; private set; }

        private readonly List<Like> _likes;

        public IReadOnlyCollection<Like> Likes => _likes.AsReadOnly();

        private readonly List<Retweet> _retweets;

        public IReadOnlyCollection<Retweet> Retweets => _retweets.AsReadOnly();

        // Ctors

        // Required by EF
        private Tweet() { }

        public Tweet(string text, TwitterCopyUser user, DateTime postedOn)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new ArgumentNullException(nameof(text));

            if (user == null)
                throw new ArgumentNullException("You must have a Tweet's author", nameof(user));

            User = user;
            Text = text;
            PostedOn = postedOn;
            LikeCount = 0;
            RetweetCount = 0;
            _likes = new List<Like>();
            _retweets = new List<Retweet>();
        }
    }
}
