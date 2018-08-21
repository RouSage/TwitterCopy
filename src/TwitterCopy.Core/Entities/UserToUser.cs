using System;

namespace TwitterCopy.Core.Entities
{
    public class UserToUser
    {
        public Guid UserId { get; private set; }

        public Guid FollowerId { get; private set; }

        // Relationships

        public TwitterCopyUser User { get; private set; }

        public TwitterCopyUser Follower { get; private set; }

        // Ctors

        private UserToUser() { }

        public UserToUser(TwitterCopyUser user, TwitterCopyUser follower)
        {
            User = user;
            Follower = follower;
        }
    }
}
