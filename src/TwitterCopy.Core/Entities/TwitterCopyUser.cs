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

        public ICollection<Like> Likes { get; set; }

        public ICollection<Retweet> Retweets { get; set; }

        public ICollection<UserToUser> Following { get; set; }

        public ICollection<UserToUser> Followers { get; set; }

        public ICollection<Tweet> Tweets { get; set; }

    }
}
