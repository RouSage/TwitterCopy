using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterCopy.Models
{
    public class Tweet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(280)]
        public string Text { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public TwitterCopyUser User { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime PostedOn { get; set; }

        public int LikeCount { get; set; }

        public ICollection<Like> Likes { get; set; }

        public int RetweetCount { get; set; }

        public ICollection<Retweet> Retweets { get; set; }
    }
}
