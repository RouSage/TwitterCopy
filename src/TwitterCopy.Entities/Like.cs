using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterCopy.Entities
{
    public class Like
    {
        [Key]
        public int Id { get; set; }

        public int TweetId { get; set; }

        [ForeignKey("TweetId")]
        public Tweet Tweet { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public TwitterCopyUser User { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime DateLiked { get; set; } = DateTime.UtcNow;
    }
}
