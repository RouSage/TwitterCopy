using System;

namespace TwitterCopy.Core.Entities
{
    public class UserToUser
    {
        public Guid UserId { get; set; }

        public Guid FollowerId { get; set; }

        // Relationships

        public TwitterCopyUser User { get; set; }

        public TwitterCopyUser Follower { get; set; }
    }
}
