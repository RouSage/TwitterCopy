using System;

namespace TwitterCopy.Entities
{
    /// <summary>
    /// Implements Follower/Following system
    /// </summary>
    public class UserToUser
    {
        public Guid UserId { get; set; }
        public TwitterCopyUser User { get; set; }

        public Guid FollowerId { get; set; }
        public TwitterCopyUser Follower { get; set; }
    }
}
