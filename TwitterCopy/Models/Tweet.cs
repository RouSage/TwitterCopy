using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TwitterCopy.Areas.Identity;

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

        public int Likes { get; set; }

        public int Retweets { get; set; }
    }
}
