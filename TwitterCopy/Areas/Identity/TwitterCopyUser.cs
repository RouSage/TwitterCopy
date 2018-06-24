using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TwitterCopy.Models;

namespace TwitterCopy.Areas.Identity
{
    // Add profile data for application users by adding properties to the TwitterCopyUser class
    public class TwitterCopyUser : IdentityUser<Guid>
    {
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
        [Url]
        [DataType(DataType.Url)]
        public string Website { get; set; }

        //[Display(Name = "Likes")]
        //public ICollection<Tweet> LikedTweets { get; set; }

        //[PersonalData]
        //public ICollection<Follower> Followers { get; set; }

        [PersonalData]
        public ICollection<Tweet> Tweets { get; set; }
    }
}
