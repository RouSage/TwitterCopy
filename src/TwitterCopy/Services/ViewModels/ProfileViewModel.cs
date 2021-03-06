﻿using System.ComponentModel.DataAnnotations;

namespace TwitterCopy.Models
{
    public class ProfileViewModel
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        public string Slug { get; set; }

        public string Bio { get; set; }

        public string Location { get; set; }

        public string Website { get; set; }

        [Display(Name = "Tweets")]
        public int TweetCount { get; set; }

        [Display(Name = "Following")]
        public int FollowingCount { get; set; }

        [Display(Name = "Followers")]
        public int FollowersCount { get; set; }

        [Display(Name = "Likes")]
        public int LikesCount { get; set; }

        public string Avatar { get; set; }

        public string Banner { get; set; }

        [Display(Name = "Join Date")]
        public string JoinDate { get; set; }
    }
}
