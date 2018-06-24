using System;
using TwitterCopy.Areas.Identity;

namespace TwitterCopy.Models
{
    public class UserToUser
    {
        public Guid UserId { get; set; }
        public TwitterCopyUser User { get; set; }

        public Guid FollowerId { get; set; }
        public TwitterCopyUser Follower { get; set; }
    }
}
