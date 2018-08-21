using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using TwitterCopy.Core.Entities.TweetAggregate;

namespace TwitterCopy.Core.Entities
{
    public class TwitterCopyUser : IdentityUser<Guid>
    {
        [PersonalData]
        public string Slug { get; set; }

        [PersonalData]
        [StringLength(1000)]
        public string Bio { get; set; }

        [PersonalData]
        [DataType(DataType.Date)]
        public DateTime RegisterDate { get; set; } = DateTime.Now.Date;

        [PersonalData]
        [DataType(DataType.Date)]
        public DateTime? BirthDate { get; set; } = DateTime.Now.Date;

        [PersonalData]
        public string Location { get; set; }

        [PersonalData]
        [DataType(DataType.Url)]
        public string Website { get; set; }

        public string Avatar { get; set; }

        // Relationships

        private readonly List<Like> _likes = new List<Like>();

        public IReadOnlyCollection<Like> Likes => _likes.AsReadOnly();

        private readonly List<Retweet> _retweets = new List<Retweet>();

        public IReadOnlyCollection<Retweet> Retweets => _retweets.AsReadOnly();

        private readonly List<UserToUser> _following = new List<UserToUser>();

        public IReadOnlyCollection<UserToUser> Following => _following.AsReadOnly();

        private readonly List<UserToUser> _followers = new List<UserToUser>();

        public IReadOnlyCollection<UserToUser> Followers => _followers.AsReadOnly();

        private readonly List<Tweet> _tweets = new List<Tweet>();

        public IReadOnlyCollection<Tweet> Tweets => _tweets.AsReadOnly();

    }
}
