using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace TwitterCopy.Core.Entities
{
    public class UserToUser
    {
        public Guid UserId { get; set; }

        public Guid FollowerId { get; set; }

        // Relationships

        [ForeignKey(nameof(UserId))]
        public TwitterCopyUser User { get; set; }

        [ForeignKey(nameof(FollowerId))]
        public TwitterCopyUser Follower { get; set; }
    }
}
