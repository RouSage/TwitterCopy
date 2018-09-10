using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TwitterCopy.Models
{
    public class TweetViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(280)]
        public string Text { get; set; }

        public string UserName { get; set; }

        public string Slug { get; set; }

        public string Avatar { get; set; }

        public int LikeCount { get; set; }

        public int RetweetCount { get; set; }

        public int ReplyCount { get; set; }

        public bool IsRetweet { get; set; }

        public DateTime RetweetDate { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime PostedOn { get; set; } = DateTime.Now;

        public List<TweetViewModel> RepliesFrom { get; set; } = new List<TweetViewModel>();
    }
}
