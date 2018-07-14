using System;
using System.ComponentModel.DataAnnotations;

namespace TwitterCopy.Models
{
    public class TweetModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(280)]
        public string Text { get; set; }

        public string AuthorName { get; set; }

        public string AuthorSlug { get; set; }

        public int LikeCount { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime PostedOn { get; set; } = DateTime.Now;
    }
}
