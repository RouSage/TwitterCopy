using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TwitterCopy.Entities
{
    // Add profile data for application users by adding properties to the TwitterCopyUser class
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
        [Url(ErrorMessage = "Url is not valid")]
        [DataType(DataType.Url)]
        public string Website { get; set; }

        public ICollection<Like> Likes { get; set; }

        public ICollection<Retweet> Retweets { get; set; }

        [PersonalData]
        public ICollection<UserToUser> Following { get; set; }

        [PersonalData]
        public ICollection<UserToUser> Followers { get; set; }

        [PersonalData]
        public ICollection<Tweet> Tweets { get; set; }
    }
}
